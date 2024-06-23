using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendRequestCard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI idText;
    public string NameText => nameText.text;
    public string IdText => idText.text;

    [SerializeField] Button acceptButton;
    [SerializeField] Button cancelButton;
    
    
    UserManager userManager;
    private void Start()
    {
        acceptButton.onClick.AddListener(AcceptFriend);
        cancelButton.onClick.AddListener(DeniedRequest);
    }
    private void OnDestroy()
    {
        acceptButton.onClick.RemoveListener(AcceptFriend);
        cancelButton?.onClick.RemoveListener(DeniedRequest);
    }
    private void DeniedRequest()
    {
        userManager.DeniedFriendRequest(idText.text,Result);
    }

    private void AcceptFriend()
    {
        userManager.AcceptFriendRequest(idText.text,Result);
    }
    void Result(string result,bool isDestroy)
    {
        Debug.Log(result);
        if (isDestroy)
        {
            Destroy(gameObject);
        }
    }
    public void Initialize(string id, string name, UserManager userManager)
    {
        nameText.text = name;
        idText.text = id;
        this.userManager = userManager;
    }
}
