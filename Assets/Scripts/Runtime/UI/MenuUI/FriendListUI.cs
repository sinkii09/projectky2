using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public enum friendUIState
{
    friendList,
    friendRequest,
    findUser,
}
public class FriendListUI : ToggleWindow
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
    MainMenuLogic mainMenuUI;
    List<FriendCard> friendCardList = new List<FriendCard>();
    List<FriendRequestCard> requestCardList = new List<FriendRequestCard>();

    private void Awake()
    {
        mainMenuUI = FindObjectOfType<MainMenuLogic>();
    }
    private void Start()
    {
        friendListToggle.onValueChanged.AddListener((isOn) => {
            {
                SwitchUIState(friendUIState.friendList);
                AudioManager.Instance.PlaySFX("Btn_click01");   
            }
        });
        friendRequestToggle.onValueChanged.AddListener((isOn) => {
            if (isOn)
            {
                SwitchUIState(friendUIState.friendRequest);
                AudioManager.Instance.PlaySFX("Btn_click01");
            }
        });

        findUserToggle.onValueChanged.AddListener((isOn) => {
            if (isOn)
            {
                SwitchUIState(friendUIState.findUser);
                AudioManager.Instance.PlaySFX("Btn_click01");
            }
        });
        browserButton.onClick.AddListener(() => { Browser(); AudioManager.Instance.PlaySFX("Btn_click01"); });

    }
    private void OnDestroy()
    {
        friendListToggle.onValueChanged.RemoveListener((isOn) =>
        {
            if (isOn)
            {
                SwitchUIState(friendUIState.friendList);
                AudioManager.Instance.PlaySFX("Btn_click01");
            }
        });
        friendRequestToggle.onValueChanged.RemoveListener((isOn) => {
            if (isOn)
            {
                SwitchUIState(friendUIState.friendRequest);
                AudioManager.Instance.PlaySFX("Btn_click01");
            }
        });

        findUserToggle.onValueChanged.RemoveListener((isOn) => {
            if (isOn) 
            { 
                SwitchUIState(friendUIState.findUser);
                AudioManager.Instance.PlaySFX("Btn_click01");
            }
        });
        browserButton.onClick.RemoveListener(Browser);
    }
    private void OnEnable()
    {
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
        userCard.Initialize(response.id, response.ingameName, UserManager.Instance);
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
            requestCard.Initialize(friendData.id,friendData.name,UserManager.Instance);
            requestCardList.Add(requestCard);
        }
    }
    void FetchFriendListSuccess(List<FriendData> friendList)
    {
        friendCardList.Clear();
        foreach (FriendData friendData in friendList)
        {
            var friendcard = Instantiate(friendCardPrefab, viewContent);
            friendcard.Initialize(friendData.id, friendData.name, UserManager.Instance);
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
