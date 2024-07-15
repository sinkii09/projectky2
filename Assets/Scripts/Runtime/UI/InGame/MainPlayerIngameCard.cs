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
    [SerializeField]
    private TMP_Text weaponNameText;

    [SerializeField]
    private TMP_Text manaText;
    [SerializeField]
    private TMP_Text manaCostText;
    [SerializeField]
    private Image specialIcon;

    [SerializeField]
    public Counter counter;
    public void UpdateBaseAttackWeapon(WeaponID weaponID)
    {
        var weapon = GamePlayDataSource.Instance.GetWeaponPrototypeByID(weaponID);
        weaponIcon.sprite = weapon.Ability.AbilityIcon;
        weaponNameText.text = weapon.Name;
    }
    public void UpdateSpecial(CharacterStats stats)
    {
        specialIcon.sprite = stats.SpecialAbility.AbilityIcon;
        manaCostText.text = stats.SpecialAbility.Cost.ToString();
    }
    public void UpdateCurrentMana(int value)
    {
        manaText.text = "MP: " + value.ToString();
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
