using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public enum SkillTriggerStyle
{
    None,
    MouseClick,
    Keyboard,
    KeyboardRelease,
    UI,
    UIRelease,
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
    public void RecvInputServerRpc(Vector3 hitPoint, int inputType = 0)
    {
        if (inputType == 0)
        {
            RequestAction(hitPoint,SkillTriggerStyle.MouseClick);
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
                PerformAction(m_SkillRequests[i].Position, m_SkillRequests[i].TriggerStyle, m_SkillRequests[i].TargetId);
            }
        }
        m_SkillRequestsCount = 0;   
    }
    void SendNewInput(AbilityRequest data)
    {
        m_ServerCharacter.RecvAbilityRequest(data);
    }
    void PerformAction(Vector3 point,SkillTriggerStyle triggerStyle, ulong targetId = 0)
    { 
        SendNewInput(PopulateSkillRequest(point));
    }
    AbilityRequest PopulateSkillRequest(Vector3 hitPoint)
    {
        var weapon = GamePlayDataSource.Instance.GetWeaponPrototypeByID(m_ServerCharacter.CurrentWeaponId.Value);
        Vector3 offset = hitPoint - m_PhysicsWrapper.Transform.position;
        offset.y = 0;
        Vector3 direction = offset.normalized;
        Vector3 position = hitPoint;
        if(Vector3.Distance(hitPoint,m_PhysicsWrapper.Transform.position) > weapon.Ability.Range)
        {
            position = m_PhysicsWrapper.Transform.position + direction*weapon.Ability.Range;
        }
        AbilityRequest data1 = new AbilityRequest(weapon.Ability, position, direction);
        return data1;
    }
    void RequestAction(Vector3 hitPoint, SkillTriggerStyle style, ulong targetID = 0)
    {
        if(m_SkillRequestsCount < m_SkillRequests.Length)
        {
            m_SkillRequests[m_SkillRequestsCount].Position = hitPoint;
            m_SkillRequests[m_SkillRequestsCount].TriggerStyle = style;
            m_SkillRequests[m_SkillRequestsCount].TargetId = targetID;
            m_SkillRequestsCount++;
        }
    }
}


