using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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

    [SerializeField]
    PlayerInfoBarUI m_PlayerInfoBarUI;

    [SerializeField]
    GameObject m_Aura;    
    ServerCharacter m_ServerCharacter;
    public ServerCharacter serverCharacter => m_ServerCharacter;

    ClientSkillPlayer m_ClientActionPlayer;

    ClientAbilityHandler m_ClientAbilityHandler;
    public bool CanPerformSkill => m_ServerCharacter.CanPerformSkills;

    MainPlayerIngameCard m_MainPlayerIngameCard;

    private Volume volume;
    private ColorAdjustments colorAdjustments;
    private Counter counter;
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
        if (!GamePlayBehaviour.Instance.IsGameStart.Value) return;
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
        if(m_ServerCharacter.OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            m_Aura.GetComponent<MeshRenderer>().material.color = Color.green;
        }
        NetworkManager.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        m_ServerCharacter.MovementStatus.OnValueChanged += OnMovementStatusChanged;
        m_ServerCharacter.LifeState.OnValueChanged += OnLifeStateChanged;
        m_ServerCharacter.CurrentWeaponId.OnValueChanged += OnCurrentWeaponChanged;
        m_ServerCharacter.ManaPoint.OnValueChanged += OnManaPointChanged;
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
        if(m_ServerCharacter)
        {
            m_ServerCharacter.LifeState.OnValueChanged -= OnLifeStateChanged;
            m_ServerCharacter.CurrentWeaponId.OnValueChanged -= OnCurrentWeaponChanged;
            m_ClientInputSender.ClientMoveEvent -= OnMoveInput;
            m_ServerCharacter.ManaPoint.OnValueChanged -= OnManaPointChanged;
        }
        NetworkManager.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
        enabled = false;
    }

    private void OnManaPointChanged(int previousValue, int newValue)
    {
        if(m_MainPlayerIngameCard!=null)
        {
            m_MainPlayerIngameCard.UpdateCurrentMana(newValue);
            Debug.Log(newValue);
        }
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
            m_RevivePart.SetActive(true);

            if (serverCharacter.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                colorAdjustments.saturation.value = 0;
                if(counter!=null)
                counter.Hide();
            }
        }
        if(newValue == LifeStateEnum.Dead)
        {
            var fx =  ParticlePool.Singleton.GetObject(m_FaintedPart, m_ServerCharacter.physicsWrapper.transform.position,Quaternion.identity);
            fx.GetComponent<SpecialFXGraphic>().OnInitialized(m_FaintedPart);

            if(serverCharacter.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                colorAdjustments.saturation.value = -100;
                if (counter != null)
                    counter.Show();
            }
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName == "Map1" || sceneName == "Map2")
        {
            if (m_ServerCharacter.IsOwner)
            {
                gameObject.AddComponent<CameraController>();
            }
            m_MainPlayerIngameCard = FindObjectOfType<MainPlayerIngameCard>();
            counter = m_MainPlayerIngameCard.counter;
            CharacterSpawner spawner = FindObjectOfType<CharacterSpawner>();
            foreach(var player in spawner.Players)
            {
                if(player.ClientId == serverCharacter.OwnerClientId)
                {
                    bool isOwner = serverCharacter.OwnerClientId == NetworkManager.Singleton.LocalClientId? true: false;
                    m_PlayerInfoBarUI.Init(player.ClientName,isOwner);
                }
            }    

            volume = FindObjectOfType<Volume>();
            if (volume != null)
            {
                if (volume.profile.TryGet(out colorAdjustments))
                {
                    colorAdjustments.saturation.value = 0;
                }
            }
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
            case MovementStatus.Dash:
                return m_VisualizationConfiguration.SpeedDash;
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
    internal void ClientPlayEffectRpc(Vector3 position,Quaternion rotation,int num = 0, bool special = false)
    {
        Ability ability;
        if (special == true)
        {
            ability = m_ServerCharacter.CharacterStats.SpecialAbility;
        }
        else
        {
            
            var weapon = GamePlayDataSource.Instance.GetWeaponPrototypeByID(m_ServerCharacter.CurrentWeaponId.Value);
            ability = weapon.Ability;
        }
        m_ClientAbilityHandler.PlayAbility(ability, position,rotation,num);
    }

    [Rpc(SendTo.ClientsAndHost)]
    internal void DeniedActionRpc()
    {
        AudioManager.Instance.PlaySFXNumber(1);
    }
}

