using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ClientInputSender : NetworkBehaviour
{
    const float k_MouseInputRaycastDistance = 100f;
    readonly RaycastHit[] k_CachedHit = new RaycastHit[4];
    LayerMask m_ActionLayerMask;

    [SerializeField]
    ServerCharacter m_ServerCharacter;

    [SerializeField] 
    private ServerInputReceiver m_InputReceiver;

    [SerializeField]
    private InputReader m_InputReader;

    [SerializeField]
    private PhysicsWrapper m_PhysicsWrapper;

    MainPlayerIngameCard m_MainPlayerIngameCard;
    ChatUI m_ChatUI;
    bool m_CanInput;

    #region Events

    public event Action<Vector3> ClientMoveEvent;

    public event Action<SkillRequestData> SkillInputEvent;

    #endregion
    public override void OnNetworkSpawn()
    {
        if (!IsClient || !IsOwner)
        {
            enabled = false;
            return;
        }
        m_CanInput = true;
        m_ChatUI = FindObjectOfType<ChatUI>();
        
        m_InputReader.MoveEvent += InputReader_MoveEvent;
        

        m_ServerCharacter.CurrentWeaponId.OnValueChanged += OnCurrentWeaponChanged;
        m_ServerCharacter.WeaponUseTimeAmount.OnValueChanged += OnWeaponAmountUseAmountChanged;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        m_ActionLayerMask = LayerMask.GetMask(new[] { "PCs", "Environment", "Ground" });
    }
    public override void OnNetworkDespawn()
    {
        m_CanInput = false;
        m_InputReader.MoveEvent -= InputReader_MoveEvent;
        if (m_ServerCharacter)
        {
            m_ServerCharacter.WeaponUseTimeAmount.OnValueChanged -= OnWeaponAmountUseAmountChanged;
            m_ServerCharacter.CurrentWeaponId.OnValueChanged -= OnCurrentWeaponChanged;
            
        }
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
    }

    private void OnCurrentWeaponChanged(WeaponID previousValue, WeaponID newValue)
    {
        m_MainPlayerIngameCard.UpdateBaseAttackWeapon(newValue);
    }

    private void Update()
    {
        m_CanInput = !m_ChatUI.IsChating;
        if (!m_CanInput) return;
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            Physics.RaycastNonAlloc(ray, k_CachedHit, k_MouseInputRaycastDistance, m_ActionLayerMask);
            if (m_InputReader.IsLeftMouseButtonDownThisFrame())
            {
                m_InputReceiver.RecvInputServerRpc(k_CachedHit[0].point);
            }
        }
    }
    private void InputReader_MoveEvent(Vector2 moveDir)
    {
        if (m_CanInput)
        {
            m_ServerCharacter.SendCharacterInputServerRpc(moveDir);
            ClientMoveEvent?.Invoke(moveDir);
        }
    }
    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName == "Map1" || sceneName == "Map2")
        {
            m_MainPlayerIngameCard = FindObjectOfType<MainPlayerIngameCard>();
            m_MainPlayerIngameCard.UpdateBaseAttackWeapon(m_ServerCharacter.CurrentWeaponId.Value);
            m_MainPlayerIngameCard.UpdateWeaponAmount(true);
        }
    }

    private void OnWeaponAmountUseAmountChanged(int previousValue, int newValue)
    {
        if (m_ServerCharacter.CurrentWeaponId.Value != m_ServerCharacter.CharacterStats.WeaponData.Id && newValue > 0)
        {
            m_MainPlayerIngameCard.UpdateWeaponAmount(false, newValue);
        }
        else
        {
            m_MainPlayerIngameCard.UpdateWeaponAmount(true);
        }
    }
}
