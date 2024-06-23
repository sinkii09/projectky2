using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponSpawner : NetworkBehaviour
{
    [SerializeField] WeaponItem weaponPrefab;
    [SerializeField] WeaponData baseAttackData;
    [SerializeField] private float m_TimeBetweenSpawn = 1f;
    [SerializeField] private float m_TimeBetweenWave = 3f;
    [SerializeField] private float m_DelayStartTime = 30f;
    [SerializeField] private float m_SpawnRange = 10f;


    LayerMask k_EvenvironmentLayerMask;
    LayerMask k_ItemLayerMask;
    List<WeaponItem> weaponItems = new List<WeaponItem>();
    List<WeaponData> weaponsData = new List<WeaponData>();

    private void Awake()
    {
        
    }
    void GetAllWeaponExcept()
    {
        foreach (var weaponData in GamePlayDataSource.Instance.GetAllWeapon())
        {
            if(weaponData.Id != baseAttackData.Id)
            {
                weaponsData.Add(weaponData);
            }
        }
    }
    public override void OnNetworkSpawn()
    {
        if(!IsServer)
        {
            enabled = false;
            return;
        }
        k_EvenvironmentLayerMask = LayerMask.GetMask(new[] { "Environment" });
        k_ItemLayerMask = LayerMask.GetMask(new[] { "Item" });
        GetAllWeaponExcept();
        StartCoroutine(SpawnWaves());
    }
    IEnumerator SpawnWaves()
    {
        yield return null;
        while(!GamePlayBehaviour.Instance.IsGameOver)
        {
            yield return SpawnAWave();
            yield return new WaitForSeconds(m_TimeBetweenWave);
        }
    }
    IEnumerator SpawnAWave()
    {
        foreach (var player in NetworkServer.Instance.m_ClientData)
        {
            var newSpawn = SpawnWeapon();
            weaponItems.Add(newSpawn);
            yield return new WaitForSeconds(m_TimeBetweenSpawn);
        }
    }
    WeaponItem SpawnWeapon()
    {
        int idx = Random.Range(0, weaponsData.Count );
        WeaponItem clone = Instantiate(weaponPrefab,RandomPositionInRange(),Quaternion.identity);
        clone.Initialize(weaponsData[idx].Id);
        if(!clone.NetworkObject.IsSpawned)
        {
            clone.NetworkObject.Spawn();
        }
        return clone;
    }
    Vector3 RandomPositionInRange()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 newPosition = transform.position + new Vector3(Mathf.Cos(angle) * m_SpawnRange, 1, Mathf.Sin(angle) * m_SpawnRange);
        if (!IsAvailablePosion(newPosition))
        {
            RandomPositionInRange();
        }
        return newPosition;
    }
    bool IsAvailablePosion(Vector3 position)
    {
        if (Physics.CheckSphere(position, 5f, k_ItemLayerMask))
        {
            return false;
        }
        if (Physics.Raycast(position, Vector3.up, 100f, k_EvenvironmentLayerMask))
        {
            return false;
        }
        return true;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, m_SpawnRange);
    }
}
