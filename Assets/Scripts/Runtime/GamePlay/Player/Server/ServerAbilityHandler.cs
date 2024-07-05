using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

public class ServerAbilityHandler
{
    ServerCharacter m_ServerCharacter;
    CharacterMovement m_Movement;
    Animator Animator;

    private Queue<AbilityRequest> m_AbilityQueue = new Queue<AbilityRequest>();
    private bool m_IsProcessing = false;
    public ServerAbilityHandler(ServerCharacter serverCharacter, CharacterMovement movement)
    {
        m_ServerCharacter = serverCharacter;
        m_Movement = movement;
        Animator = m_ServerCharacter.ServerAnimationHandler.NetworkAnimator.Animator;
    }
    public void OnUpdate()
    {
        if( m_AbilityQueue.Count > 0 )
        {
            var peek = m_AbilityQueue.Peek();
            if (!m_IsProcessing)
            {
                ProcessAbilityRequest(peek);
            }
            else
            {
                peek.Ability.OnAbilityUpdate(m_ServerCharacter);
            }
        }
    }
    public void ReceiveAbilityRequest(AbilityRequest request)
    {
        if(!m_IsProcessing)
        {
            m_AbilityQueue.Enqueue(request);
        }
    }
    private void ProcessAbilityRequest(AbilityRequest request)
    {
        if(CanActivateAbility(request))
        {
            m_IsProcessing=true;
            request.Ability.TimeStarted = Time.time;
            ActivateAbility(request);
            

           // CoroutineRunner.Instance.StartCoroutine(WaitForAnimationToComplete(request));
        }
        else
        {
            SendRejectionToClient(request);
            m_AbilityQueue.Dequeue();
        }
    }
    //private IEnumerator WaitForAnimationToComplete(AbilityRequest request)
    //{
    //    if(Animator != null)
    //    {
    //        while (Animator.GetCurrentAnimatorStateInfo(1).IsName(request.Ability.abilityAnimationTrigger))
    //        {
    //            yield return null;
    //        }
    //    }
    //    m_IsProcessing = false;
    //    m_AbilityQueue.Dequeue();
    //}
    private bool CanActivateAbility(AbilityRequest request)
    {
        return request.Ability != null && request.Ability.CanActivate(m_ServerCharacter);
    }
    private void ActivateAbility(AbilityRequest request)
    {
        request.Ability.Activate(m_ServerCharacter,request);
    }
    private void SendRejectionToClient(AbilityRequest request)
    {

    }
    public void DequeuePeakAbility()
    {
        m_IsProcessing = false;
        m_AbilityQueue.Peek().Ability.OnReset();
        m_AbilityQueue.Dequeue();
    }
}
