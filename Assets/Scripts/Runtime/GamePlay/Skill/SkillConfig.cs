using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum SkillLogic
{
    Melee,
    LaunchProjectile,
    AoE,
    Target,
    Toss,
}

[Serializable]
public class SkillConfig
{
    public SkillLogic Logic;

    public int Damage;

    public int Amount;

    public float Range;

    public float DurationSeconds;

    public float ExecTimeSeconds;

    public float EffectDurationSeconds;

    public float ReuseTimeSeconds;

    public string AnimAnticipation;

    public string Anim;

    public string Anim2;

    public string ReactAnim;

    public string OtherAnimatorVariable;

    public float Radius;

    public bool ActionInterruptible;

    public List<Skill> IsInterruptableBy;

    public BlockingModeType BlockingMode;

    //public BaseActionInput ActionInput;

    public ProjectileInfo[] Projectiles;

    public WeaponInfo[] Spawns;

    public GameObject[] SpecialFX;

    public Sprite Icon;

    public bool CheckingAmount;

    public bool IsFriendly;
    public bool CanBeInterruptedBy(SkillID actionActionID)
    {
        foreach (var skill in IsInterruptableBy)
        {
            if (skill.SkillID == actionActionID)
            {
                return true;
            }
        }

        return false;
    }
}

