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

    public bool CanPerformSkill => m_ServerCharacter.CanPerformSkills;
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
    [Rpc(SendTo.ClientsAndHost)]
    public void ClientCancelSkillByIDRpc(SkillID skillID)
    {
        m_ClientActionPlayer.CancelAllSKillWithSamePrototypeID(skillID);
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
        m_ServerCharacter = GetComponentInParent<ServerCharacter>();
        m_ClientVisualsAnimator = m_NetworkAnimator.Animator;
        
        NetworkManager.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        m_ServerCharacter.MovementStatus.OnValueChanged += OnMovementStatusChanged;

        OnMovementStatusChanged(MovementStatus.Normal, m_ServerCharacter.MovementStatus.Value);

        transform.SetPositionAndRotation(serverCharacter.physicsWrapper.Transform.position, serverCharacter.physicsWrapper.Transform.rotation);
        m_LerpedPosition = transform.position;
        m_LerpedRotation = transform.rotation;
        m_PositionLerper = new PositionLerper(serverCharacter.physicsWrapper.Transform.position, k_LerpTime);
        m_RotationLerper = new RotationLerper(serverCharacter.physicsWrapper.Transform.rotation, k_LerpTime);

        m_ClientInputSender.SkillInputEvent += OnSkillInput;
        m_ClientInputSender.ClientMoveEvent += OnMoveInput;
    }
    public override void OnNetworkDespawn()
    {
        NetworkManager.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
        
        m_ClientInputSender.SkillInputEvent -= OnSkillInput;
        m_ClientInputSender.ClientMoveEvent -= OnMoveInput;
        enabled = false;
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
}
