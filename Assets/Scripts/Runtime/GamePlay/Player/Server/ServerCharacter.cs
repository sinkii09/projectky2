using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum LifeStateEnum
{
    Alive,
    Dead,
}
public class ServerCharacter : NetworkBehaviour
{
    #region NetVariables
    public NetworkVariable<MovementStatus> MovementStatus { get; } = new NetworkVariable<MovementStatus>();
    public NetworkVariable<ulong> HeldItem { get; } = new NetworkVariable<ulong>();
    public NetworkVariable<SkillID> CurrentSkillId { get; } = new NetworkVariable<SkillID>();
    public NetworkVariable<WeaponID> CurrentWeaponId { get; } = new NetworkVariable<WeaponID>();
    public NetworkVariable<int> WeaponUseTimeAmount { get; } = new NetworkVariable<int>();
    public NetworkVariable<ulong> TargetId { get; } = new NetworkVariable<ulong>();
    public NetworkVariable<bool> isGameOver { get; } = new NetworkVariable<bool>(false);
    public NetworkVariable<LifeStateEnum> LifeState { get; } = new NetworkVariable<LifeStateEnum>(LifeStateEnum.Alive);
    public bool IsValidTarget => LifeState.Value != LifeStateEnum.Dead;
    public int HitPoints
    {
        get => NetHealthState.HitPoints.Value;
        private set => NetHealthState.HitPoints.Value = value;
    }
    ServerCharacter m_LastInflicter;
    public bool CanPerformSkills => LifeState.Value == LifeStateEnum.Alive;
    #endregion

    #region References
    [Header("Movement Handler")]
    [SerializeField]
    CharacterMovement m_Movement;
    public CharacterMovement Movement => m_Movement;

    [Header("Physics Handler")]
    [SerializeField]
    PhysicsWrapper m_PhysicsWrapper;
    public PhysicsWrapper physicsWrapper => m_PhysicsWrapper;

    [Header("Damage Handler")]
    [SerializeField]
    DamageReceiver m_DamageReceiver;

    [Header("Client Visual Handler")]
    [SerializeField]
    ClientCharacter m_ClientCharacter;
    public ClientCharacter ClientCharacter => m_ClientCharacter;

    [Header("Animation Handler")]
    [SerializeField]
    ServerAnimationHandler m_ServerAnimationHandler;
    public ServerAnimationHandler ServerAnimationHandler => m_ServerAnimationHandler;

    [Header("Character Stats")]
    [SerializeField]
    CharacterStats m_CharacterStats;
    public CharacterStats CharacterStats => m_CharacterStats;

    private ServerSkillPlayer m_ServerSkillPlayer;
    public ServerSkillPlayer ServerSkillPlayer => m_ServerSkillPlayer;

    private ServerAbilityHandler m_ServerAbilityHandler;    
    public NetworkHealthState NetHealthState { get; private set; }



    private GamePlayBehaviour m_GamePlayBehaviour;

    private CharacterSpawner m_CharacterSpawner;

    private Rigidbody rb;
    #endregion


    #region Unity CallBacks
    private void Awake()
    {
        m_GamePlayBehaviour = FindObjectOfType<GamePlayBehaviour>();
        m_CharacterSpawner = FindObjectOfType<CharacterSpawner>();
        m_ServerSkillPlayer = new ServerSkillPlayer(this,m_Movement);
        m_ServerAbilityHandler = new ServerAbilityHandler(this,m_Movement);
        NetHealthState = GetComponent<NetworkHealthState>();
        rb = GetComponent<Rigidbody>(); 
    }
    private void Update()
    {
        m_ServerSkillPlayer.OnUpdate();
        m_ServerAbilityHandler.OnUpdate();
    }
    #endregion


