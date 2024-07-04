using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum SkillTriggerStyle
{
    None,
    MouseClick,
    Keyboard,
    KeyboardRelease,
    UI,
    UIRelease,
}
public enum AttackType
{
    BaseAttack,
    SpecialAbility    
}
public class ServerInputReceiver : NetworkBehaviour
{
    #region Var
    bool IsReleaseStyle(SkillTriggerStyle style)
    {
        return style == SkillTriggerStyle.KeyboardRelease || style == SkillTriggerStyle.UIRelease;
    }

    struct SkillRequest
    {
        public Vector3 Position;
        public AttackType AttackType;
        public SkillTriggerStyle TriggerStyle;
        public WeaponID RequestedSkillID;
        public ulong TargetId;
    }
    readonly SkillRequest[] m_SkillRequests = new SkillRequest[1];
    int m_SkillRequestsCount;
    #endregion

    #region Ref
    [SerializeField]
    ServerCharacter m_ServerCharacter;

    [SerializeField]
    PhysicsWrapper m_PhysicsWrapper;
    CharacterStats m_CharacterStats => m_ServerCharacter.CharacterStats;
    #endregion

    [Rpc(SendTo.Server,RequireOwnership = true)]
    public void RecvInputServerRpc(Vector3 hitPoint, AttackType attackType)
    {
        if (attackType == AttackType.SpecialAbility)
        {
            
            RequestAction(hitPoint,AttackType.SpecialAbility, SkillTriggerStyle.MouseClick);
        }
        else
        {
            RequestAction(hitPoint,AttackType.BaseAttack, SkillTriggerStyle.MouseClick);
        }
    }
    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }
    }
    private void Update()
    {
        for (int i = 0; i< m_SkillRequestsCount; ++i)
        {
            if (!IsReleaseStyle(m_SkillRequests[i].TriggerStyle))
            {
                PerformAction(m_SkillRequests[i].Position, m_SkillRequests[i].AttackType ,m_SkillRequests[i].TriggerStyle, m_SkillRequests[i].TargetId);
            }
        }
        m_SkillRequestsCount = 0;   
    }
    void SendNewInput(AbilityRequest data)
    {
        m_ServerCharacter.RecvAbilityRequest(data);
    }
    void PerformAction(Vector3 point,AttackType attackType,SkillTriggerStyle triggerStyle, ulong targetId = 0)
    {
        if (PopulateSkillRequest(point, attackType) == null)
        {
            return;
        }
        SendNewInput(PopulateSkillRequest(point, attackType));
    }
    AbilityRequest PopulateSkillRequest(Vector3 hitPoint,AttackType attackType)
    {
        Ability ability;
        AbilityRequest data1;

        Vector3 offset = hitPoint - m_PhysicsWrapper.Transform.position;
        offset.y = 0;
        Vector3 direction = offset.normalized;
        Vector3 position = hitPoint;

        switch (attackType)
        {
            case AttackType.SpecialAbility:
            {
                    ability = m_ServerCharacter.CharacterStats.SpecialAbility;
                    if (Vector3.Distance(hitPoint, m_PhysicsWrapper.Transform.position) > ability.MaxRange)
                    {
                        return null;
                    }
                    data1 = new AbilityRequest(ability, position, direction);
                break;
            }
            case AttackType.BaseAttack:
            {
                    var weapon = GamePlayDataSource.Instance.GetWeaponPrototypeByID(m_ServerCharacter.CurrentWeaponId.Value);
                    ability = weapon.Ability;
                    
                    if (Vector3.Distance(hitPoint, m_PhysicsWrapper.Transform.position) > ability.MaxRange)
                    {
                        position = m_PhysicsWrapper.Transform.position + direction * ability.MaxRange;
                    }
                    else if (Vector3.Distance(hitPoint, m_PhysicsWrapper.Transform.position) < ability.MinRange)
                    {
                        position = m_PhysicsWrapper.Transform.position + direction * ability.MinRange;
                    }
                    data1 = new AbilityRequest(ability, position, direction);
                break;
            }
            default: return null;
        }
        return data1;
    }
    void RequestAction(Vector3 hitPoint, AttackType attackType ,SkillTriggerStyle style, ulong targetID = 0)
    {
        if(m_SkillRequestsCount < m_SkillRequests.Length)
        {
            m_SkillRequests[m_SkillRequestsCount].Position = hitPoint;
            m_SkillRequests[m_SkillRequestsCount].AttackType = attackType;
            m_SkillRequests[m_SkillRequestsCount].TriggerStyle = style;
            m_SkillRequests[m_SkillRequestsCount].TargetId = targetID;
            m_SkillRequestsCount++;
        }
    }
}


