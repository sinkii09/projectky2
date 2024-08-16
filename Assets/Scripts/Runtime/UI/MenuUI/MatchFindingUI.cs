using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchFindingUI : MonoBehaviour
{
    [SerializeField] Button PlayButton;
    [SerializeField] Button CancelButton;
    [SerializeField] MapScroller MapScroller;
    [SerializeField] ToggleSwitch modeSwitch;
    [SerializeField] TextMeshProUGUI timer_TMP;
    [SerializeField] GameObject spinner;
    [SerializeField] MainMenuLogic mainMenuLogic;
    

    bool isMatchMake;
    private void Start()
    {
        
        mainMenuLogic.OnTimeLapse += UpdateTimer;
        mainMenuLogic.OnStartMatchMake += StartMatchMake;
        PlayButton.onClick.AddListener(() => { FindMatch(); AudioManager.Instance.PlaySFX("Btn_click01"); });
        CancelButton.onClick.AddListener(() => { CancelFindMatch(); AudioManager.Instance.PlaySFX("Btn_click01"); });
        timer_TMP.text = "00:00";
    }

    private void OnDestroy()
    {
        mainMenuLogic.OnTimeLapse -= UpdateTimer;
        mainMenuLogic.OnStartMatchMake -= StartMatchMake;
        PlayButton.onClick.RemoveAllListeners();
    }

    public void FindMatch()
    {
        Map map = MapScroller.GetCurrentMap();
        ToggleMatchMake(true);
        mainMenuLogic.PlayButtonPressed(map);
        
    }

    private void StartMatchMake()
    {
        spinner.SetActive(false);
        timer_TMP.gameObject.SetActive(true);
        CancelButton.gameObject.SetActive(true);
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
        PlayButton.gameObject.SetActive(!isMatchmake);
        if (isMatchmake)
        {
            spinner.SetActive(true);
        }
        else
        {
            CancelButton.gameObject.SetActive(false);
            timer_TMP.gameObject.SetActive(false);
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
