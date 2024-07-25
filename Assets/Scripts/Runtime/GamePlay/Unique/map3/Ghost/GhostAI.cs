using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class GhostAI : NetworkBehaviour
{
    private NavMeshAgent ghost;
    private Animator animator;

    private List<Transform> players; // List of players
    public float followRange = 20f; // Range within which the ghost will follow a player
    public float attackRange = 2f; // Range within which the ghost will attack the player

    private Vector3 moveDirection = Vector3.zero;
    private float directionChangeTimer = 0f;
    public float directionChangeInterval = 2f; // Interval to change direction

    private float stuckTimer = 0f;
    public float stuckThreshold = 5f; // Time threshold to determine if stuck
    public float minVelocity = 0.1f; // Minimum velocity to be considered not stuck

    // Dissolve
    [SerializeField] private SkinnedMeshRenderer [] MeshR;
    private float dissolveValue = 1;
    private bool dissolveFlg = false; // Flag to control dissolve effect

    private const int maxHP = 100;
    private int HP = maxHP;

    private GhostSpawner spawner;

    private Transform currentTarget;

    // Start is called before the first frame update
    void Start()
    {
        ghost = GetComponent<NavMeshAgent>();
        if (ghost == null)
        {
            Debug.LogError("NavMeshAgent component not found on " + gameObject.name);
        }

        animator = GetComponent<Animator>();

        // Find all players in the scene
        players = new List<Transform>();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (spawner != null)
        {
            spawner.RemoveGhost(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            NetworkManager.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;

        }

    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject playerObject in playerObjects)
        {
            players.Add(playerObject.transform);
        }
    }

    public override void OnNetworkDespawn()
    {
        if(IsServer)
        {
            players.Clear();
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(!IsServer && !IsSpawned) { return; }
        if(HP <= 0)
        {
            SetDissolveAnimation();
            return;
        }

        Transform closestPlayer = GetClosestPlayer();

        if(closestPlayer != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, closestPlayer.position);

            if(distanceToPlayer <= followRange)
            {
                currentTarget = closestPlayer;
                ghost.SetDestination(closestPlayer.position);

                if(distanceToPlayer <= attackRange)
                {
                    SetAttackAnimation();
                }
                else
                {
                    SetMoveAnimation();
                }

                // Check if the ghost is stuck
                if(ghost.velocity.magnitude < minVelocity)
                {
                    stuckTimer += Time.deltaTime;
                    if(stuckTimer >= stuckThreshold)
                    {
                        currentTarget = null;
                        Move();
                    }
                }
                else
                {
                    stuckTimer = 0f;
                }
            }
            else
            {
                currentTarget = null;
                Move();
            }
        }
        else
        {
            currentTarget = null;
            Move();
        }
    }

    void Move()
    {
        directionChangeTimer += Time.deltaTime;

        if(directionChangeTimer >= directionChangeInterval)
        {
            ChooseNewDirection();
        }

        // If there's no target, set moving animation
        if(currentTarget == null)
        {
            SetMoveAnimation();
        }

        ghost.SetDestination(transform.position + moveDirection * 5f);
    }

    void ChooseNewDirection()
    {
        // Choose a random direction
        moveDirection = Random.insideUnitSphere;
        moveDirection.y = 0; // Ensure the direction is horizontal
        directionChangeTimer = 0f;
    }

    Transform GetClosestPlayer()
    {
        Transform closestPlayer = null;
        float closestDistance = Mathf.Infinity;
        if(players.Count > 0)
        {
            foreach (Transform player in players)
            {
                if (player != null)
                {
                    float distance = Vector3.Distance(transform.position, player.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPlayer = player;
                    }
                }
            }

        }
        return closestPlayer;
    }

    void SetMoveAnimation()
    {
        animator.SetBool("isAttacking", false);
        animator.SetBool("isDissolving", false);
        animator.SetBool("isMoving", true); // Ensure moving animation is active
    }

    void SetAttackAnimation()
    {
        animator.SetBool("isAttacking", true);
        animator.SetBool("isMoving", false); // Ensure moving animation is inactive
    }

    void SetDissolveAnimation()
    {
        if(!dissolveFlg)
        {
            dissolveFlg = true; // Set the flag to true to indicate the dissolve effect is active
            animator.SetBool("isDissolving", true);
            StartCoroutine(DissolveEffect());
        }
    }

    IEnumerator DissolveEffect()
    {
        while(dissolveValue > 0)
        {
            dissolveValue -= Time.deltaTime / 2; // Adjust the dissolve speed as necessary
            foreach(var mesh in MeshR)
            {
                mesh.material.SetFloat("_DissolveAmount", dissolveValue);
            }
            yield return null;
        }

        gameObject.SetActive(false); // Deactivate the ghost after dissolving
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                SetAttackAnimation();
            }

        }
    }

    public void SetSpawner(GhostSpawner spawner)
    {
        this.spawner = spawner;
    }

}
