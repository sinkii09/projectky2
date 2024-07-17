using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BouncingBullet : NetworkBehaviour
{
    [SerializeField] GameObject visual;
    [SerializeField] GameObject hitFx;
    [SerializeField] int maxBounceAmount;
    LayerMask targetLayer;
    LayerMask blockLayer;

    Vector3 startPos;
    Vector3 direction;
    int bounceAmount;
    int damage;
    float speed;
    float range;
    float timer;
    ServerCharacter spawner;

    public void Initialize(in ProjectileInfo info, ServerCharacter serverCharacter)
    {
        direction = transform.forward;
        damage = info.Damage;
        range = info.Range;
        speed = info.Speed;
        Debug.Log($"range {range} && speed {speed}");
        spawner = serverCharacter;

        bounceAmount = 0;

        targetLayer = LayerMask.GetMask("PCs");
        blockLayer = LayerMask.GetMask("Environment");
    }
    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            timer = Time.time;
            startPos = transform.position;
        }
        if(IsClient)
        {
            visual.transform.parent = null;
        }
    }
    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            visual.transform.parent = transform;
        }
    }
    private void Update()
    {
        if(IsClient)
        {
            visual.transform.position = transform.position;
        }
    }
    private void FixedUpdate()
    {
        if(IsServer)
        {
            if (Time.time - timer > (range / speed))
            {
                if (!NetworkObject)
                {
                    NetworkObject.Despawn();
                }
            }
            RaycastCheck();
        }

    }
    void RaycastCheck()
    {
        float distanceToMove = speed * Time.fixedDeltaTime;
        Ray ray = new Ray(transform.position,transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distanceToMove, blockLayer))
        {
            transform.position = hit.point;
            direction = new Vector3(Vector3.Reflect(transform.forward, hit.normal).x,direction.y, Vector3.Reflect(transform.forward, hit.normal).z);
            ClientHitEnemyRpc(transform.position);
            bounceAmount++;
            if (bounceAmount >= maxBounceAmount)
            {
                if (!NetworkObject)
                {
                    NetworkObject.Despawn();
                }
            }
        }
        else
        {
            transform.position += direction * distanceToMove;
            //if (Vector3.Distance(transform.position, startPos) > range)
            //{
            //    if (!NetworkObject)
            //    {
            //        NetworkObject.Despawn();
            //    }
            //}
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            var serverCharacter = other.gameObject.GetComponent<ServerCharacter>();
            if (serverCharacter != null && serverCharacter.OwnerClientId == spawner.OwnerClientId) return;
            var damageable = other.gameObject.GetComponent<IDamageable>();
            if (damageable != null && damageable.IsDamageable())
            {
                ClientHitEnemyRpc(transform.position);
                damageable.ReceiveHP(-damage, spawner);
                NetworkObject.Despawn();
            }
        }
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void ClientHitEnemyRpc(Vector3 position)
    {
        if (hitFx)
        {
            var hitPart = ParticlePool.Singleton.GetObject(hitFx, position, transform.rotation);
            hitPart.GetComponent<SpecialFXGraphic>().OnInitialized(hitFx);
        }
    }
}
