using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkSetup : MonoBehaviour
{
    public NetworkManager networkManager;

    void Start()
    {
        if (networkManager == null)
        {
            networkManager = FindObjectOfType<NetworkManager>();
        }

        // Set up NetworkConfig
        SetupNetworkConfig(networkManager.NetworkConfig);
    }

    private void SetupNetworkConfig(NetworkConfig config)
    {
        // Example setup for NetworkConfig
        config.ConnectionApproval = true;
        config.EnableSceneManagement = true;

        
    }
}
