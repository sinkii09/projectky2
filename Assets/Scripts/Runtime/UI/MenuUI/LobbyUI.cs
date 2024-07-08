using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] Button PlayButton;
    [SerializeField] Button BackButton;
    [SerializeField] Button CancelButton;
    [SerializeField] MapScroller MapScroller;
    [SerializeField] MatchFindingUI MatchFindingUI;

    [SerializeField] MainMenuLogic mainMenuLogic;


    bool isMatchMake;
    private void Start()
    {
        ToggleMatchMake(false);
        PlayButton.onClick.AddListener(() => { FindMatch(); AudioManager.Instance.PlaySFXNumber(0); });
        BackButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySFXNumber(0);
            ExitRoom();
        });
        CancelButton.onClick.AddListener(() =>
        {

            AudioManager.Instance.PlaySFXNumber(0);
            CancelFindMatch();
        });
    }
    private void OnEnable()
    {
        ToggleMatchMake(false);
    }
    private void OnDestroy()
    {
        PlayButton.onClick.RemoveAllListeners();
        BackButton.onClick.RemoveAllListeners();
    }
    public void FindMatch()
    {
        ToggleMatchMake(true);
        Map map = MapScroller.GetCurrentMap();
        mainMenuLogic.PlayButtonPressed(map);
    }
    public void CancelFindMatch()
    {
        mainMenuLogic.CancelMatchFinding();
        ToggleMatchMake(false);
    }
    public void ExitRoom()
    {
        if(isMatchMake)
        {
            CancelFindMatch();
        }
        mainMenuLogic.ToMainMenu();
    }
    void ToggleMatchMake(bool isMatchmake)
    {
        this.isMatchMake = isMatchmake;
        MatchFindingUI.gameObject.SetActive(isMatchmake);
        MapScroller.gameObject.SetActive(!isMatchmake);
    }
}
