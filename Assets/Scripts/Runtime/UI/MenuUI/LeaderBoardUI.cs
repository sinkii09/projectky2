using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class LeaderBoardUI : ToggleWindow
{
    MainMenuLogic mainMenuLogic;

    [SerializeField] Transform content;
    [SerializeField] LeaderboardCard cardPrefab;
    [SerializeField] TextMeshProUGUI userOrderText;
    [SerializeField] TextMeshProUGUI userNameText;
    [SerializeField] TextMeshProUGUI userPointText;

    List<LeaderboardCard> cardList;
    private void Awake()
    {
        mainMenuLogic = FindObjectOfType<MainMenuLogic>();
        cardList = new List<LeaderboardCard>();
    }
    public override void Active(bool isActive)
    {
        base.Active(isActive);
        if(isActive )
        {
            mainMenuLogic.FetchLeaderBoard(FetchLeaderBoardSuccess, FetchLeaderBoardFailed);
            mainMenuLogic.FetchUserRank(FetchUserRankSuccess, FetchUserRankFailed);
        }
    }

    private void FetchUserRankSuccess(UserRank info)
    {
        userOrderText.text = ClientSingleton.Instance.Manager.User.Name;
        userOrderText.text = info.rank.ToString();
        userPointText.text = info.rankpoints.ToString();
    }

    private void FetchUserRankFailed()
    {
        userOrderText.text = string.Empty;
        userOrderText.text = string.Empty;
        userPointText.text = string.Empty;
    }
    private void FetchLeaderBoardSuccess(List<LeadUser> list)
    {
        foreach( LeaderboardCard card in cardList )
        {
            Destroy(card.gameObject);
        }
        foreach(var user in  list )
        {
            var card = Instantiate(cardPrefab,content);
            card.Initialize(user.rank, user.name, user.rankpoints);
            cardList.Add(card);
        }
    }
    private void FetchLeaderBoardFailed(string obj)
    {
        Debug.Log("Load LeaderboardFailed");
    }


}
