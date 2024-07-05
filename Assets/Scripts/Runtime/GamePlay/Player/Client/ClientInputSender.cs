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

    [SerializeField]
    Transform m_MousePoint;

    [SerializeField]
    private GameIndicator m_RangeIndicator;

    [SerializeField]
    private GameIndicator m_RadiusIndicator;

    MainPlayerIngameCard m_MainPlayerIngameCard;
    ChatUI m_ChatUI;
    bool m_CanInput;

    AttackType m_AttackType = AttackType.BaseAttack;
    #region Events

    public event Action<Vector3> ClientMoveEvent;

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

        m_RangeIndicator.Initialize(m_PhysicsWrapper.Transform);
        m_RadiusIndicator.Initialize(m_MousePoint,m_ServerCharacter.CharacterStats.SpecialAbility.indicatorTexture);
        
        m_InputReader.MoveEvent += InputReader_MoveEvent;
        m_InputReader.SpecialInputEvent += InputReader_SpecialInputEvent;

        m_ServerCharacter.CurrentWeaponId.OnValueChanged += OnCurrentWeaponChanged;
        m_ServerCharacter.WeaponUseTimeAmount.OnValueChanged += OnWeaponAmountUseAmountChanged;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        m_ActionLayerMask = LayerMask.GetMask(new[] { "PCs", "Environment", "Ground" });
    }
    public override void OnNetworkDespawn()
    {
        m_CanInput = false;
        m_InputReader.MoveEvent -= InputReader_MoveEvent;
        m_InputReader.SpecialInputEvent -= InputReader_SpecialInputEvent;
        if (m_ServerCharacter)
        {
            m_ServerCharacter.WeaponUseTimeAmount.OnValueChanged -= OnWeaponAmountUseAmountChanged;
            m_ServerCharacter.CurrentWeaponId.OnValueChanged -= OnCurrentWeaponChanged;
            
        }
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
    }
    private void InputReader_SpecialInputEvent(bool obj)
    {
        switch (m_AttackType)
        {
            case AttackType.BaseAttack:
                Ability special = m_ServerCharacter.CharacterStats.SpecialAbility;
                if (special == null) return;
                if(special.ShowIndicator)
                {
                    ShowIndicator(special);
                }
                m_AttackType = AttackType.SpecialAbility;
                return;
            case AttackType.SpecialAbility:
                HideIndicator();
                m_AttackType = AttackType.BaseAttack;
                return;
        }
    }

    private void OnCurrentWeaponChanged(WeaponID previousValue, WeaponID newValue)
    {
        m_MainPlayerIngameCard.UpdateBaseAttackWeapon(newValue);
    }

    private void Update()
    {
        m_CanInput = !m_ChatUI.IsChating;
        if (!m_CanInput) return;
        if (!EventSystem.current.IsPointerOverGameObject() && Camera.main != null)
        {
            var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            Physics.RaycastNonAlloc(ray, k_CachedHit, k_MouseInputRaycastDistance, m_ActionLayerMask);
            m_MousePoint.position = k_CachedHit[0].point;
            if (m_InputReader.IsLeftMouseButtonDownThisFrame())
            {
                switch (m_AttackType)
                {
                    case AttackType.BaseAttack:
                        m_InputReceiver.RecvInputServerRpc(k_CachedHit[0].point,AttackType.BaseAttack);
                        break;
                    case AttackType.SpecialAbility:
                        HideIndicator();
                        m_AttackType = AttackType.BaseAttack;
                        m_InputReceiver.RecvInputServerRpc(k_CachedHit[0].point,AttackType.SpecialAbility);
                        break;
                }
                
            }
            if(Input.GetMouseButtonDown(1))
            {
                if(m_AttackType == AttackType.SpecialAbility)
                {
                    HideIndicator();
                    m_AttackType = AttackType.BaseAttack;
                }
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
            m_MainPlayerIngameCard.UpdateSpecial(m_ServerCharacter.CharacterStats);
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

    private void HideIndicator()
    {
        m_RangeIndicator.HideIndicator();
        m_RadiusIndicator.HideIndicator();
    }

    private void ShowIndicator(Ability ability)
    {
        m_RangeIndicator.ShowIndicator(ability.MaxRange);
        m_RadiusIndicator.ShowIndicator(ability.Radius);
    }

}
