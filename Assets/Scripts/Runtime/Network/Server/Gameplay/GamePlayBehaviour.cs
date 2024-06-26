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
    [SerializeField] CountDownTimer m_countDownTimer;
    [SerializeField] float m_CharSelectCountdownDuration;
    [SerializeField] float m_InGameCountdownDuration;
    private GamePlayState currentGamePlayState = GamePlayState.Undefined;

    public event Action OnGameOver;
    public bool IsGameOver;
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
                NetworkManager.SceneManager.LoadScene(m_MapScene, LoadSceneMode.Additive);
                break;
            case GamePlayState.GameOver:
                NetworkManager.SceneManager.LoadScene(m_PostScene, LoadSceneMode.Additive);
                break;
        }
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
        if(currentGamePlayState == GamePlayState.SelectCharacter)
        {

            

        }
        else if(currentGamePlayState == GamePlayState.PlayGame)
        {
            OnGameOver?.Invoke();
            IsGameOver = true;
            LoadSceneDelay(GamePlayState.GameOver);
        }
    }
    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, System.Collections.Generic.List<ulong> clientsCompleted, System.Collections.Generic.List<ulong> clientsTimedOut)
    {
        if(sceneName == m_CharSelectScene)
        {
            m_countDownTimer.StartCountdown(m_CharSelectCountdownDuration);
            return;
        }
        else if(sceneName == m_MapScene)
        {
            Debug.Log("load game scene");
            NetworkManager.SceneManager.UnloadScene(SceneManager.GetSceneByName(m_CharSelectScene));
            m_countDownTimer.StartCountdown(m_InGameCountdownDuration);
            return;
        }
        else if(sceneName == m_PostScene)
        {
            NetworkManager.SceneManager.UnloadScene(SceneManager.GetSceneByName(m_MapScene));
            return;
        }
        LoadState(GamePlayState.SelectCharacter);
    }

    IEnumerator LoadGamePlayCoroutine(float time,GamePlayState state)
    {
        yield return new WaitForSeconds(time);
        LoadState(state);
    }
    public void LoadSceneDelay(GamePlayState state)
    {
        StartCoroutine(LoadGamePlayCoroutine(1,state));
    }

}
