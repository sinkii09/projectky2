using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PhysicsProjectile : NetworkBehaviour
{
    const int k_MaxCollisions = 99;
    const float k_WallLingerSec = 2f;
    const float k_EnemyLingerSec = 0.2f;
    const float k_LerpTime = 0.1f;

    [SerializeField]
    protected SphereCollider m_OurCollider;

    //[SerializeField]
    //protected SpecialFXGraphic m_OnHitParticlePrefab;

    [SerializeField]
    protected Transform m_Visualization;

    [SerializeField]
    protected TrailRenderer m_TrailRenderer;
    [SerializeField]
    protected GameObject m_hitParticle;
    protected bool m_Started;
    protected bool m_IsDead;

    protected ulong m_SpawnerId;

    protected ProjectileInfo m_ProjectileInfo;

    protected float m_DestroyAtSec;


    protected Collider[] m_CollisionCache = new Collider[k_MaxCollisions];

    int m_CollisionMask;
    int m_BlockerMask;
    protected int m_PCsLayer;

    protected List<GameObject> m_HitTargets = new List<GameObject>();


    public virtual void Initialize(ulong creatorsNetworkObjectId, in ProjectileInfo projectileInfo, Transform spawnerTransform)
    {
        m_SpawnerId = creatorsNetworkObjectId;
        m_ProjectileInfo = projectileInfo;
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            m_Started = true;
            m_IsDead = false;

            m_HitTargets = new List<GameObject>();
            m_DestroyAtSec = Time.fixedTime + (m_ProjectileInfo.Range / m_ProjectileInfo.Speed);

            m_CollisionMask = LayerMask.GetMask(new[] { "PCs", "Environment" });
            m_BlockerMask = LayerMask.GetMask(new[] {  "Environment" });
            m_PCsLayer = LayerMask.NameToLayer("PCs");
        }
        if (IsClient)
        {
            m_TrailRenderer.Clear();

            m_Visualization.parent = null;
            m_Visualization.transform.rotation = transform.rotation;
            //   m_PositionLerper = new PositionLerper(transform.position, k_LerpTime);
        }
    }

    public override void OnNetworkDespawn()
    {
        if(IsServer)
        {
            m_Started = false;
        }

        if (IsClient)
        {
            m_TrailRenderer.Clear();
            m_Visualization.parent = transform;
        }
    }
    protected virtual void Update()
    {
        if(IsClient)
        {

                m_Visualization.position = transform.position;
            
        }
    }

    protected virtual void FixedUpdate()
    {
        if (!m_Started || !IsServer)
        {
            return;
        }
        if(m_DestroyAtSec < Time.fixedTime)
        {
            var networkObject = gameObject.GetComponent<NetworkObject>();
            networkObject.Despawn();
            return;
        }
        var displacement = transform.forward * (m_ProjectileInfo.Speed * Time.fixedDeltaTime);
        transform.position += displacement;

        if (!m_IsDead)
        {
            DetectCollisions();
        }
    }
    protected virtual void DetectCollisions()
    {
        var position = transform.localToWorldMatrix.MultiplyPoint(m_OurCollider.center);
        var numCollisions = Physics.OverlapSphereNonAlloc(position, m_OurCollider.radius, m_CollisionCache, m_CollisionMask);
        for (int i = 0; i < numCollisions; i++)
        {
            int layerTest = 1 << m_CollisionCache[i].gameObject.layer;

            if ((layerTest & m_BlockerMask) != 0)
            {
                m_ProjectileInfo.Speed = 0;
                m_IsDead = true;
                m_DestroyAtSec = Time.fixedTime + k_WallLingerSec;
                return;
            }
            if (m_CollisionCache[i].gameObject.GetComponent<ServerCharacter>().OwnerClientId == m_SpawnerId) continue;
            if (m_CollisionCache[i].gameObject.layer == m_PCsLayer && !m_HitTargets.Contains(m_CollisionCache[i].gameObject))
            {
                m_HitTargets.Add(m_CollisionCache[i].gameObject);

                if (m_HitTargets.Count >= m_ProjectileInfo.MaxVictims)
                {
                    m_DestroyAtSec = Time.fixedTime + k_EnemyLingerSec;
                    m_IsDead = true;
                }

                var targetNetObj = m_CollisionCache[i].GetComponentInParent<NetworkObject>();
                if (targetNetObj)
                {
                    ClientHitEnemyRpc(targetNetObj.NetworkObjectId);

                    NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(m_SpawnerId, out var spawnerNet);
                    var spawnerObj = spawnerNet != null ? spawnerNet.GetComponent<ServerCharacter>() : null;
                    
                    if (m_CollisionCache[i].TryGetComponent(out IDamageable damageable))
                    {
                        damageable.ReceiveHP(-m_ProjectileInfo.Damage, spawnerObj);
                    }
                }

                if (m_IsDead)
                {
                    return; 
                }
            }
        }
    }
    [Rpc(SendTo.ClientsAndHost)]
    protected virtual void ClientHitEnemyRpc(ulong enemyId)
    {
        NetworkObject targetNetObject;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyId, out targetNetObject))
        {
            if(m_hitParticle)
            {
                var hitPart = ParticlePool.Singleton.GetObject(m_hitParticle,transform.position,transform.rotation);
                hitPart.GetComponent<SpecialFXGraphic>().OnInitialized(m_hitParticle);
            }
        }
    }
}
