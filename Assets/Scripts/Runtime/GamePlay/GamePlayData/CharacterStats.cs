using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Character Stats", menuName = "Characters/Stats")]
public class CharacterStats : ScriptableObject
{
    public Skill BaseAttack;
    public Skill SpecialSkill;
    public WeaponData WeaponData;
    public int BaseHP;
    public int Speed;
}
