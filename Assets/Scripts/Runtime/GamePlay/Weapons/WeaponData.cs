using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new weapon data",menuName = "GamePlay/WeaponData")]
public class WeaponData : ScriptableObject
{
    [SerializeField] string weaponName;
    [SerializeField] GameObject visual;
    [SerializeField] int amount;
    [SerializeField] Ability ability;
    [SerializeField] Sprite icon;

    public WeaponID Id;
    public string Name { get { return weaponName; } }
    public GameObject Visual { get { return visual; } }
    public int Amount { get { return amount; } }
    public Ability Ability { get {  return ability; } }

    public Sprite Icon { get { return icon; } }
}
