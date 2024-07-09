using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoBarUI : NetworkBehaviour
{
    [SerializeField] Slider healthbarUI;
    [SerializeField] Image weaponIcon;
    [SerializeField] TextMeshProUGUI playerName;

    [SerializeField] ServerCharacter serverCharacter;

    private bool invert = true;
    private void Update()
    {
        if(!IsSpawned) return;
        if(!IsClient) return;
        healthbarUI.value = Mathf.LerpUnclamped(healthbarUI.value, serverCharacter.NetHealthState.HitPoints.Value, .1f);
    }
    private void LateUpdate()
    {
        if(Camera.main == null) return;
        if(invert)
        {
            Vector3 dirToCamera = (Camera.main.transform.position - transform.position).normalized;
            transform.LookAt(transform.position + dirToCamera * -1);
        }
        else
        {
            transform.LookAt(Camera.main.transform);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsClient)
        {
            enabled = false;
            return;
        }
        Debug.Log("HP bar spawned");
        serverCharacter ??= GetComponentInParent<ServerCharacter>();
        healthbarUI.maxValue = serverCharacter.HitPoints;
        healthbarUI.value = healthbarUI.maxValue;
        var currentWeapon = GamePlayDataSource.Instance.GetWeaponPrototypeByID(serverCharacter.CurrentWeaponId.Value);
        weaponIcon.sprite = currentWeapon.Icon;
        playerName.text = NetworkServer.Instance.GetNetworkedPlayer(serverCharacter.OwnerClientId).PlayerName.Value;


        serverCharacter.CurrentWeaponId.OnValueChanged += OnWeaponChange;
        serverCharacter.NetHealthState.HitPoints.OnValueChanged += OnHealthChange;
    }

    public override void OnNetworkDespawn()
    {
        serverCharacter.CurrentWeaponId.OnValueChanged -= OnWeaponChange;
        serverCharacter.NetHealthState.HitPoints.OnValueChanged -= OnHealthChange;
    }

    
    private void OnHealthChange(int previousValue, int newValue)
    {
        
    }

    private void OnWeaponChange(WeaponID previousValue, WeaponID newValue)
    {
        weaponIcon.sprite = GamePlayDataSource.Instance.GetWeaponPrototypeByID(newValue).Icon;
    }

}
