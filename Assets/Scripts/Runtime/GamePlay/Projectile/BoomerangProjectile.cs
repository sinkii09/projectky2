using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class BoomerangProjectile : NetworkBehaviour
{

    [SerializeField] GameObject visual;

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
    

    public void Initialize(Vector3 start, Vector3 end, ProjectileInfo info, ServerCharacter serverCharacter)
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
            blockLayer = 1 << LayerMask.NameToLayer("Environment");
            Debug.Log($"Boomerang initialized with start: {startPoint}, end: {endPoint}, speed: {speed}, damage: {damage}, distance: {totalDistance}, servercharacter : {serverCharacter.OwnerClientId}");
        
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
            Debug.LogError("Spawner is null.");
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
                Debug.Log("Boomerang returning to spawner.");
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
            collisionBlock = true;
        }
        if (other.gameObject.layer != targetLayer) return;
        var serverCharacter = other.GetComponent<ServerCharacter>();
        if (serverCharacter != null && serverCharacter.OwnerClientId == spawner.OwnerClientId) return;
        if (damagePlayer.Contains(serverCharacter)) return;
        var damageable = other.GetComponent<IDamageable>();
        if (damageable != null && damageable.IsDamageable())
        {
            damageable.ReceiveHP(-damage, spawner);
            damagePlayer.Add(serverCharacter);
        }
    }
    private void OnBoomerangReturn()
    {
        NetworkObject.Despawn();
    }
}
