using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


public class MainMenuUI : MonoBehaviour
{
    [SerializeField] Button rankedBtn;
    [SerializeField] Button normalBtn;
    [SerializeField] Button cancelBtn;

    [SerializeField] MainMenuLogic mainMenuLogic;

    private void Start()
    {
        rankedBtn.onClick.AddListener(GoToRankMode);
        normalBtn.onClick.AddListener(GoToNormalMode);
        cancelBtn.onClick.AddListener(ExitApp);
    }
    private void OnDestroy()
    {
        rankedBtn.onClick.RemoveListener(GoToRankMode);
        normalBtn.onClick.RemoveListener(GoToNormalMode);
        cancelBtn.onClick.RemoveListener(ExitApp);
    }
    private void GoToNormalMode()
    {
        mainMenuLogic.PlayMode = PlayMode.Default;
        mainMenuLogic.ToLobby();
    }

    private void GoToRankMode()
    {
        mainMenuLogic.PlayMode = PlayMode.Ranked;
        mainMenuLogic.ToLobby();
    }
    private void ExitApp()
    {
        mainMenuLogic.ExitApplication();
    }
}
