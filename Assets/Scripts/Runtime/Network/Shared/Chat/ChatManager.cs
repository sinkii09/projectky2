using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using System.Net.Sockets;
using System;
using System.Threading.Tasks;

public class ChatManager : MonoBehaviour
{
    private SocketIO client;

    private string url = "http://localhost:3000";
    private string roomId = string.Empty;
    public string RoomId { get; set; }
    User user;
    bool isInitialize;
    public bool IsInit => isInitialize;

    [SerializeField] ChatUI chatUI;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void InitializeClient(string token, User user)
    {
        this.user = user;
        client = new SocketIO(url, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
            {
                {"token", token }
            },
            EIO = 4,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        }) ;

        client.JsonSerializer = new NewtonsoftJsonSerializer();
        //client.ConnectAsync();

        client.OnConnected += async (sender, e) =>
        {
            Debug.Log("Connected to Server");
            await JoinRoomChat(roomId);
        };
        client.OnPing += (sender, e) =>
        {
            Debug.Log("Ping");
        };
        client.OnPong += (sender, e) =>
        {
            Debug.Log("Pong: " + e.TotalMilliseconds);
        };
        client.OnDisconnected += (sender, e) =>
        {
            Debug.Log("disconnect: " + e);
        };
        client.OnReconnectAttempt += (sender, e) =>
        {
            Debug.Log($"Reconnecting: attempt = {e}");
        };
        client.On("recentMessages", response =>
        {
            var messages = response.GetValue<List<ChatMessage>>();
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                chatUI.ClearLog();
            });
            foreach (var message in messages)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    chatUI.LogText(message);
                });
            }
        });
        client.On("chat message", (response) =>
        {
            var chatMessage = response.GetValue<ChatMessage>();
            Debug.Log("Received message from " + chatMessage.SenderName + ": " + chatMessage.Message);
            UnityMainThreadDispatcher.Instance().Enqueue(()=>
            {
                chatUI.LogText(chatMessage);
            });
        });
        try
        {
            client.ConnectAsync();
        }
        catch(Exception e)
        {
            throw new Exception(e.ToString());
        }
        isInitialize = true;
    }
    public async void SendChatMessage(string message)
    {
        if (client.Connected)
        {
            try
            {
                await client.EmitAsync("chat message", new {  message = message, roomId = roomId });
                Debug.Log("Message sent successfully: " + message);
            }
            catch (Exception ex)
            {
                ChatMessage chatMessage = new ChatMessage{Message = "Error sending message: " + ex.Message};
                chatUI.LogText(chatMessage);
            }
        }
        else
        {
            ChatMessage chatMessage = new ChatMessage { Message = "Cannot send message, client not connected" };
            chatUI.LogText(chatMessage);
        }
    }
    public async Task JoinRoomChat(string newRoomId)
    {
        try
        {
            await client.EmitAsync("joinRoom", new { oldRoomId = roomId, newRoomId = newRoomId  });
            this.roomId = newRoomId;
            Debug.Log("player success join room chat");
        }
        catch (Exception e)
        {
            ChatMessage chatMessage = new ChatMessage { Message = "Cannot join room chat - " + e };
            chatUI.LogText(chatMessage);
        }
    }
    internal async void LeaveRoomChatAsync(string roomId)
    {
        this.roomId = string.Empty;
        await client.EmitAsync("leaveRoom", new { roomId = roomId });
    }
    public void OnLogout()
    {
        LeaveRoomChatAsync(roomId);
        client.DisconnectAsync();
        Debug.Log("log out chat");
    }
}
public class ChatMessage
{
    public string SenderId { get; set; }
    public string SenderName { get; set; }
    public string RoomId { get; set; }
    public string Message { get; set; }

    public DateTime createdAt;
}
