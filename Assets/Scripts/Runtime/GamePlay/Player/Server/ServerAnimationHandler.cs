using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class ServerAnimationHandler : NetworkBehaviour
{
    [SerializeField]
    ServerCharacter m_ServerCharacter;

    [SerializeField]
    VisualizationConfiguration m_VisualizationConfiguration;


    [SerializeField]
    NetworkAnimator m_NetworkAnimator;
    public NetworkAnimator NetworkAnimator => m_NetworkAnimator;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(WaitToRegisterOnLifeStateChanged());
        }

    }

    public override void OnNetworkDespawn()
    {
        if(IsServer)
        {
            m_ServerCharacter.LifeState.OnValueChanged -= OnLifeStateChanged;
        }
    }
    IEnumerator WaitToRegisterOnLifeStateChanged()
    {
        yield return new WaitForEndOfFrame();
        m_ServerCharacter.LifeState.OnValueChanged += OnLifeStateChanged;
        if(m_ServerCharacter.LifeState.Value != LifeStateEnum.Alive)
        {
            OnLifeStateChanged(LifeStateEnum.Alive, m_ServerCharacter.LifeState.Value);
        }
    }
    private void OnLifeStateChanged(LifeStateEnum previousValue, LifeStateEnum newValue)
    {
        switch (newValue)
        {
            case LifeStateEnum.Alive:
                NetworkAnimator.SetTrigger(m_VisualizationConfiguration.AliveStateTriggerID);
                break;
            case LifeStateEnum.Dead:
                NetworkAnimator.SetTrigger(m_VisualizationConfiguration.DeadStateTriggerID);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newValue), newValue, null);
        }
    }
}
