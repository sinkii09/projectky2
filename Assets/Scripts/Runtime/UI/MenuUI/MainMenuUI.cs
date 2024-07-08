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
        rankedBtn.onClick.AddListener(() =>
        {
            GoToRankMode();
            AudioManager.Instance.PlaySFXNumber(0);
        });
        normalBtn.onClick.AddListener(() =>
        {
            GoToNormalMode();
            AudioManager.Instance.PlaySFXNumber(0);
        });
        cancelBtn.onClick.AddListener(() => { ExitApp(); AudioManager.Instance.PlaySFXNumber(0); });
    }
    private void OnDestroy()
    {
        rankedBtn.onClick.RemoveAllListeners();
        normalBtn.onClick.RemoveAllListeners();
        cancelBtn.onClick.RemoveAllListeners();
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
