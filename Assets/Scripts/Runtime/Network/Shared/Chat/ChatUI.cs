using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    [SerializeField] ChatManager chatManager;
    [SerializeField] GameObject chatWindow;
    [SerializeField] GameObject redNote;
    [SerializeField] Toggle chatToggle;
    [SerializeField] Button sendTextBtn;
    [SerializeField] TMP_InputField chatInputField;
    [SerializeField] Transform chatContainer;
    [SerializeField] TextMeshProUGUI chatText;
    [SerializeField] ScrollRect scrollRect;

    List<TextMeshProUGUI> chatList = new List<TextMeshProUGUI>();
    public bool IsChating { get; private set; } 
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        chatWindow.SetActive(false);
    }
    private void Start()
    {
        chatToggle.onValueChanged.AddListener((isOn) => OnToggleValueChanged(isOn));
        sendTextBtn.onClick.AddListener(() => { SendText(); AudioManager.Instance.PlaySFXNumber(0); });
    }

    private void OnDestroy()
    {
        chatText.text = null;
        sendTextBtn.onClick.RemoveAllListeners();
    }
    private void Update()
    {
        if (!chatManager.IsInit)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!chatWindow.activeInHierarchy)
            {
                chatWindow.SetActive(true);
                EventSystem.current.SetSelectedGameObject(chatInputField.gameObject, null);
                chatInputField.OnPointerClick(new PointerEventData(EventSystem.current));
            }
            else
            {
                SendText();
            }
        }
        if (chatList.Count > 25)
        {
            Destroy(chatList[0]);
            chatList.RemoveAt(0);
        }
        if(chatWindow.activeInHierarchy)
        {
            IsChating = true;
        }
        else
        {
            IsChating= false;
        }
    }
    void SendText()
    {
        if (!string.IsNullOrWhiteSpace(chatInputField.text))
        {
            chatManager.SendChatMessage(chatInputField.text);
            chatInputField.text = string.Empty;
            EventSystem.current.SetSelectedGameObject(chatInputField.gameObject, null);
            chatInputField.OnPointerClick(new PointerEventData(EventSystem.current));
        }
    }
    public void ClearLog()
    {
        foreach (var chat in chatList)
        {
            Destroy(chat.gameObject);
        }
        chatList.Clear();
    }
    public void LogText(ChatMessage chatMessage)
    {
        var chat = Instantiate(chatText, chatContainer);
        chat.text += $"{chatMessage.SenderName}: {chatMessage.Message}\n";
        chatList.Add(chat);
        if(!chatWindow.activeInHierarchy && !redNote.activeInHierarchy)
        {
            redNote.SetActive(true);
        }
    }

    private void OnToggleValueChanged(bool isOn)
    {
        if(isOn)
        {
            redNote.SetActive(false);
        }
        chatWindow?.SetActive(isOn);
    }
}
