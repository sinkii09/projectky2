using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CurvedProjectile : NetworkBehaviour
{
    [SerializeField] GameObject visual;
    [SerializeField] SphereCollider sphereCollider;
    [SerializeField] GameObject m_hitParticle;
    private Vector3 startPoint;
    private Vector3 controlPoint;
    private Vector3 endPoint;
    public float speed;
    private float t;
    private ProjectileInfo projectileInfo;
    private ServerCharacter spawner;
    private float totalDistance;
    public float explosionRadius = 3f;
    public LayerMask targetLayer;
    LayerMask blockLayer;
    private Collider[] hitColliders = new Collider[10];
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();

    bool isDead;
    public void Initialize(Vector3 start, Vector3 control, Vector3 end, in ProjectileInfo info,ServerCharacter serverCharacter = null)
    {
            startPoint = start;
            controlPoint = control;
            endPoint = end;
            speed = info.Speed;
            t = 0;
            projectileInfo = info;
            spawner = serverCharacter;
            totalDistance = CalculateTotalDistance();

            targetLayer = 1 << LayerMask.NameToLayer("PCs");
            blockLayer = LayerMask.GetMask(new[] { "PCs", "Environment" });
            isDead = false;
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            networkPosition.Value = startPoint;
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
    void Update()
    {
        if (IsServer)
        {
            if (t < 1)
            {
                t += Time.deltaTime * speed / totalDistance;
                transform.position = CalculateBezierPoint(t, startPoint, controlPoint, endPoint);
            }
            else
            {
                OnProjectileHit();
            }    
        }
        if(IsClient)
        {
             visual.transform.position = transform.position;
        }
    }
    private Vector3 CalculateBezierPoint(float t, Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 position = uu * startPoint; // u^2 * P0
        position += 2 * u * t * controlPoint; // 2 * u * t * P1
        position += tt * endPoint; // t^2 * P2
        return position;
    }
    private float CalculateTotalDistance(int segments = 20)
    {
        float distance = 0f;
        Vector3 previousPoint = startPoint;

        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector3 currentPoint = CalculateBezierPoint(t, startPoint, controlPoint, endPoint);
            distance += Vector3.Distance(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        return distance;
    }
    private void OnProjectileHit()
    {
        isDead = true;
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, hitColliders, targetLayer);
        for (int i = 0; i < hitCount; i++)
        {
            var serverCharacter = hitColliders[i].GetComponent<ServerCharacter>();
            if (serverCharacter != null && serverCharacter.OwnerClientId == spawner.OwnerClientId) continue;
            var damageable = hitColliders[i].GetComponent<IDamageable>();
            if (damageable != null && damageable.IsDamageable())
            {
                damageable.ReceiveHP(-projectileInfo.Damage, spawner);
            }
        }
        ExplosionClientRpc();
        NetworkObject.Despawn();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(isDead) return;
        if(other.gameObject.layer != blockLayer) return;
        if(other.gameObject.TryGetComponent(out ServerCharacter character))
        {
            if (character.OwnerClientId == spawner.OwnerClientId) return;
        }

        OnProjectileHit();

    }
    [Rpc(SendTo.ClientsAndHost)]
    void ExplosionClientRpc()
    {
        if (m_hitParticle)
        {
            var hitPart = ParticlePool.Singleton.GetObject(m_hitParticle, transform.position, transform.rotation);
            hitPart.GetComponent<SpecialFXGraphic>().OnInitialized(m_hitParticle);
        }
    }
}
