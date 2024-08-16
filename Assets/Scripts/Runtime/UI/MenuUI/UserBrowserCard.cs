using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class UserBrowserCard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    public string NameText => nameText.text;
    [SerializeField] TextMeshProUGUI idText;
    public string IdText => idText.text;

    UserManager userManager;


    [SerializeField] Button addFriendlButton;
    [SerializeField] Button chatButton;
    public void Initialize(string id, string name, UserManager userManager)
    {
        nameText.text = $"name: {name}";
        idText.text = $"id: {id}";
        this.userManager = userManager;

        CheckIfFriend();
    }
    void CheckIfFriend()
    {
        userManager.FetchFriendList(FetchFriendListResult,FetchFailed);
    }
    void FetchFriendListResult(List<FriendData> friends)
    {
        foreach (FriendData friend in friends)
        {
            if(string.Equals(friend.id,IdText))
            {
                addFriendlButton.gameObject.SetActive(false);
            }
        }
    }
    void FetchFailed(string error)
    {
        Debug.LogError(error);
    }
    private void Start()
    {
        addFriendlButton.onClick.AddListener(AddFriend);
    }

    private void OnDestroy()
    {
        addFriendlButton.onClick.RemoveListener(AddFriend);
    }

    private void AddFriend()
    {
        userManager.AddFriend(NameText, Result);
    }
    void Result(string result, bool isDestroy)
    {
        if (isDestroy)
        {
            addFriendlButton.interactable = false;
            addFriendlButton.GetComponentInChildren<TMP_Text>().text = "Requested";
        }
    }
}