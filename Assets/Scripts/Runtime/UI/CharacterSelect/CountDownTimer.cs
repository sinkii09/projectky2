using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
public enum CountDownType
{
    None,
    Ready,
    GamePlay
}
public class CountDownTimer : NetworkBehaviour
{
    public event Action OnTimeExpired;
    
    private NetworkVariable<float> remainingTime = new NetworkVariable<float>();

    private float countdownDuration;
    public CountDownType countDownType = CountDownType.None;

    private float start =0;
    private bool isStartCountdown;

    private void Update()
    {
        if (!IsServer) return;
        if (!isStartCountdown) return;
        start += Time.deltaTime;
        remainingTime.Value = Mathf.Max(0,countdownDuration - start);
        if (start > countdownDuration)
        {
            isStartCountdown = false;
            OnTimeExpired?.Invoke();
        }
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
        Debug.Log($"countdown duration {duration}");
        countdownDuration = duration;
        start = 0;
        isStartCountdown = true;
    }
    public float GetRemainingTime()
    {
        return remainingTime.Value;
    }
}
