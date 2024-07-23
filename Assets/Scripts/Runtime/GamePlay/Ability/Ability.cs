using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

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
    public GameObject indicatorFX;
    public Texture inputIndicatorTexture;
    public Sprite AbilityIcon;
    public bool IsSpecialAbility = false;
    public bool CheckAmount = true;
    public bool ShowIndicator = true;
    public string SFX_Name;
    public float TimeStarted {  get; set; }
    public float TimeRunning { get { return (Time.time - TimeStarted); } }
    
    public abstract void Activate(ServerCharacter serverCharacter, AbilityRequest data);
    public virtual bool CanActivate(ServerCharacter serverCharacter)
    {
        if(IsSpecialAbility && Cost > 0)
        {
            if(serverCharacter.ManaPoint.Value >= Cost)
            {
                serverCharacter.ReceiveMP(-Cost);
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (Time.time - TimeStarted > cooldownTime)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public virtual void OnAbilityUpdate(ServerCharacter serverCharacter)
    {

    }
    public virtual void OnReset()
    {
        
    }
    public virtual void OnPlayEffectClient(ClientCharacter clientCharacter,Vector3 position,Quaternion rotation,int num = 0) { }
    public virtual void OnPlaySFXClient(Vector3 position)
    {
        if(!string.IsNullOrEmpty(SFX_Name))
        {
            AudioManager.Instance.PlaySFXAtPosition(SFX_Name,position);
        }
    }
    public virtual void OnShowIndicatorClient(Vector3 position,float radius) 
    {
        if(!indicatorFX) { return; }
        var indicator = ParticlePool.Singleton.GetObject(indicatorFX, position, Quaternion.identity);
        indicator.GetComponent<SpecialFXGraphic>().OnInitialized(indicatorFX);
        if (indicator.TryGetComponent(out VisualEffect effect))
        {
            effect.SetFloat("Diameter", radius*2);
            effect.Play();
        }
    }
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
