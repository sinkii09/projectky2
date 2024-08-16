using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum GamePlayState { Undefined, SelectCharacter, Standby ,PlayGame, GameOver }
public class GameStateManager : NetworkBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public NetworkVariable<GamePlayState> CurrentGamePlayState { get; private set; } = new NetworkVariable<GamePlayState>(GamePlayState.Undefined);

    public  event Action<GamePlayState, GamePlayState> OnGamePlayStateChanged;
    private void Awake()
    {
        Instance = this;

    }
    public override void OnNetworkSpawn()
    {
        CurrentGamePlayState.OnValueChanged += HandleGamePlayStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        CurrentGamePlayState.OnValueChanged -= HandleGamePlayStateChanged;
    }

    private void HandleGamePlayStateChanged(GamePlayState previousValue, GamePlayState newValue)
    {
        OnGamePlayStateChanged?.Invoke(previousValue, newValue);
    }

    public void SetGamePlayState(GamePlayState newState)
    {
        if(IsServer)
        {
            CurrentGamePlayState.Value = newState;
        }
    }
}
