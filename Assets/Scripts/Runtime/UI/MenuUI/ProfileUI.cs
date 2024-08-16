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
        idText.text = $"Id: {user.UserId}";
    }
    void ChangeName()
    {
        nameInputField.gameObject.SetActive(true);
        nameInputField.text = nameText.text;
        nameText.text = "";
        cancelChangeBtn.gameObject.SetActive(true);
        changePasswordBtn.gameObject.SetActive(false);
        changeNameBtn.onClick.RemoveAllListeners();
        changeNameBtn.onClick.AddListener(SendChangedNameRequest);
        AudioManager.Instance.PlaySFX("Btn_click01");
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
        UserManager.Instance.ClientUpdateUser(ChangeNameConfirm,name:nameInputField.text);
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
        changePasswordBtn.gameObject.SetActive(true);
        changeNameBtn.onClick.RemoveAllListeners();
        changeNameBtn.onClick.AddListener(() => { ChangeName(); });
    }
    void CancelChangeName()
    {
        ChangeNameConfirm();
        AudioManager.Instance.PlaySFX("Btn_click01");
    }
    void ChangePassword()
    {
        changePasswordPopup.ShowPasswordPopup("Change password", ChangePasswordConfirm);
        AudioManager.Instance.PlaySFX("Btn_click01");
    }

    private void ChangePasswordConfirm(string oldPass, string newPass, string confirmPass)
    {
        if(string.IsNullOrEmpty(newPass) || string.IsNullOrEmpty(confirmPass) || string.IsNullOrEmpty(oldPass))
        {
            PopupManager.Instance.ShowPopup("Input Cannot be empty");
            return;
        }
        if(!string.Equals(newPass,confirmPass))
        {
            PopupManager.Instance.ShowPopup("Confirm password Incorrect");
            return;
        }
        UserManager.Instance.ClientUpdateUser(oldpassword: oldPass, newpassword: newPass,result:ChangeResult);
    }
    void ChangeResult(string result, bool success)
    {
        PopupManager.Instance.ShowPopup(result);
    }
    void Logout()
    {
        menuLogic.Logout();
        AudioManager.Instance.PlaySFX("Btn_click01");
    }
    void ToMainMenu()
    {
        menuLogic.HideProfile();
        AudioManager.Instance.PlaySFX("Btn_click01");
    }
}
