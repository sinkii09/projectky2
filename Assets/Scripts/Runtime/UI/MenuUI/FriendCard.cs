using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendCard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    public string NameText => nameText.text;
    [SerializeField] TextMeshProUGUI idText;
    public string IdText => idText.text;

    [SerializeField] Button chatButton;
    [SerializeField] Button cancelButton;


    UserManager userManager;
    private void Start()
    {

        cancelButton.onClick.AddListener(DeleteFriend);
    }
    private void OnDestroy()
    {
        cancelButton.onClick.RemoveListener(DeleteFriend);
    }
    private void DeleteFriend()
    {
        userManager.DeleteFriend(idText.text,Result);
        cancelButton.interactable = false;
    }

    void Result(string result,bool isDestroy)
    {
        if (isDestroy)
        {
            Destroy(gameObject);
        }
        else
        {
            cancelButton.interactable = true;
        }
    }
    public void Initialize(string id, string name, UserManager userManager)
    {
        nameText.text = name;
        idText.text = id;
        this.userManager = userManager;
    }
}
