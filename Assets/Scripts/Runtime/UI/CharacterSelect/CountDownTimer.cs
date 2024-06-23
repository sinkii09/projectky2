using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CountDownTimer : NetworkBehaviour
{
    public event Action OnTimeExpired;
    
    private NetworkVariable<float> remainingTime = new NetworkVariable<float>();

    private float countdownDuration;
    private float startTime;
    private bool isStartCountdown;

    private void Update()
    {
        if (IsServer)
        {
            if (!isStartCountdown) return;
            float elapsedTime = NetworkManager.LocalTime.TimeAsFloat - startTime;
            remainingTime.Value = Mathf.Max(0, countdownDuration - elapsedTime);
            UpdateTimerServerRpc();
        }
    }

    public override void OnNetworkDespawn()
    {
        isStartCountdown = false;
    }

    public void StartCountdown(float duration)
    {
        countdownDuration = duration;
        startTime = NetworkManager.LocalTime.TimeAsFloat;
        isStartCountdown = true;
    }
    public float GetRemainingTime()
    {
        return remainingTime.Value;
    }
    [Rpc(SendTo.Server)]
    void UpdateTimerServerRpc()
    {
        if (remainingTime.Value > 0)
        {
            return;
        }
        OnTimeExpired?.Invoke();
        isStartCountdown = false;
    }
}
