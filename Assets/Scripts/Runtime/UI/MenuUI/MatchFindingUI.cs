using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchFindingUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timer_TMP;

    [SerializeField] MainMenuLogic mainMenuLogic;


    private void Start()
    {
        mainMenuLogic.OnTimeLapse += UpdateTimer;
    }
    private void OnDisable()
    {
        timer_TMP.text = "00:00";
    }
    private void OnDestroy()
    {
        mainMenuLogic.OnTimeLapse -= UpdateTimer;
    }
    void UpdateTimer(int elapsedSeconds)
    {
        TimeSpan elapsedTime = TimeSpan.FromSeconds(elapsedSeconds);
        timer_TMP.text = (string.Format("{0:D2}:{1:D2}", elapsedTime.Minutes, elapsedTime.Seconds));
    }
}
