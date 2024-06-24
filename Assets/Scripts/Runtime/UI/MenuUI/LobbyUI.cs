using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] Button PlayButton;
    [SerializeField] Button CancelButton;
    [SerializeField] MapScroller MapScroller;

    [SerializeField] MainMenuLogic mainMenuLogic;

    private void Start()
    {
        PlayButton.onClick.AddListener(PlayButtonPress);
        CancelButton.onClick.AddListener(BackToMenu);
    }
    private void OnDestroy()
    {
        PlayButton.onClick.RemoveAllListeners();
        CancelButton.onClick.RemoveAllListeners();
    }
    private void BackToMenu()
    {
        mainMenuLogic.ToMainMenu();
    }

    public void PlayButtonPress()
    {
        Map map = MapScroller.GetCurrentMap();
        mainMenuLogic.PlayButtonPressed(map);
    }
}
