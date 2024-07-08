using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class BoomerangProjectile : NetworkBehaviour
{

    [SerializeField] GameObject visual;
    [SerializeField] GameObject m_hitParticle;

    Vector3 startPoint;
    Vector3 endPoint;

    float speed;
    int damage;
    LayerMask targetLayer;
    LayerMask blockLayer;
    private ServerCharacter spawner;
    private bool returning;
    private bool collisionBlock;
    private float totalDistance;
    private float traveledDistance;

    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();

    private List<ServerCharacter> damagePlayer = new List<ServerCharacter>();
    

    public void Initialize(Vector3 start, Vector3 end, in ProjectileInfo info, ServerCharacter serverCharacter)
    {
            startPoint = start;
            endPoint = end;
            
            speed = info.Speed;
            damage = info.Damage;
            returning = false;
            collisionBlock = false;
            traveledDistance = 0;
            spawner = serverCharacter;
            totalDistance = Vector3.Distance(startPoint, endPoint);
            targetLayer = 1 << LayerMask.NameToLayer("PCs");
            blockLayer = LayerMask.GetMask("Environment");
        
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            networkPosition.Value = startPoint;
        }
        else
        {
            visual.transform.parent = null;
        }
    }

    public override void OnNetworkDespawn()
    {
        if(IsClient)
        {
            visual.transform.parent = transform;
        }
        damagePlayer.Clear();
    }

    void Update()
    {
        
        if (IsServer)
        {
            MoveBoomerang();  
        }
        else
        {
            visual.transform.position = transform.position;
            visual.transform.Rotate(Vector3.forward*20);
        }

    }
    private void MoveBoomerang()
    {
        if (spawner == null)
        {
            return;
        }

        float step = speed * Time.deltaTime;

        if (!returning)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPoint, step);
            traveledDistance += step;

            if (Vector3.Distance(transform.position, endPoint) < 0.1f || traveledDistance >= totalDistance || collisionBlock)
            {
                returning = true;
                damagePlayer.Clear();
            }
            
        }
        else
        {
            
            transform.position = Vector3.MoveTowards(transform.position, spawner.transform.position, step);

            if (Vector3.Distance(transform.position, spawner.transform.position) < 0.1f)
            {
                OnBoomerangReturn();
            }
        }

    }
    void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
        {
            return;
        }
        if(!collisionBlock && other.gameObject.layer == blockLayer)
        {
            Debug.Log("boom hit wall");
            collisionBlock = true;
        }
        if (other.gameObject.layer != targetLayer) return;
        var serverCharacter = other.GetComponent<ServerCharacter>();
        if (serverCharacter != null && serverCharacter.OwnerClientId == spawner.OwnerClientId) return;
        if (damagePlayer.Contains(serverCharacter)) return;
        var damageable = other.GetComponent<IDamageable>();
        if (damageable != null && damageable.IsDamageable())
        {
            ClientHitEnemyRpc(serverCharacter.OwnerClientId);
            damageable.ReceiveHP(-damage, spawner);
            damagePlayer.Add(serverCharacter);
            Debug.Log("Add player");
        }
    }
    private void OnBoomerangReturn()
    {
        NetworkObject.Despawn();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ClientHitEnemyRpc(ulong enemyId)
    {
        NetworkObject targetNetObject;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyId, out targetNetObject))
        {
            if (m_hitParticle)
            {
                var hitPart = ParticlePool.Singleton.GetObject(m_hitParticle, transform.position, transform.rotation);
                hitPart.GetComponent<SpecialFXGraphic>().OnInitialized(m_hitParticle);
            }
        }
    }
}
