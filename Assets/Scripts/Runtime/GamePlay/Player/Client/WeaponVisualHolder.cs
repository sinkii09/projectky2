using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponVisualHolder : MonoBehaviour
{
    [SerializeField] Transform handSlot;
    [SerializeField] GameObject baseWeapon;
    [SerializeField] GameObject Axe;
    [SerializeField] GameObject CrossBow;
    [SerializeField] GameObject Zapper;
    [SerializeField] GameObject Blaster;
    public void UpdateWeaponVisual(WeaponID weaponID)
    {
        foreach (Transform child in handSlot)
        {
            child.gameObject.SetActive(false);
        }
        switch(weaponID.ID)
        {
            case 0:
                CrossBow.SetActive(true);
                break;
            case 1:
                if(baseWeapon)
                baseWeapon.SetActive(true);
                break; 
            case 2:
                Zapper.SetActive(true);
                break;
            case 3:
                Axe.SetActive(true);
                break;
            case 4:
                Blaster.SetActive(true);
                break;
            default: 
                break;


        }
    }
}
