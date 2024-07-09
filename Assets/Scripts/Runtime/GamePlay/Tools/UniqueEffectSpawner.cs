using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class UniqueEffectSpawner : NetworkBehaviour
{
    [SerializeField] private float spawnRange = 30f;
    [SerializeField] private float duration = 5f;
    [SerializeField] private int amount = 2;

    [SerializeField] UniqueEffect[] effects;
    [SerializeField] GameObject weatherFx;

    LayerMask k_EvenvironmentLayerMask;
    float timer;
    private void Awake()
    {
        weatherFx.SetActive(false);
    }
    private void FixedUpdate()
    {
        if (!IsSpawned) return;
        if(!IsServer) return;
        if(Time.time > timer+duration)
        {
            timer = Time.time;
            SpawnEffect();
        }
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            timer = Time.time;
            k_EvenvironmentLayerMask = LayerMask.GetMask(new[] { "Environment" });
        }
        if(IsClient)
        {
            weatherFx.SetActive(true);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }
    
    void SpawnEffect()
    {
        for(int i = 0; i < amount; i++)
        {
            int idx = Random.Range(0, effects.Length);
            var effect = Instantiate(effects[idx]);
            effect.Initialize(RandomPositionInRange());
            if (!effect.NetworkObject.IsSpawned)
            {
                effect.NetworkObject.Spawn();
            }
        }
    }
    Vector3 RandomPositionInRange()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 newPosition = transform.position + new Vector3(Mathf.Cos(angle) * spawnRange, 0, Mathf.Sin(angle) * spawnRange);
        if (!IsAvailablePosion(newPosition))
        {
            RandomPositionInRange();
        }
        return newPosition + Vector3.up * 2;
    }
    bool IsAvailablePosion(Vector3 position)
    {
        if (Physics.Raycast(position + Vector3.up * 100, Vector3.down, 100f, k_EvenvironmentLayerMask))
        {
            return false;
        }
        return true;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, spawnRange);
    }
}
