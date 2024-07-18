using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
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
    [SerializeField] private KillNotifyHolder m_KillNotifyCardHolder;
    [SerializeField] private PlayerIngameCard m_PlayerIngameCardPrefab;
    [SerializeField] private MainPlayerIngameCard m_MainPlayerStatus;
    [SerializeField] private GameObject m_GameOverUI;
    [SerializeField] private TextMeshProUGUI m_TimerText;
    [SerializeField] private Button ExtButton;
    [SerializeField] private Counter m_CenterTimer;
    
    Dictionary<ulong, PlayerIngameCard> playerCards = new Dictionary<ulong, PlayerIngameCard>();

    CountDownTimer countDownTimer;
    private void Awake()
    {
        m_NetcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
        m_NetcodeHooks.OnNetworkDespawnHook += OnNetworkDeSpawn;
        ExtButton.onClick.AddListener(ExitButtonClick);
        m_CenterTimer.gameObject.SetActive(true);
        m_GameOverUI.SetActive(false);
        Instance = this;
    }
    private void Update()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            if (!countDownTimer) return;
            if(GamePlayBehaviour.Instance.IsGameStart.Value)
            {
                float duration = countDownTimer.GetRemainingTime();
                TimeSpan timeSpan = TimeSpan.FromSeconds(duration);
                m_TimerText.text = $"{timeSpan.Minutes}:{timeSpan.Seconds}";
            }
            else
            {
                float duration = countDownTimer.GetRemainingTime();
                TimeSpan timeSpan = TimeSpan.FromSeconds(duration);
                if(timeSpan.Seconds <= 1) 
                {
                    m_CenterTimer.GetGUIText().text = "READY!";
                }
                else
                {
                    m_CenterTimer.GetGUIText().text = $"{timeSpan.Seconds - 1}";            
                }
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
        m_CharacterSpawner.OnPlayerKilled += M_CharacterSpawner_OnPlayerKilled;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        GamePlayBehaviour.Instance.IsGameOver.OnValueChanged += OnGameOver;
        GamePlayBehaviour.Instance.IsGameStart.OnValueChanged += OnGameStart;
        
    }

    void OnNetworkDeSpawn()
    {
        m_CharacterSpawner.Players.OnListChanged -= Players_OnListChanged;
        m_CharacterSpawner.OnPlayerKilled -= M_CharacterSpawner_OnPlayerKilled;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
        GamePlayBehaviour.Instance.IsGameOver.OnValueChanged -= OnGameOver;
        GamePlayBehaviour.Instance.IsGameStart.OnValueChanged -= OnGameStart;
    }

    private void OnGameStart(bool previousValue, bool newValue)
    {
        if(newValue)
        {
            m_CenterTimer.gameObject.SetActive(false);
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


    private void M_CharacterSpawner_OnPlayerKilled(ulong killPlayer, ulong deadPlayer)
    {
        string killPlayerName = "";
        string deadPlayerName = "";

        foreach (var player in  m_CharacterSpawner.Players)
        {
            if(player.ClientId == killPlayer)
            {
                killPlayerName = player.ClientName;
            }
            else if (player.ClientId == deadPlayer)
            {
                deadPlayerName = player.ClientName;
            }
        }
        m_KillNotifyCardHolder.UpdateCardList(killPlayerName, deadPlayerName);
    }
    public void ActiveCenterTimer(bool isActive)
    {
        if(isActive)
        {
            m_CenterTimer.Show();
        }
        else
        {
            m_CenterTimer.Hide();
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
