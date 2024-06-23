using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static AuthenticateResponse;

public enum friendUIState
{
    friendList,
    friendRequest,
    findUser,
}
public class FriendListUI : MonoBehaviour
{
    [SerializeField] Transform viewContent;
    [SerializeField] FriendCard friendCardPrefab;
    [SerializeField] FriendRequestCard friendRequestCardPrefab;
    [SerializeField] UserBrowserCard userBrowserPrefab;

    [SerializeField] private Toggle friendListToggle;
    [SerializeField] private Toggle friendRequestToggle;
    [SerializeField] private Toggle findUserToggle;

    [SerializeField] ToggleGroup header;

    [SerializeField] Button browserButton;
    [SerializeField] TMP_InputField browserInputField;
    friendUIState state;
    MainMenuUI mainMenuUI;
    List<FriendCard> friendCardList = new List<FriendCard>();
    List<FriendRequestCard> requestCardList = new List<FriendRequestCard>();

    private void Start()
    {
        friendListToggle.onValueChanged.AddListener((isOn) => {
            if (isOn) SwitchUIState(friendUIState.friendList);
        });
        friendRequestToggle.onValueChanged.AddListener((isOn) => {
            if (isOn) SwitchUIState(friendUIState.friendRequest);
        });

        findUserToggle.onValueChanged.AddListener((isOn) => {
            if (isOn) SwitchUIState(friendUIState.findUser);
        });
        browserButton.onClick.AddListener(Browser);
    }
    private void OnDestroy()
    {
        friendListToggle.onValueChanged.RemoveListener((isOn) => {
            if (isOn) SwitchUIState(friendUIState.friendList);
        });
        friendRequestToggle.onValueChanged.RemoveListener((isOn) => {
            if (isOn) SwitchUIState(friendUIState.friendRequest);
        });

        findUserToggle.onValueChanged.RemoveListener((isOn) => {
            if (isOn) SwitchUIState(friendUIState.findUser);
        });
        browserButton.onClick.RemoveListener(Browser);
    }
    private void OnEnable()
    {
        mainMenuUI = FindObjectOfType<MainMenuUI>();
        if (ClientSingleton.Instance.Manager != null)
        {
            friendListToggle.isOn = true;
            SwitchUIState(friendUIState.friendList);
        }
    }
    void SwitchUIState(friendUIState state)
    {
        this.state = state;
        browserInputField.text = "";
        if (viewContent.GetComponentsInChildren<Transform>().Length > 0)
        {
            foreach (Transform child in viewContent)
            {
                Destroy(child.gameObject);
            }
        }
        switch (state)
        {
            case friendUIState.friendList:
                FetchFriendLists();
                break;
            case friendUIState.friendRequest:
                FetchFriendRequestList();
                break;
            case friendUIState.findUser:
                break;
        }
    }

    private void Browser()
    {
        if (string.IsNullOrEmpty(browserInputField.text))
        {
            SwitchUIState(state);
            return;
        }
        switch (state)
        {
            case friendUIState.friendList:
                foreach(var data in friendCardList)
                {
                    if(!string.Equals(data.IdText,browserInputField.text) && !string.Equals(data.NameText,browserInputField.text))
                    {
                        Destroy(data.gameObject);
                    }
                }
                break;
            case friendUIState.friendRequest:
                foreach (var data in requestCardList)
                {
                    if (!string.Equals(data.IdText, browserInputField.text) && !string.Equals(data.NameText, browserInputField.text))
                    {
                        Destroy(data.gameObject);
                    }
                }
                break; 
            case friendUIState.findUser:
                FindPlayer();
                break;
        }
    }
    private void FindPlayer()
    {
        User user = ClientSingleton.Instance.Manager.User;
        if (string.Equals(browserInputField.text, user.Name) || string.Equals(browserInputField.text, user.Data.userId))
        {
            Debug.Log("can find your self");
            return;
        }
        browserButton.interactable = false;
        mainMenuUI.FindPlayer(browserInputField.text, GetPlayerSuccess, GetPlayerFailed);
    }
    void GetPlayerSuccess(GetPlayerResponse response)
    {
        browserButton.interactable = true;
        foreach (Transform child in viewContent)
        {
            Destroy(child.gameObject);
        }
        var userCard = Instantiate(userBrowserPrefab,viewContent);
        userCard.Initialize(response.id, response.ingameName, mainMenuUI.UserManager);
    }
    void GetPlayerFailed(string error)
    {
        browserButton.interactable = true;
    }
    private void FetchFriendRequestList()
    {
        mainMenuUI.FetchFriendRequestList(FetchFriendRequetListSuccess,FetchFriendListFailed);
    }
    void FetchFriendRequetListSuccess(List<FriendData> requestList)
    {
        requestCardList.Clear();
        foreach (FriendData friendData in requestList)
        {
            var requestCard = Instantiate(friendRequestCardPrefab,viewContent);
            requestCard.Initialize(friendData.id,friendData.name,mainMenuUI.UserManager);
            requestCardList.Add(requestCard);
        }
    }
    void FetchFriendListSuccess(List<FriendData> friendList)
    {
        friendCardList.Clear();
        foreach (FriendData friendData in friendList)
        {
            var friendcard = Instantiate(friendCardPrefab, viewContent);
            friendcard.Initialize(friendData.id, friendData.name, mainMenuUI.UserManager);
            friendCardList.Add(friendcard);
        }
    }
    void FetchFriendListFailed(string error)
    {
        Debug.LogError(error);
    }
    private void FetchFriendLists()
    {
        mainMenuUI.FetchFriendList(FetchFriendListSuccess,FetchFriendListFailed);
    }
}
