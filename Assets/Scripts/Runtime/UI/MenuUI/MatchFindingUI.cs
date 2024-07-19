using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchFindingUI : MonoBehaviour
{
    [SerializeField] Button PlayButton;
    [SerializeField] MapScroller MapScroller;
    [SerializeField] ToggleSwitch modeSwitch;
    [SerializeField] TextMeshProUGUI timer_TMP;

    [SerializeField] MainMenuLogic mainMenuLogic;
    

    bool isMatchMake;
    private void Start()
    {
        
        mainMenuLogic.OnTimeLapse += UpdateTimer;
        PlayButton.onClick.AddListener(() => { FindMatch(); AudioManager.Instance.PlaySFXNumber(0); });
        timer_TMP.text = "00:00";
    }
    private void OnDestroy()
    {
        mainMenuLogic.OnTimeLapse -= UpdateTimer;
        PlayButton.onClick.RemoveAllListeners();
    }

    public void FindMatch()
    {
        Map map = MapScroller.GetCurrentMap();
        mainMenuLogic.PlayButtonPressed(map);
        ToggleMatchMake(true);
    }
    public void CancelFindMatch()
    {
        mainMenuLogic.CancelMatchFinding();
        ToggleMatchMake(false);
    }
    void UpdateTimer(int elapsedSeconds)
    {
        TimeSpan elapsedTime = TimeSpan.FromSeconds(elapsedSeconds);
        timer_TMP.text = (string.Format("{0:D2}:{1:D2}", elapsedTime.Minutes, elapsedTime.Seconds));
    }

    void ToggleMatchMake(bool isMatchmake)
    {
        this.isMatchMake = isMatchmake;
        MapScroller.ToFindMatchState(isMatchmake);
        timer_TMP.gameObject.SetActive(isMatchmake);
        if(isMatchmake)
        {
            PlayButton.onClick.RemoveListener(FindMatch);
            PlayButton.onClick.AddListener(CancelFindMatch);
            PlayButton.GetComponentInChildren<TextMeshProUGUI>().text = "Cancel";
        }
        else
        {
            PlayButton.onClick.RemoveListener(CancelFindMatch);
            PlayButton.onClick.AddListener(FindMatch);
            PlayButton.GetComponentInChildren<TextMeshProUGUI>().text = "Play";
            timer_TMP.text = "00:00";
        }
    }

    public void GoToNormalMode()
    {
        Debug.Log("To Normal Mode");
        mainMenuLogic.PlayMode = PlayMode.Default;
    }

    public void GoToRankMode()
    {
        Debug.Log("To Rank Mode");

        mainMenuLogic.PlayMode = PlayMode.Ranked;
    }
}
