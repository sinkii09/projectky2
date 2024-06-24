using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MainMenuLogic;

public class MatchFindingUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timer_TMP;
    [SerializeField] Button CancelButton;

    [SerializeField] MainMenuLogic mainMenuLogic;


    private void Start()
    {
        mainMenuLogic.OnTimeLapse += UpdateTimer;
        CancelButton.onClick.AddListener(CancelMatchMake);
    }
    private void OnDestroy()
    {
        mainMenuLogic.OnTimeLapse -= UpdateTimer;
        CancelButton.onClick.RemoveListener(CancelMatchMake);
    }
    void UpdateTimer(int elapsedSeconds)
    {
        TimeSpan elapsedTime = TimeSpan.FromSeconds(elapsedSeconds);
        timer_TMP.text = (string.Format("{0:D2}:{1:D2}", elapsedTime.Minutes, elapsedTime.Seconds));
    }
    void CancelMatchMake()
    {
        mainMenuLogic.CancelMatchFinding();
    }

}
