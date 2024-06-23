using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClient : IDisposable
{
    public event Action<ConnectStatus> OnLocalConnection;
    public event Action<ConnectStatus> OnLocalDisconnection;

    const int k_TimeoutDuration = 10;
    NetworkManager m_NetworkManager;

    DisconnectReason DisconnectReason { get; } = new DisconnectReason();
    public NetworkClient()
    {
        m_NetworkManager = NetworkManager.Singleton;
        m_NetworkManager.OnClientDisconnectCallback += RemoteDisconnect;
    }
    public void StartClient(string ipaddress, int port)
    {
        var unityTransport = m_NetworkManager.gameObject.GetComponent<UnityTransport>();
        unityTransport.SetConnectionData(ipaddress, (ushort)port);
        ConnectClient();
    }
    public void DisconnectClient()
    {
        DisconnectReason.SetDisconnectReason(ConnectStatus.UserRequestedDisconnect);
        NetworkShutdown();
    }
    private void ConnectClient()
    {
        var userData = ClientSingleton.Instance.Manager.User.Data;
        var payload = JsonUtility.ToJson(userData);

        var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

        m_NetworkManager.NetworkConfig.ConnectionData = payloadBytes;
        m_NetworkManager.NetworkConfig.ClientConnectionBufferTimeout = k_TimeoutDuration;

        //  If the socket connection fails, we'll hear back by getting an ReceiveLocalClientDisconnectStatus callback for ourselves and get a message telling us the reason
        //  If the socket connection succeeds, we'll get our  ReceiveLocalClientConnectStatus callback This is where game-layer failures will be reported.
        if (m_NetworkManager.StartClient())
        {
            Debug.Log("Starting Client!");
            NetworkMessenger.RegisterListener(NetworkMessage.LocalClientConnected,
                ReceiveLocalClientConnectStatus);
            NetworkMessenger.RegisterListener(NetworkMessage.LocalClientDisconnected,
                ReceiveLocalClientDisconnectStatus);
        }
        else
        {
            Debug.LogWarning($"Could not Start Client!");
            OnLocalDisconnection?.Invoke(ConnectStatus.Undefined);
        }
    }
    void ReceiveLocalClientConnectStatus(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out ConnectStatus status);
        Debug.Log("ReceiveLocalClientConnectStatus: " + status);

        //this indicates a game level failure, rather than a network failure. See note in ServerGameNetPortal.
        if (status != ConnectStatus.Success)
            DisconnectReason.SetDisconnectReason(status);

        OnLocalConnection?.Invoke(status);
    }
    void ReceiveLocalClientDisconnectStatus(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out ConnectStatus status);
        Debug.Log("ReceiveLocalClientDisconnectStatus: " + status);
        DisconnectReason.SetDisconnectReason(status);
    }
    void RemoteDisconnect(ulong clientId)
    {
        Debug.Log($"Got Client Disconnect callback for {clientId}");
        if (clientId == m_NetworkManager.LocalClientId)
            return;
        NetworkShutdown();
    }
    void NetworkShutdown()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
            SceneManager.LoadScene("MainMenu");
        if (m_NetworkManager.IsConnectedClient)
            m_NetworkManager.Shutdown(true);
        OnLocalDisconnection?.Invoke(DisconnectReason.Reason);
        NetworkMessenger.UnRegisterListener(NetworkMessage.LocalClientConnected);
        NetworkMessenger.UnRegisterListener(NetworkMessage.LocalClientDisconnected);
    }
    public void Dispose()
    {
        if (m_NetworkManager != null && m_NetworkManager.CustomMessagingManager != null)
        {
            m_NetworkManager.OnClientDisconnectCallback -= RemoteDisconnect;
        }
    }
}
