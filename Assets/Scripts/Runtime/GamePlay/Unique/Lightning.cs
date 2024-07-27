using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class Lightning : UniqueEffect
{
    [SerializeField] int damage;
    [SerializeField] float radius;
    [SerializeField] string sfxName;
    Collider[] hitColliders = new Collider[5];

    bool isStart;
    float timer;
    private void FixedUpdate()
    {
        if(IsServer && isStart)
        {
            if(Time.time >= timer + lifeTime)
            {
                CheckCollision();
                if (NetworkObject != null)
                {
                    NetworkObject.Despawn(true);
                }
            }
        }
    }
    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            SendClientDoEffectRpc(transform.position);
            timer = Time.time;
            isStart = true;
        }
    }
    void CheckCollision()
    {
        if (IsServer)
        {
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, radius, hitColliders, LayerMask.GetMask("PCs"));
            for (int i = 0; i < hitCount; i++)
            {
                var damageable = hitColliders[i].GetComponent<IDamageable>();
                if (damageable != null && damageable.IsDamageable())
                {
                    damageable.ReceiveHP(-damage);
                }
            }
        }
    }
    [Rpc(SendTo.ClientsAndHost)]
    void SendClientDoEffectRpc(Vector3 position)
    {
        var abilityFX = ParticlePool.Singleton.GetObject(effectPrefab, position + new Vector3(0,-0.08f,0), Quaternion.identity);
        abilityFX.GetComponent<SpecialFXGraphic>().OnInitialized(effectPrefab);
        AudioManager.Instance.PlaySFXAtPosition(sfxName, position);
        if(abilityFX.TryGetComponent(out VisualEffect VFX))
        {
            VFX.Play();
        }
    }
}
