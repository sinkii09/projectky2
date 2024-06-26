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
    private void Awake()
    {
        loadingPanel.SetActive(true);
        FetchGameResult();
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

    private void OnFetchSuccess(GameSessionResult gameSessionResult)
    {
        loadingPanel.SetActive(false);
        List<PlayerResult> result = gameSessionResult.gameResult;
        foreach(var playerResult in result)
        {
            var card = Instantiate(playerCardPrefab, cardHolder);
            card.UpdateUI(playerResult);
            card.transform.SetSiblingIndex(playerResult.place - 1);
        }
    }
}
