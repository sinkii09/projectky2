using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    public GameObject popupPanel;
    public TextMeshProUGUI messageText;
    public Button confirmButton;
    public Button cancelButton;

    protected void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        popupPanel.SetActive(false);

        confirmButton.onClick.AddListener(() => { OnConfirmButtonClick(); AudioManager.Instance.PlaySFXNumber(0); });
        cancelButton.onClick.AddListener(() => { OnCancelButtonClick(); AudioManager.Instance.PlaySFXNumber(0); });
    }
    public virtual void ShowPopup(string message, UnityAction onConfirm = null, UnityAction onCancel = null)
    {
        messageText.text = message;
        confirmButton.gameObject.SetActive(true);
        cancelButton.gameObject.SetActive(true);
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            onConfirm?.Invoke();
            popupPanel.SetActive(false);
        });
        cancelButton.onClick.AddListener(() =>
        {
            onCancel?.Invoke();
            popupPanel.SetActive(false);
        });
        popupPanel.SetActive(true);
    }
    protected virtual void OnConfirmButtonClick()
    {

        popupPanel.SetActive(false);


    }

    protected virtual void OnCancelButtonClick()
    {

        popupPanel.SetActive(false);

    }
}
