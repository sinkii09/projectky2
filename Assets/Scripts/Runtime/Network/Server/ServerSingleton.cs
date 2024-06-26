using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Core;
using UnityEngine;

public class ServerSingleton : MonoBehaviour
{
    static ServerSingleton instance;
    public static ServerSingleton Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = FindObjectOfType<ServerSingleton>();
            if(instance == null ) return null;
            return instance;
        }
    }
    ServerGameManager m_GameManager;
    public ServerGameManager Manager
    {
        get
        {
            if (m_GameManager != null)
            {
                return m_GameManager;
            }
            return null;
        }
    }
    
    ServerToBackend m_ServerToBackend;
    public ServerToBackend ServerToBackend
    {
        get
        {
            if (m_ServerToBackend != null)
            {
                return m_ServerToBackend;
            }
            return null;
        }
    }
    public async Task CreateServer()
    {
        await UnityServices.InitializeAsync();

        m_GameManager = new ServerGameManager(
                ApplicationData.IP(),
                ApplicationData.Port(),
                ApplicationData.QPort(),
                NetworkManager.Singleton);
        m_ServerToBackend = new ServerToBackend();
    }
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void OnDestroy()
    {
        m_GameManager?.Dispose();
    }
}
