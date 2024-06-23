using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [HideInInspector]
    public NetworkVariable<NetworkString> PlayerName = new NetworkVariable<NetworkString>();

    public override void OnNetworkSpawn()
    {
        if (IsServer && !IsHost)
            return;

        ClientSingleton.Instance.Manager.AddMatchPlayer(this);
    }
    public override void OnNetworkDespawn()
    {
        if (IsServer && !IsHost)
            return;
        if (ApplicationData.IsServerUnitTest)
            return;

        ClientSingleton.Instance.Manager.RemoveMatchPlayer(this);
    }
}
