using System;
using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GamePlayBehaviour : NetworkBehaviour
{
    public static GamePlayBehaviour Instance { get; private set; }
    [SerializeField] GameObject CMCam;
    [SerializeField] GameObject LoadingScreen;
    [SerializeField] CountDownTimer m_countDownTimer;
    [SerializeField] float m_CharSelectCountdownDuration;
    [SerializeField] float m_InGameCountdownDuration;
    [SerializeField] float m_GameStartDelay;

    public event Action OnGameOver;
    bool isCharacterSet;
    public NetworkVariable<bool> IsStartCharSelect { get; private set; } = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> IsGameStart { get; private set; } = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> IsGameOver { get; private set; } = new NetworkVariable<bool>(false);

    public NetworkVariable<int> completePlayerCount { get; private set; } = new NetworkVariable<int>(0);

    private void Awake()
    {
        Instance = this;
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            m_countDownTimer.OnTimeExpired += CountDownTimer_OnTimeExpired;
            SceneTransitionManager.Instance.OnPlayersLoadComplete += OnPlayersLoadComplete;
        }
        
        if(IsClient)
        {
            SceneTransitionManager.Instance.OnLocalLoadComplete += OnLocalLoadComplete;
            SceneTransitionManager.Instance.OnSceneLoad += OnSceneLoad;
        }

        GameStateManager.Instance.OnGamePlayStateChanged += Instance_OnGamePlayStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer) 
        {
            m_countDownTimer.OnTimeExpired -= CountDownTimer_OnTimeExpired;
            SceneTransitionManager.Instance.OnPlayersLoadComplete -= OnPlayersLoadComplete;
        }
        if(IsClient)
        {
            SceneTransitionManager.Instance.OnLocalLoadComplete -= OnLocalLoadComplete;
            SceneTransitionManager.Instance.OnSceneLoad -= OnSceneLoad;
        }
        GameStateManager.Instance.OnGamePlayStateChanged += Instance_OnGamePlayStateChanged;
    }

    private void Instance_OnGamePlayStateChanged(GamePlayState arg1, GamePlayState arg2)
    {
        if(IsClient)
        {
            if(arg2 != GamePlayState.GameOver)
            {
                LoadingScreen.SetActive(true);
            }
        }
    }

    private void CountDownTimer_OnTimeExpired()
    {
        if (IsServer)
        {
            var state = GameStateManager.Instance.CurrentGamePlayState.Value;
            switch (state)
            {
                case GamePlayState.SelectCharacter:
                    isCharacterSet = true;
                    break;
                case GamePlayState.PlayGame:
                    Debug.Log($"CountDownTimer_OnTimeExpired called. Current GamePlayState: {state}, IsCharacterSet: {isCharacterSet}, IsGameStart: {IsGameStart.Value}");
                    if (!IsGameStart.Value)
                    {
                        IsGameStart.Value = true;
                        m_countDownTimer.StartCountdown(m_InGameCountdownDuration);
                        ClientPlayBGMRpc(4);
                    }
                    else
                    {
                        OnGameOver?.Invoke();
                        IsGameOver.Value = true;
                        LoadSceneDelay(5, GamePlayState.GameOver);
                    }
                    break;
            }
        }
    }
    public void OnPlayersLoadComplete()
    {
        var state = GameStateManager.Instance.CurrentGamePlayState.Value;
        switch (state)
        {
            case GamePlayState.Undefined:
                ClientToggleLoadingRpc(true);
                GameStateManager.Instance.SetGamePlayState(GamePlayState.SelectCharacter);
                break;
            case GamePlayState.SelectCharacter:
                NetworkServer.Instance.CreateGameSession();
                m_countDownTimer.StartCountdown(m_CharSelectCountdownDuration);
                IsStartCharSelect.Value = true;
                break;
            case GamePlayState.PlayGame:
                m_countDownTimer.StartCountdown(m_GameStartDelay);
                break;   
        }
    }

    private void OnSceneLoad()
    {
    }

    private void OnLocalLoadComplete()
    {
        LoadingScreen.SetActive(false);
        var state = GameStateManager.Instance.CurrentGamePlayState.Value;
        switch (state)
        {
            case GamePlayState.SelectCharacter:
                AudioManager.Instance.PlayBGMNumber(2);
                InventoryManager.Instance.AddAllPlayer();
                break;
            case GamePlayState.PlayGame:
                Camera.main.orthographic = false;
                CMCam.SetActive(true);
                break;
        }
    }
    IEnumerator LoadGamePlayCoroutine(float time,GamePlayState state)
    {
        yield return new WaitForSeconds(time);
        GameStateManager.Instance.SetGamePlayState(state);
    }
    public void LoadSceneDelay(float time,GamePlayState state)
    {
        StartCoroutine(LoadGamePlayCoroutine(time,state));
    }
    [Rpc(SendTo.ClientsAndHost)]
    void ClientPlayBGMRpc(int num)
    {
        AudioManager.Instance.PlayBGMNumber(num);
    }
    [Rpc(SendTo.ClientsAndHost)]
    void ClientToggleLoadingRpc(bool isActive)
    {
        LoadingScreen.SetActive(isActive);
    }
}
