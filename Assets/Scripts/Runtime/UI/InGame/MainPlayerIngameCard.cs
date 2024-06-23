using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainPlayerIngameCard : PlayerIngameCard
{
    [SerializeField]
    private Image weaponIcon;
    [SerializeField]
    private TMP_Text weaponAmountText;
    public void UpdateBaseAttackWeapon(WeaponID weaponID)
    {
        var weapon = GamePlayDataSource.Instance.GetWeaponPrototypeByID(weaponID);
        weaponIcon.sprite = weapon.Icon;
    }

    internal void UpdateWeaponAmount(bool isBaseAttack, int newValue = 0)
    {
        if(!isBaseAttack)
        {
            weaponAmountText.enabled = true;
            weaponAmountText.text = newValue.ToString();
        }
        else
        {
            weaponAmountText.enabled = false;
        }
    }
}
