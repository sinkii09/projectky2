using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class ClientCharacter : NetworkBehaviour
{
    #region References
    [SerializeField]
    GameObject m_Visual;

    Animator m_ClientVisualsAnimator;
    public Animator OurAnimator => m_ClientVisualsAnimator;

    [SerializeField]
    NetworkAnimator m_NetworkAnimator;
    public NetworkAnimator NetworkAnimator => m_NetworkAnimator;

    [SerializeField]
    ClientInputSender m_ClientInputSender;

    [SerializeField]
    VisualizationConfiguration m_VisualizationConfiguration;

    ServerCharacter m_ServerCharacter;
    public ServerCharacter serverCharacter => m_ServerCharacter;

    ClientSkillPlayer m_ClientActionPlayer;

    ClientAbilityHandler m_ClientAbilityHandler;
    public bool CanPerformSkill => m_ServerCharacter.CanPerformSkills;
    #endregion
    #region VFX
    [SerializeField] GameObject m_RevivePart;
    [SerializeField] GameObject m_FaintedPart;
    [SerializeField] GameObject m_NewWeaponPart;
    [SerializeField] GameObject m_BaseWeaponPart;

    #endregion
    PositionLerper m_PositionLerper;

    RotationLerper m_RotationLerper;

    const float k_LerpTime = 0.08f;

    Vector3 m_LerpedPosition;

    Quaternion m_LerpedRotation;

    float m_CurrentSpeed;

    [Rpc(SendTo.ClientsAndHost)]
    public void ClientPlaySkillRpc(SkillRequestData data) 
    {
        SkillRequestData data1 = data;
        m_ClientActionPlayer.PlaySKill(ref data1);
    }
    private void Awake()
    {
        enabled = false;
    }
    private void Update()
    {
        if (m_ClientVisualsAnimator)
        {
            OurAnimator.SetFloat(m_VisualizationConfiguration.SpeedVariableID, m_CurrentSpeed);
        }
        m_ClientActionPlayer.OnUpdate();
    }
    public override void OnNetworkSpawn()
    {
        if (!IsClient || transform.parent == null)
        {
            return;
        }
        enabled = true;

        m_ClientActionPlayer = new ClientSkillPlayer(this);
        m_ClientAbilityHandler = new ClientAbilityHandler(this);
        m_ServerCharacter = GetComponentInParent<ServerCharacter>();
        m_ClientVisualsAnimator = m_NetworkAnimator.Animator;
        
        NetworkManager.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        m_ServerCharacter.MovementStatus.OnValueChanged += OnMovementStatusChanged;
        m_ServerCharacter.LifeState.OnValueChanged += OnLifeStateChanged;
        m_ServerCharacter.CurrentWeaponId.OnValueChanged += OnCurrentWeaponChanged;
        OnMovementStatusChanged(MovementStatus.Normal, m_ServerCharacter.MovementStatus.Value);

        transform.SetPositionAndRotation(serverCharacter.physicsWrapper.Transform.position, serverCharacter.physicsWrapper.Transform.rotation);
        m_LerpedPosition = transform.position;
        m_LerpedRotation = transform.rotation;
        m_PositionLerper = new PositionLerper(serverCharacter.physicsWrapper.Transform.position, k_LerpTime);
        m_RotationLerper = new RotationLerper(serverCharacter.physicsWrapper.Transform.rotation, k_LerpTime);

        m_ClientInputSender.ClientMoveEvent += OnMoveInput;
    }
    public override void OnNetworkDespawn()
    {
        NetworkManager.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
        m_ServerCharacter.LifeState.OnValueChanged -= OnLifeStateChanged;
        m_ServerCharacter.CurrentWeaponId.OnValueChanged -= OnCurrentWeaponChanged;
        m_ClientInputSender.ClientMoveEvent -= OnMoveInput;
        enabled = false;
    }

    private void OnCurrentWeaponChanged(WeaponID previousValue, WeaponID newValue)
    {
        if(newValue == serverCharacter.CharacterStats.WeaponData.Id)
        {
            var fx = ParticlePool.Singleton.GetObject(m_NewWeaponPart, m_ServerCharacter.physicsWrapper.transform.position, Quaternion.identity);
            fx.GetComponent<SpecialFXGraphic>().OnInitialized(m_NewWeaponPart);
        }
        else
        {
            var fx = ParticlePool.Singleton.GetObject(m_BaseWeaponPart, m_ServerCharacter.physicsWrapper.transform.position, Quaternion.identity);
            fx.GetComponent<SpecialFXGraphic>().OnInitialized(m_BaseWeaponPart);
        }
    }

    private void OnLifeStateChanged(LifeStateEnum previousValue, LifeStateEnum newValue)
    {
        if(newValue == LifeStateEnum.Alive)
        {
            var fx = ParticlePool.Singleton.GetObject(m_RevivePart, m_ServerCharacter.physicsWrapper.transform.position, Quaternion.identity);
            fx.GetComponent<SpecialFXGraphic>().OnInitialized(m_RevivePart);
        }
        if(newValue == LifeStateEnum.Dead)
        {
            var fx =  ParticlePool.Singleton.GetObject(m_FaintedPart, m_ServerCharacter.physicsWrapper.transform.position,Quaternion.identity);
            fx.GetComponent<SpecialFXGraphic>().OnInitialized(m_FaintedPart);
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if(sceneName=="Map1" || sceneName=="Map2")
        {
            if (m_ServerCharacter.IsOwner)
                gameObject.AddComponent<CameraController>();
        }
    }

    private void OnMoveInput(Vector3 vector)
    {
        if(!IsAnimating())
        {
            OurAnimator.SetTrigger(m_VisualizationConfiguration.AnticipateMoveTriggerID);
        }
    }

    private void OnSkillInput(SkillRequestData data)
    {
        //m_ClientActionPlayer.AnticipateSkill(ref data);
    }

    void OnMovementStatusChanged(MovementStatus previousValue, MovementStatus newValue)
    {
        m_CurrentSpeed = GetVisualMovementSpeed(newValue);
    }
    float GetVisualMovementSpeed(MovementStatus movementStatus)
    {
        if (m_ServerCharacter.LifeState.Value != LifeStateEnum.Alive)
        {
            return m_VisualizationConfiguration.SpeedDead;
        }
        switch (movementStatus)
        {
            case MovementStatus.Idle:
                return m_VisualizationConfiguration.SpeedIdle;
            case MovementStatus.Normal:
                return m_VisualizationConfiguration.SpeedNormal;
            case MovementStatus.Uncontrolled:
                return m_VisualizationConfiguration.SpeedUncontrolled;
            case MovementStatus.Slowed:
                return m_VisualizationConfiguration.SpeedSlowed;
            case MovementStatus.Hasted:
                return m_VisualizationConfiguration.SpeedHasted;
            case MovementStatus.Jump:
                return m_VisualizationConfiguration.SpeedJump;
            default:
                throw new Exception($"Unknown MovementStatus {movementStatus}");
        }
    }
    void OnAnimEvent(string id)
    {
        m_ClientActionPlayer.OnAnimEvent(id);
    }
    public bool IsAnimating()
    {
        if (OurAnimator.GetFloat(m_VisualizationConfiguration.SpeedVariableID) > 0.0)
        {
            return true;
        }
        for (int i = 0; i < OurAnimator.layerCount; i++)
        {
            if (OurAnimator.GetCurrentAnimatorStateInfo(i).tagHash != m_VisualizationConfiguration.BaseNodeTagID)
            {
                return true;
            }
        }
        return false;
    }
    public void SetActiveVisual(bool isActive)
    {
        m_Visual.SetActive(isActive);
    }

    [Rpc(SendTo.ClientsAndHost)]
    internal void ClientPlayEffectRpc(Vector3 position,int num = 0, int special = -1)
    {
        Ability ability;
        if (special == -1)
        {
            ability = m_ServerCharacter.CharacterStats.SpecialAbility;
        }
        else
        {
            
            var weapon = GamePlayDataSource.Instance.GetWeaponPrototypeByID(m_ServerCharacter.CurrentWeaponId.Value);
            ability = weapon.Ability;
            Debug.Log(ability.name);
        }
        m_ClientAbilityHandler.PlayAbility(ability, position,num);
    }
}

