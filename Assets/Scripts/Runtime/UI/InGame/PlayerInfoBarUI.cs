using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoBarUI : MonoBehaviour
{
    [SerializeField] Slider healthbarUI;
    [SerializeField] Image weaponIcon;
    [SerializeField] TextMeshProUGUI playerName;
    [SerializeField] Image fillImage;
    [SerializeField] ServerCharacter serverCharacter;

    private bool isStart;
    private void Update()
    {
        if (!isStart) return;
        healthbarUI.value = Mathf.LerpUnclamped(healthbarUI.value, serverCharacter.NetHealthState.HitPoints.Value, .1f);
    }
    //private void LateUpdate()
    //{
    //    if(invert)
    //    {
    //        Vector3 dirToCamera = (Camera.main.transform.position - transform.position).normalized;
    //        transform.LookAt(transform.position + dirToCamera * -1);
    //    }
    //    else
    //    {
    //        transform.LookAt(Camera.main.transform);
    //    }
    //}
    private void LateUpdate()
    {    
        if(Camera.main == null) return;
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                             Camera.main.transform.rotation * Vector3.up);
        }
    }
    public void Init(string name, bool isOwner = false)
    {
        isStart = true;
        serverCharacter ??= GetComponentInParent<ServerCharacter>();
        healthbarUI.maxValue = serverCharacter.HitPoints;
        healthbarUI.value = healthbarUI.maxValue;
        var currentWeapon = GamePlayDataSource.Instance.GetWeaponPrototypeByID(serverCharacter.CurrentWeaponId.Value);
        weaponIcon.sprite = currentWeapon.Ability.AbilityIcon;
        fillImage.color = isOwner ? Color.white : Color.red;
        playerName.text = name;


        serverCharacter.CurrentWeaponId.OnValueChanged += OnWeaponChange;
        serverCharacter.NetHealthState.HitPoints.OnValueChanged += OnHealthChange;
    }

    private void OnDestroy()
    {
        serverCharacter.CurrentWeaponId.OnValueChanged -= OnWeaponChange;
        serverCharacter.NetHealthState.HitPoints.OnValueChanged -= OnHealthChange;
    }
    private void OnHealthChange(int previousValue, int newValue)
    {
        
    }

    private void OnWeaponChange(WeaponID previousValue, WeaponID newValue)
    {
        weaponIcon.sprite = GamePlayDataSource.Instance.GetWeaponPrototypeByID(newValue).Ability.AbilityIcon;
    }

}
