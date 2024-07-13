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
        if (!IsServer && !IsSpawned) return;
        MainCountdown();
    }
    void MainCountdown()
    {
        if (!isStartCountdown) return;
        float elapsedTime = NetworkManager.LocalTime.TimeAsFloat - startTime;
        remainingTime.Value = Mathf.Max(0, countdownDuration - elapsedTime);
        UpdateTimerServer();
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        isStartCountdown = false;
    }

    public void StartCountdown(float duration)
    {
        Debug.Log("duration " + duration );
        countdownDuration = duration;
        startTime = NetworkManager.LocalTime.TimeAsFloat;
        isStartCountdown = true;
        Debug.Log("start countdown");
    }
    public float GetRemainingTime()
    {
        return remainingTime.Value;
    }
    void UpdateTimerServer()
    {
        if (remainingTime.Value > 0)
        {
            Debug.Log(remainingTime.Value);
            return;
        }
        Debug.Log("on time expired");
        OnTimeExpired?.Invoke();
        isStartCountdown = false;
    }
}
