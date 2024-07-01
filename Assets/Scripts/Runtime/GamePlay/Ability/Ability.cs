using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string abilityName;

    public int Damage;

    public float MinRange;
    public float MaxRange;

    public string abilityAnimationTrigger;
    public string rejectionAnimationTrigger;

    public float executeTime;

    public ProjectileInfo[] projectileInfoList;
    public bool CheckAmount = true;
    public abstract void Activate(ServerCharacter serverCharacter, AbilityRequest data);
    public abstract bool CanActivate(ServerCharacter serverCharacter);
    protected IEnumerator ExecuteTimeDelay()
    {
        yield return new WaitForSeconds(executeTime);
    }
    protected void DecreaseAmount(ServerCharacter serverCharacter)
    {
        if (serverCharacter.WeaponUseTimeAmount.Value <= 0)
        {
            serverCharacter.CurrentWeaponId.Value = serverCharacter.CharacterStats.WeaponData.Id;
            return;
        }
        serverCharacter.WeaponUseTimeAmount.Value--;
    }
}
