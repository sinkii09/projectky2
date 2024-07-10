using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Counter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI counterText;
    [SerializeField] float duration;
    float remaing;
    float startTime;
    bool isStart;
    private void FixedUpdate()
    {
        if(isStart)
        {
            float elapsedTime = Time.time - startTime;
            remaing = Mathf.Max(0, duration - elapsedTime);
            Debug.Log(remaing);
            TimeSpan span = TimeSpan.FromSeconds(remaing);
            counterText.text = span.Seconds.ToString();
        }
    }
    public void Show()
    {
        isStart = true;
        startTime = Time.time;
        gameObject.SetActive(true);
        
    }
    public void Hide()
    {
        isStart=false;
        gameObject.SetActive(false);
    }
}
