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

    int bounceAmount;
    int damage;
    float speed;
    float range;
    float timer;
    ServerCharacter spawner;
    Rigidbody rb;
    public void Initialize(in ProjectileInfo info, ServerCharacter serverCharacter)
    {

        damage = info.Damage;
        range = info.Range;
        speed = info.Speed;
        Debug.Log("speed = " + info.Speed);
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
            rb.velocity = transform.forward * 25;
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
            if (Time.time > timer + range / 10)
            {
                if (!NetworkObject)
                {
                    NetworkObject.Despawn();
                }
            }
        }

    }
    private void OnCollisionEnter(Collision collision)
    {
        if(!IsServer) return;
        if (((1 << collision.gameObject.layer) & blockLayer) != 0)
        {
            ClientHitEnemyRpc(transform.position);
            if (bounceAmount > maxBounceAmount && !NetworkObject)
            {
                NetworkObject.Despawn();
            }
            bounceAmount++;
        }
        if(((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            var serverCharacter = collision.gameObject.GetComponent<ServerCharacter>();
            if (serverCharacter != null && serverCharacter.OwnerClientId == spawner.OwnerClientId) return;
            var damageable = collision.gameObject.GetComponent<IDamageable>();
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
