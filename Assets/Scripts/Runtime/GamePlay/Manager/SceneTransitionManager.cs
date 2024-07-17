using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : NetworkBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [SerializeField] private string charSelectScene;
    [SerializeField] private string mapScene;
    [SerializeField] private string postScene;

    public NetworkVariable<int> PlayersLoadedCount { get; private set; } = new NetworkVariable<int>(0);
    public event Action OnSceneLoad;
    public event Action OnPlayersLoadComplete;
    public event Action OnLocalLoadComplete;
    private int totalPlayers;

    private void Awake()
    {
        Instance = this;
    }
    public override void OnNetworkSpawn()
    {
        totalPlayers = 0;
        GameStateManager.Instance.OnGamePlayStateChanged += HandleGamePlayStateChanged;
        
        NetworkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;

        

        NetworkManager.SceneManager.OnSynchronizeComplete += SceneManager_OnSynchronizeComplete;

        if(IsClient)
        {
            NetworkManager.SceneManager.OnLoad += SceneManager_OnLoad;
        }
    }

    public override void OnNetworkDespawn()
    {
        GameStateManager.Instance.OnGamePlayStateChanged -= HandleGamePlayStateChanged;

        NetworkManager.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;

        NetworkManager.SceneManager.OnSynchronizeComplete -= SceneManager_OnSynchronizeComplete;

        if (IsClient)
        {
            NetworkManager.SceneManager.OnLoad -= SceneManager_OnLoad;
        }
    }
    private void HandleGamePlayStateChanged(GamePlayState oldState, GamePlayState newState)
    {
        if (IsServer)
        {
            
            switch (newState)
            {
                case GamePlayState.SelectCharacter:
                    PlayersLoadedCount.Value = 0;
                    LoadScene(charSelectScene);
                    break;
                case GamePlayState.PlayGame:
                    PlayersLoadedCount.Value = 0;
                    UnloadScene(charSelectScene, () =>
                    {
                        LoadScene(mapScene);
                    });
                    
                    break;
                case GamePlayState.GameOver:
                    LoadScene(postScene,false);
                    break;
            }
        }
    }

    private void SceneManager_OnLoad(ulong clientId, string sceneName, LoadSceneMode loadSceneMode, AsyncOperation asyncOperation)
    {
        OnSceneLoad?.Invoke();
    }

    private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (IsServer)
        {
            if(GameStateManager.Instance.CurrentGamePlayState.Value != GamePlayState.Undefined)
            {
                NotifyPlayersLoadComplete();
            }
        }
        if (IsClient)
        {
            Debug.Log("onclientLoadComplete");
            OnLocalLoadComplete?.Invoke();
        }
    }

    private void SceneManager_OnSynchronizeComplete(ulong clientId)
    {
        if(IsClient)
        {
            Debug.Log($"client sync complete with {GameStateManager.Instance.CurrentGamePlayState.Value}");
            NotifyPlayerLoadedServerRpc();
        }
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void NotifyPlayerLoadedServerRpc()
    {
        PlayersLoadedCount.Value++;
        Debug.Log("total player count: " + totalPlayers);
        if (PlayersLoadedCount.Value == totalPlayers)
        {
            NotifyPlayersLoadComplete();
        }
    }
    private void NotifyPlayersLoadComplete()
    {
        OnPlayersLoadComplete?.Invoke();
    }

    public void SetTotalPlayers(int playerCount)
    {
        totalPlayers = playerCount;
        Debug.Log("total player change to: " + totalPlayers);
    }
    public void LoadScene(string sceneName, bool isAdditiveMode = true)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, isAdditiveMode? LoadSceneMode.Additive : LoadSceneMode.Single);
    }

    public void UnloadScene(string sceneName, Action onComplete)
    {
        StartCoroutine(UnloadSceneCoroutine(sceneName, onComplete));
    }

    private IEnumerator UnloadSceneCoroutine(string sceneName, Action onComplete)
    {
        var asyncUnload = NetworkManager.Singleton.SceneManager.UnloadScene(SceneManager.GetSceneByName(sceneName));
        yield return asyncUnload;
        onComplete?.Invoke();
    }
}
