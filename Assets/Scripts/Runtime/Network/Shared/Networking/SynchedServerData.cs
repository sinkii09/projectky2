using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SynchedServerData : NetworkBehaviour
{
    [HideInInspector]
    public NetworkVariable<NetworkString> serverID = new NetworkVariable<NetworkString>();
    public NetworkVariable<Map> map = new NetworkVariable<Map>();
    public NetworkVariable<PlayMode> gameMode = new NetworkVariable<PlayMode>();
    public NetworkVariable<GameQueue> gameQueue = new NetworkVariable<GameQueue>();

    public Action OnNetworkSpawned;
    public Action OnNetworkDeSpawned;

    public override void OnNetworkSpawn()
    {
        OnNetworkSpawned?.Invoke();
        if(IsClient)
        {
            JoinChat(serverID.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        OnNetworkDeSpawned?.Invoke();
        if (IsClient)
        {
            JoinChat("");
        }
    }
    async void JoinChat(string room)
    {
        Debug.Log($"join room {room}");
        await ChatManager.Instance.JoinRoomChat(room);
    }
}
