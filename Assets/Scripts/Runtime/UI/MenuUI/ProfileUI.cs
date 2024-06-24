using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    [SerializeField] Button changeNameBtn;
    [SerializeField] Button cancelChangeBtn;
    [SerializeField] Button changePasswordBtn;
    [SerializeField] Button logoutBtn;
    [SerializeField] Button backBtn;
    [SerializeField] Button findPlayerBtn;

    [SerializeField] ChangePasswordPopup changePasswordPopup;

    [SerializeField] TextMeshProUGUI idText;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TMP_InputField nameInputField;

    [SerializeField] MainMenuLogic menuLogic;

    User user;

    private void Start()
    {
        changeNameBtn.onClick.AddListener(ChangeName);
        cancelChangeBtn.onClick.AddListener(CancelChangeName);
        changePasswordBtn.onClick.AddListener(ChangePassword);
        logoutBtn.onClick.AddListener(Logout);
        backBtn.onClick.AddListener(ToMainMenu);

        nameInputField.gameObject.SetActive(false);
        cancelChangeBtn.gameObject.SetActive(false);
    }
    public void SetUserProfile(User user)
    {
        this.user = user;
        nameText.text = user.Name;
        idText.text = user.UserId;
    }
    void ChangeName()
    {
        nameInputField.gameObject.SetActive(true);
        nameInputField.text = nameText.text;
        nameText.text = "";
        cancelChangeBtn.gameObject.SetActive(true);
        changeNameBtn.onClick.RemoveAllListeners();
        changeNameBtn.onClick.AddListener(SendChangedNameRequest);
    }
    void SendChangedNameRequest()
    {
        if(string.IsNullOrEmpty(nameInputField.text))
        {
            ChangeNameConfirm("Cannot input blank");
            return;
        }
        if(string.Equals(nameInputField.text,user.Name))
        {
            ChangeNameConfirm("please input other name");
            return;
        }
        UserManager.Instance.UpdateUser(ChangeNameConfirm,name:nameInputField.text);
    }
    void ChangeNameConfirm(string result = "",bool success = false)
    {
        if(success)
        {
            user.Name = nameInputField.text;
        }
        if(!string.IsNullOrEmpty(result))
        {
            PopupManager.Instance.ShowPopup(result);   
        }
        nameText.text = user.Name;
        nameInputField.text = "";
        nameInputField.gameObject.SetActive(false);
        cancelChangeBtn.gameObject.SetActive(false);
        changeNameBtn.onClick.RemoveAllListeners();
        changeNameBtn.onClick.AddListener(() => { ChangeName(); });
    }
    void CancelChangeName()
    {
        ChangeNameConfirm();
    }
    void ChangePassword()
    {
        changePasswordPopup.ShowPasswordPopup("Change password", ChangePasswordConfirm);
    }

    private void ChangePasswordConfirm(string oldPass, string newPass)
    {
        UserManager.Instance.UpdateUser(oldpassword: oldPass, newpassword: newPass,result:ChangeResult);
    }
    void ChangeResult(string result, bool success)
    {
        PopupManager.Instance.ShowPopup(result);
    }
    void Logout()
    {
        menuLogic.Logout();
    }
    void ToMainMenu()
    {
        menuLogic.HideProfile();
    }
}
