using System;
using Unity.Netcode;


public class NetcodeHooks : NetworkBehaviour
{
    public event Action OnNetworkSpawnHook;

    public event Action OnNetworkDespawnHook;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        OnNetworkSpawnHook?.Invoke();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        OnNetworkDespawnHook?.Invoke();
    }
}