    #region Network CallBacks
    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
        }
        WeaponUseTimeAmount.Value = 0;
        CurrentSkillId.Value = CharacterStats.BaseAttack.SkillID;
        CurrentWeaponId.Value = CharacterStats.WeaponData.Id;
        HitPoints = CharacterStats.BaseHP;
        m_GamePlayBehaviour.OnGameOver += GamePlayBehaviour_IsGameOver;
        LifeState.OnValueChanged += OnLifeStateChanged;
        m_DamageReceiver.DamageReceived += ReceiveHP;
        m_DamageReceiver.CollisionEntered += CollisionEntered;
        
    }

    public override void OnNetworkDespawn()
    {
        m_GamePlayBehaviour.OnGameOver -= GamePlayBehaviour_IsGameOver;
        LifeState.OnValueChanged -= OnLifeStateChanged;

        if (m_DamageReceiver)
        {
            m_DamageReceiver.DamageReceived -= ReceiveHP;
            m_DamageReceiver.CollisionEntered -= CollisionEntered;
        }
    }

    #endregion

    #region ServerRPC
    [Rpc(SendTo.Server)]
    public void SendCharacterInputServerRpc(Vector3 movementTarget, bool canJump = false)
    {
        if (isGameOver.Value) return;
        if (LifeState.Value != LifeStateEnum.Alive) return;
        if (m_Movement.IsPerformingForcedMovement()) return;
        if (m_ServerSkillPlayer.GetActiveSkillInfo(out SkillRequestData skillRequestData))
        {
            if(GamePlayDataSource.Instance.GetSkillPrototypeByID(skillRequestData.SkillID).Config.ActionInterruptible)
            {
                m_ServerSkillPlayer.ClearActions(false);
            }
        }
        m_ServerSkillPlayer.CancelRunningActionsByLogic(SkillLogic.Target, true);
        m_Movement.SetMoveDirection(movementTarget);
    }

    public void RecvDoSkillServer(SkillRequestData data)
    {
        if (isGameOver.Value) return;
        if (LifeState.Value != LifeStateEnum.Alive) return;
        SkillRequestData data1 = data;
        if(!GamePlayDataSource.Instance.GetSkillPrototypeByID(data1.SkillID).Config.IsFriendly)
        {
            ServerSkillPlayer.OnGameplayActivity(Skill.GameplayActivity.UsingAttackAction);
        }
        PlaySkill(ref data1);
    }
    public void RecvAbilityRequest(AbilityRequest data)
    {
        if (isGameOver.Value) return;
        if (LifeState.Value != LifeStateEnum.Alive) return;
        
        m_ServerAbilityHandler.ReceiveAbilityRequest(data);
        Debug.Log(data.Ability.name + " server");
    }
    public void DequeueAbility()
    {
        m_ServerAbilityHandler.DequeuePeakAbility();
    }
    #endregion

    #region Others

    private void GamePlayBehaviour_IsGameOver()
    {
        isGameOver.Value = true;
        m_ServerSkillPlayer.ClearActions(true);
        m_Movement.CancelMove();
    }

    void OnLifeStateChanged(LifeStateEnum prevState, LifeStateEnum newState)
    {
        if(newState != LifeStateEnum.Alive)
        {
            m_ServerSkillPlayer.ClearActions(true);
            m_Movement.CancelMove();
            rb.useGravity = false;
            m_PhysicsWrapper.DamageCollider.enabled = false;
            StartCoroutine(DisableVisual());
        }
        else
        {
            rb.useGravity = true;
            m_PhysicsWrapper.DamageCollider.enabled = true;
            m_ClientCharacter.SetActiveVisual(true);
        }
    }
    IEnumerator DisableVisual()
    {
        yield return new WaitForSeconds(2);
        m_ClientCharacter.SetActiveVisual(false);
    }
    public void PlaySkill(ref SkillRequestData data)
    {
        if(data.CancelMovement)
        {
            m_Movement.CancelMove();
        }
        m_ServerSkillPlayer.PlaySkill(ref data);
    }
    void ReceiveHP(int HP, ServerCharacter inflicter = null)
    {
        m_LastInflicter = inflicter;
        if(HP > 0)
        {
            m_ServerSkillPlayer.OnGameplayActivity(Skill.GameplayActivity.Healed);
            float healingMod = m_ServerSkillPlayer.GetBuffedValue(Skill.BuffableValue.PercentHealingReceived);
            HP = (int)(HP * healingMod);
        }
        else
        {
            m_ServerSkillPlayer.OnGameplayActivity(Skill.GameplayActivity.AttackedByEnemy);
            float damageMod = m_ServerSkillPlayer.GetBuffedValue(Skill.BuffableValue.PercentDamageReceived);
            HP = (int)(HP * damageMod);
            ServerAnimationHandler.NetworkAnimator.SetTrigger("HitReact1");
        }
        HitPoints = Mathf.Clamp(HitPoints + HP, 0, CharacterStats.BaseHP);
        if(HitPoints <= 0)
        {
            HitPoints = 0;
            LifeState.Value = LifeStateEnum.Dead;
            m_ServerSkillPlayer.ClearActions(false);
            m_CharacterSpawner.UpdateKill(m_LastInflicter.OwnerClientId);
        }
        m_CharacterSpawner.UpdateHealth(OwnerClientId, HitPoints, LifeState.Value == LifeStateEnum.Alive);
    }
    public void Revive(Vector3 position)
    {
        if(LifeState.Value == LifeStateEnum.Dead)
        {
            physicsWrapper.transform.position = position;
            HitPoints = Mathf.Clamp(CharacterStats.BaseHP, 0, CharacterStats.BaseHP);
            LifeState.Value = LifeStateEnum.Alive;
            m_CharacterSpawner.UpdateHealth(OwnerClientId, CharacterStats.BaseHP);
        }
    }
    void CollisionEntered(Collision collision)
    {
        if(m_ServerSkillPlayer != null)
        {
            m_ServerSkillPlayer.CollisionEntered(collision);
        }
    }
    #endregion
}
