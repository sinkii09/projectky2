using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

[Serializable]
public enum BlockingModeType
{
    EntireDuration,
    OnlyDuringExecTime,
}
public abstract class Skill : ScriptableObject
{
    [NonSerialized]
    public SkillID SkillID;
    protected SkillRequestData m_Data;
    public ref SkillRequestData Data => ref m_Data;
    public float TimeStarted { get; set; }
    public float TimeRunning { get { return (Time.time - TimeStarted); } }

    public SkillConfig Config;

    public const string k_DefaultHitReact = "HitReact1";

    public void Initialize(ref SkillRequestData data)
    {
        m_Data = data;
        SkillID = data.SkillID;
    }
    public virtual void Reset()
    {
        m_Data = default;
        SkillID = default;
        TimeStarted = 0;
    }
    public abstract bool OnStart(ServerCharacter serverCharacter);
    public abstract bool OnUpdate(ServerCharacter serverCharacter);
    public virtual void End(ServerCharacter serverCharacter)
    {
        Cancel(serverCharacter);
    }
    public virtual void Cancel(ServerCharacter serverCharacter)
    {

    }
    public void DecreaseAmount(ServerCharacter serverCharacter)
    {
        if(serverCharacter.WeaponUseTimeAmount.Value<=1)
        {
            
            return;
        }
        serverCharacter.WeaponUseTimeAmount.Value--;
    }
    public virtual bool ShouldBecomeNonBlocking()
    {
        return Config.BlockingMode == BlockingModeType.OnlyDuringExecTime ? TimeRunning >= Config.ExecTimeSeconds : false;
    }
    public virtual bool ChainIntoNewSkill(ref SkillRequestData newSkill) { return false; }
    public virtual void CollisionEntered(ServerCharacter serverCharacter, Collision collision) { }

    public enum BuffableValue
    {
        PercentHealingReceived, // unbuffed value is 1.0. Reducing to 0 would mean "no healing". 2 would mean "double healing"
        PercentDamageReceived,  // unbuffed value is 1.0. Reducing to 0 would mean "no damage". 2 would mean "double damage"
        ChanceToStunTramplers,  // unbuffed value is 0. If > 0, is the chance that someone trampling this character becomes stunned
    }
    public virtual void BuffValue(BuffableValue buffType, ref float buffedValue) { }

    public static float GetUnbuffedValue(Skill.BuffableValue buffType)
    {
        switch (buffType)
        {
            case BuffableValue.PercentDamageReceived: return 1;
            case BuffableValue.PercentHealingReceived: return 1;
            case BuffableValue.ChanceToStunTramplers: return 0;
            default: throw new System.Exception($"Unknown buff type {buffType}");
        }
    }
    public enum GameplayActivity
    {
        AttackedByEnemy,
        Healed,
        StoppedChargingUp,
        UsingAttackAction,
    }

    public virtual void OnGameplayActivity(ServerCharacter serverCharacter, GameplayActivity activityType) { }

    public bool AnticipatedClient { get; protected set; }

    public virtual bool OnStartClient(ClientCharacter clientCharacter)
    {
        AnticipatedClient = false; 
        TimeStarted = Time.time;
        return true;
    }
    public virtual bool OnUpdateClient(ClientCharacter clientCharacter)
    {
        return true;
    }
    public virtual void EndClient(ClientCharacter clientCharacter)
    {
        CancelClient(clientCharacter);
    }
    public virtual void CancelClient(ClientCharacter clientCharacter) { }
    public virtual void OnAnimEventClient(ClientCharacter clientCharacter, string id) { }
    public virtual void OnStoppedChargingUpClient(ClientCharacter clientCharacter, float finalChargeUpPercentage) { }
    public virtual void AnticipateActionClient(ClientCharacter clientCharacter)
    {
        AnticipatedClient = true;
        TimeStarted = Time.time;

        if (!string.IsNullOrEmpty(Config.AnimAnticipation))
        {
            clientCharacter.OurAnimator.SetTrigger(Config.AnimAnticipation);
        }
    }
    public static bool ShouldClientAnticipate(ClientCharacter clientCharacter, ref SkillRequestData data)
    {
        if (!clientCharacter.CanPerformSkill) { return false; }

        var skillDescription = GamePlayDataSource.Instance.GetSkillPrototypeByID(data.SkillID).Config;

        bool isTargetEligible = true;
        if (data.ShouldClose == true)
        {
            ulong targetId = (data.TargetIds != null && data.TargetIds.Length > 0) ? data.TargetIds[0] : 0;
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out NetworkObject networkObject))
            {
                float rangeSquared = skillDescription.Range * skillDescription.Range;
                isTargetEligible = (networkObject.transform.position - clientCharacter.transform.position).sqrMagnitude < rangeSquared;
            }
        }
        return isTargetEligible && skillDescription.Logic != SkillLogic.Target;
    }
}
