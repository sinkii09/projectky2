using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string abilityName;

    public int Damage;
    public int Radius;

    public float MinRange;
    public float MaxRange;

    public string abilityAnimationTrigger;
    public string rejectionAnimationTrigger;

    public float executeTime;
    public float durationTime;
    public float cooldownTime;

    public int Cost;

    public ProjectileInfo[] projectileInfoList;
    public GameObject[] weaponVisual;
    public GameObject[] effect;
    public Texture indicatorTexture;
    public bool IsSpecialAbility = false;
    public bool CheckAmount = true;
    public bool ShowIndicator = true;
    public float TimeStarted {  get; set; }
    public float TimeRunning { get { return (Time.time - TimeStarted); } }
    
    public abstract void Activate(ServerCharacter serverCharacter, AbilityRequest data);
    public abstract bool CanActivate(ServerCharacter serverCharacter);
    public virtual void OnAbilityUpdate(ServerCharacter serverCharacter)
    {

    }
    public virtual void OnReset()
    {
        
    }
    public virtual void OnPlayClient(ClientCharacter clientCharacter,Vector3 position,Quaternion rotation,int num = 0) { }

    protected IEnumerator ExecuteTimeDelay()
    {
        yield return new WaitForSeconds(executeTime);
    }
    protected IEnumerator DurationTimeDelay()
    {
        yield return new WaitForSeconds(durationTime);
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
