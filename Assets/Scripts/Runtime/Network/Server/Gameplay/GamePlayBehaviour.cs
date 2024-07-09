using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GamePlayState
{
    Undefined,
    SelectCharacter,
    PlayGame,
    GameOver
}

public class GamePlayBehaviour : NetworkBehaviour
{
    public static GamePlayBehaviour Instance { get; private set; }
    [SerializeField] string m_CharSelectScene;
    [SerializeField] string m_MapScene;
    [SerializeField] string m_PostScene;
    [SerializeField] string m_MainScene;
    [SerializeField] CountDownTimer m_countDownTimer;
    [SerializeField] float m_CharSelectCountdownDuration;
    [SerializeField] float m_InGameCountdownDuration;
    private GamePlayState currentGamePlayState = GamePlayState.Undefined;

    public event Action OnGameOver;
    public NetworkVariable<bool> IsGameOver { get; private set; } = new NetworkVariable<bool>(false);
    private void Awake()
    {
        Instance = this;
    }
    public void LoadState(GamePlayState state)
    {
        currentGamePlayState = state;
        
        switch (state)
        {
            case GamePlayState.SelectCharacter:
                NetworkManager.SceneManager.LoadScene(m_CharSelectScene, LoadSceneMode.Additive);
                break;
            case GamePlayState.PlayGame:
                UnloadScene(m_CharSelectScene, () =>
                {
                    NetworkManager.SceneManager.LoadScene(m_MapScene, LoadSceneMode.Additive);
                });
                break;
            case GamePlayState.GameOver:
                Debug.Log("Load post scene");
                NetworkManager.SceneManager.LoadScene(m_PostScene, LoadSceneMode.Single);
                break;
        }
    }
    private void UnloadScene(string sceneName, Action onComplete)
    {
        StartCoroutine(UnloadSceneCoroutine(sceneName, onComplete));
    }

    private IEnumerator UnloadSceneCoroutine(string sceneName, Action onComplete)
    {
        var asyncUnload = NetworkManager.SceneManager.UnloadScene(SceneManager.GetSceneByName(sceneName));
        yield return asyncUnload;
        onComplete?.Invoke();
    }
    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }
        NetworkManager.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        m_countDownTimer.OnTimeExpired += CountDownTimer_OnTimeExpired;
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
        m_countDownTimer.OnTimeExpired -= CountDownTimer_OnTimeExpired;
    }
    private void CountDownTimer_OnTimeExpired()
    {
        if(currentGamePlayState == GamePlayState.PlayGame)
        {
            OnGameOver?.Invoke();
            IsGameOver.Value = true;
            Debug.Log("On GameOver");
            LoadState(GamePlayState.GameOver);
        }
    }
    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, System.Collections.Generic.List<ulong> clientsCompleted, System.Collections.Generic.List<ulong> clientsTimedOut)
    {
        if(sceneName == m_CharSelectScene)
        {
            NetworkServer.Instance.CreateGameSession();
            ClientPlayBGMRpc(2);
            m_countDownTimer.StartCountdown(m_CharSelectCountdownDuration);
            return;
        }
        else if(sceneName == m_MapScene)
        {
            ClientPlayBGMRpc(4);
            m_countDownTimer.StartCountdown(m_InGameCountdownDuration);
            return;
        }
        else if(sceneName == m_MainScene)
        {
            LoadState(GamePlayState.SelectCharacter);
        }
    }

    IEnumerator LoadGamePlayCoroutine(float time,GamePlayState state)
    {
        yield return new WaitForSeconds(time);
        LoadState(state);
    }
    public void LoadSceneDelay(GamePlayState state)
    {
        StartCoroutine(LoadGamePlayCoroutine(3,state));
    }
    [Rpc(SendTo.ClientsAndHost)]
    void ClientPlayBGMRpc(int num)
    {
        AudioManager.Instance.PlayBGMNumber(num);
    }
}
