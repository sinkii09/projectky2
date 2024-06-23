using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WeaponItem : NetworkBehaviour
{
    const float k_DespawnTime = 5;
    bool m_CanPickup;
    
    WeaponID m_ID;
    int m_Amount;
    Skill m_Skill;
    public Skill Skill => m_Skill;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }
        m_CanPickup = true;
        UpdateVisualClientRpc(m_ID);
    }
    public void Initialize(WeaponID weaponID)
    {
        WeaponData weaponData = GamePlayDataSource.Instance.GetWeaponPrototypeByID(weaponID);
        m_ID = weaponID;
        m_Amount = weaponData.Amount;
        StartCoroutine(DeSpawnOverTime());
        
    }
    IEnumerator DeSpawnOverTime()
    {
        yield return new WaitForSeconds(k_DespawnTime);
        if (NetworkObject != null)
        {
            NetworkObject.Despawn(true);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (other.gameObject.TryGetComponent(out ServerCharacter serverCharacter))
        {
            if(m_CanPickup)
            {
                m_CanPickup = false;
                serverCharacter.CurrentWeaponId.Value = m_ID;
                serverCharacter.WeaponUseTimeAmount.Value = m_Amount; 
                NetworkObject.Despawn();
            }
        }
    }
    [Rpc(SendTo.ClientsAndHost,RequireOwnership = false)]
    void UpdateVisualClientRpc(WeaponID weaponId)
    {
        var weaponData = GamePlayDataSource.Instance.GetWeaponPrototypeByID(weaponId);
        Instantiate(weaponData.Visual,transform);

    }
}
