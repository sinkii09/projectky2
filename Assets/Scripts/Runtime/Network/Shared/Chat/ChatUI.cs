using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    [SerializeField] ChatManager chatManager;
    [SerializeField] GameObject chatWindow;
    [SerializeField] Button sendChatBtn;
    [SerializeField] TMP_InputField chatInputField;
    [SerializeField] Transform chatContainer;
    [SerializeField] TextMeshProUGUI chatText;
    [SerializeField] ScrollRect scrollRect;

    List<TextMeshProUGUI> chatList = new List<TextMeshProUGUI>();
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        chatWindow.SetActive(false);
    }
    private void OnDestroy()
    {
        chatText.text = null;
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

                if(!string.IsNullOrWhiteSpace(chatInputField.text))
                {
                    chatManager.SendChatMessage(chatInputField.text);
                    chatInputField.text = string.Empty;
                    EventSystem.current.SetSelectedGameObject(chatInputField.gameObject, null);
                    chatInputField.OnPointerClick(new PointerEventData(EventSystem.current));
                }
                else
                {
                    chatWindow.SetActive(false);
                }
            }
        }
        if (chatList.Count > 25)
        {
            Destroy(chatList[0]);
            chatList.RemoveAt(0);
        }
    }
    public void LogText(ChatMessage chatMessage)
    {
        var chat = Instantiate(chatText, chatContainer);
        chat.text += $"{chatMessage.SenderName}: {chatMessage.Message}\n";
        chatList.Add(chat);
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            ScrollToBottom();
        });
    }
    private void ScrollToBottom()
    {
        //scrollRect.verticalNormalizedPosition = 0f;
    }
}
