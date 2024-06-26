using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

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
    
    Dictionary<ulong, PlayerIngameCard> playerCards = new Dictionary<ulong, PlayerIngameCard>();

    GamePlayBehaviour gamePlayBehaviour;
    private void Awake()
    {
        m_NetcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
        m_NetcodeHooks.OnNetworkDespawnHook += OnNetworkDeSpawn;
        

        m_GameOverUI.SetActive(false);
        Instance = this;
    }

    private void OnDestroy()
    {
        
        m_NetcodeHooks.OnNetworkSpawnHook -= OnNetworkSpawn;
        m_NetcodeHooks.OnNetworkDespawnHook -= OnNetworkDeSpawn;
        
    }

    void OnNetworkSpawn()
    {
        m_CharacterSpawner.Players.OnListChanged += Players_OnListChanged;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        gamePlayBehaviour = FindObjectOfType<GamePlayBehaviour>();
        if( gamePlayBehaviour != null )
        {
            Debug.Log("gameBehaviour found");
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

}
