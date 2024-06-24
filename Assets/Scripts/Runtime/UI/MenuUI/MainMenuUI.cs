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

}
