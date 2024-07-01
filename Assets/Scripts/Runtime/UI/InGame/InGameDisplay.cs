using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class InGameDisplay : MonoBehaviour
{
    public static InGameDisplay Instance;
    [Header("References")]
    [SerializeField] private NetcodeHooks m_NetcodeHooks;
    [SerializeField] private CharacterDatabase m_CharacterDatabase;
    [SerializeField] private CharacterSpawner m_CharacterSpawner;
    [SerializeField] private Transform m_PlayerIngameCardHolder;
    [SerializeField] private PlayerIngameCard m_PlayerIngameCardPrefab;
    [SerializeField] private MainPlayerIngameCard m_MainPlayerStatus;
    [SerializeField] private GameObject m_GameOverUI;
    [SerializeField] private TextMeshProUGUI m_TimerText;
    [SerializeField] private Button ExtButton;
    
    Dictionary<ulong, PlayerIngameCard> playerCards = new Dictionary<ulong, PlayerIngameCard>();

    GamePlayBehaviour gamePlayBehaviour;
    CountDownTimer countDownTimer;
    private void Awake()
    {
        m_NetcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
        m_NetcodeHooks.OnNetworkDespawnHook += OnNetworkDeSpawn;
        ExtButton.onClick.AddListener(ExitButtonClick);

        m_GameOverUI.SetActive(false);
        Instance = this;
    }
    private void Update()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            if (countDownTimer)
            {
                float duration = countDownTimer.GetRemainingTime();
                TimeSpan timeSpan = TimeSpan.FromSeconds(duration);
                m_TimerText.text = $"{timeSpan.Minutes}:{timeSpan.Seconds}";
            }
        }
    }
    private void OnDestroy()
    {
        
        m_NetcodeHooks.OnNetworkSpawnHook -= OnNetworkSpawn;
        m_NetcodeHooks.OnNetworkDespawnHook -= OnNetworkDeSpawn;
        ExtButton?.onClick.RemoveListener(ExitButtonClick);
    }

    void OnNetworkSpawn()
    {
        countDownTimer = FindObjectOfType<CountDownTimer>();
        m_CharacterSpawner.Players.OnListChanged += Players_OnListChanged;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        gamePlayBehaviour = FindObjectOfType<GamePlayBehaviour>();
        if( gamePlayBehaviour != null )
        {
            gamePlayBehaviour.IsGameOver.OnValueChanged += OnGameOver;
        }
    }

    void OnNetworkDeSpawn()
    {
        m_CharacterSpawner.Players.OnListChanged -= Players_OnListChanged;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
        if (gamePlayBehaviour != null)
        {
            gamePlayBehaviour.IsGameOver.OnValueChanged -= OnGameOver;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (loadSceneMode != UnityEngine.SceneManagement.LoadSceneMode.Additive) return;
        foreach (var item in m_CharacterSpawner.Players)
        {
            if (item.ClientId != NetworkManager.Singleton.LocalClientId)
            {
                var playerCard = Instantiate(m_PlayerIngameCardPrefab, m_PlayerIngameCardHolder);
                playerCard.Initialize(item, m_CharacterDatabase);
                playerCards[item.ClientId] = playerCard;
            }
            else
            {
                m_MainPlayerStatus.Initialize(item, m_CharacterDatabase);
            }
        }
    }
    private void Players_OnListChanged(NetworkListEvent<CharacterInGameState> changeEvent)
    {
        foreach(var item in m_CharacterSpawner.Players)
        {
            if(item.ClientId != NetworkManager.Singleton.LocalClientId)
            {
                if (playerCards.ContainsKey(item.ClientId)) 
                playerCards[item.ClientId].UpdateDisplay(item);
            }
            else
            {
                m_MainPlayerStatus.UpdateDisplay(item);
            }
        }
    }


    private void OnGameOver(bool previousValue, bool newValue)
    {
        m_GameOverUI.SetActive(newValue);
    }
    private void ExitButtonClick()
    {
        PopupManager.Instance.ShowPopup("DO you want to leave?", ExitGame);
    }
    private void ExitGame()
    {
        ClientSingleton.Instance.Manager.Disconnect();
    }
}
