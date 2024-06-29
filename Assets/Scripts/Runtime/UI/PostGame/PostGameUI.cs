using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PostGameUI : MonoBehaviour
{
    [SerializeField] PostGamePlayerCard playerCardPrefab;
    [SerializeField] Transform cardHolder;
    [SerializeField] GameObject loadingPanel;
    [SerializeField] Button ExitButton;
    private void Awake()
    {
        if (!ClientSingleton.Instance) return;
        loadingPanel.SetActive(true);
        FetchGameResult();
    }
    private void Start()
    {
        ExitButton.onClick.AddListener(DisconNect);
    }
    private void FetchGameResult()
    {
        string id = ClientSingleton.Instance.Manager.User.UserId;
        UserManager.Instance.ClientFetchGameResult(id, OnFetchSuccess, OnFetchFailed);
    }

    private void OnFetchFailed()
    {
        loadingPanel.SetActive(false);
        Debug.Log("Can not connect to Server");
    }

    private void OnFetchSuccess(GameSessionResult gameSessionResult,List<PlayerResult> results)
    {
        loadingPanel.SetActive(false);
        foreach(var playerResult in gameSessionResult.gameResult)
        {
            var card = Instantiate(playerCardPrefab, cardHolder);
            card.UpdateUI(playerResult);
            card.transform.SetSiblingIndex(playerResult.place - 1);
        }
    }
    public void DisconNect()
    {
        ClientSingleton.Instance.Manager.Disconnect();
    }
}
