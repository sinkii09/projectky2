using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UniqueEffect : NetworkBehaviour
{
    [SerializeField] protected float lifeTime;
    [SerializeField] protected float effectDuration;

    [SerializeField] protected GameObject effectPrefab;

    public virtual void Initialize(Vector3 position)
    {
        transform.position = position;
    }
    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            StartCoroutine(DeSpawnOverTime());
        }
        
    }
    public override void OnNetworkDespawn()
    {

    }

    IEnumerator DeSpawnOverTime()
    {
        yield return new WaitForSeconds(lifeTime);
        if (NetworkObject != null)
        {
            NetworkObject.Despawn(true);
        }
    }
}
