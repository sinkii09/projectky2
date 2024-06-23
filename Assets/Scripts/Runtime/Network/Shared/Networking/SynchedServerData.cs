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
    public NetworkVariable<GameMode> gameMode = new NetworkVariable<GameMode>();
    public NetworkVariable<GameQueue> gameQueue = new NetworkVariable<GameQueue>();

    public Action OnNetworkSpawned;

    public override void OnNetworkSpawn()
    {
        OnNetworkSpawned?.Invoke();
    }
}
