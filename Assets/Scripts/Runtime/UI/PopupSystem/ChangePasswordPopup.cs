using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChangePasswordPopup : MonoBehaviour
{
    public TMP_InputField oldPasswordInput;
    public TMP_InputField newPasswordInput;
    public TMP_InputField confirmPassword;

    public GameObject popupPanel;
    public TextMeshProUGUI messageText;
    public Button confirmButton;
    public Button cancelButton;
    private void Start()
    {
        popupPanel.SetActive(false);
    }
    public void ShowPasswordPopup(string message, UnityAction<string,string,string> onConfirm, UnityAction onCancel = null)
    {
        messageText.text = message;
        confirmButton.gameObject.SetActive(true);
        cancelButton.gameObject.SetActive(true);
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            onConfirm?.Invoke(oldPasswordInput.text,newPasswordInput.text,confirmPassword.text);
            popupPanel.SetActive(false);
        });
        cancelButton.onClick.AddListener(() =>
        {
            onCancel?.Invoke();
            popupPanel.SetActive(false);
        });
        popupPanel.SetActive(true);
    }
}
