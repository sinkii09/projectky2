using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientSingleton : MonoBehaviour
{

    static ClientSingleton instance;

    public static ClientSingleton Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = FindObjectOfType<ClientSingleton>();
            if (instance == null) return null;
            return instance;
        }
    }
    public ClientGameManager Manager
    {
        get
        {
            if (m_GameManager != null) return m_GameManager;
            return null;
        }
    }

    ClientGameManager m_GameManager;

    public void CreateClient(Action InitCallback, LoginResponse reponse)
    {
        m_GameManager = new ClientGameManager(InitCallback,reponse);
    }
    void Start()
    {
        DontDestroyOnLoad(gameObject);

    }

    public void FindSynchData()
    {
    }
    public void ToLoginScene()
    {
        SceneManager.LoadScene("Login", LoadSceneMode.Single);
    }
    public void Logout()
    {
        Manager?.Dispose();
        InventoryManager.Instance.Dispose();
        ShopManager.Instance.Dispose();
        ChatManager.Instance.OnLogout();
        ToLoginScene();
    }
    void OnDestroy()
    {
        Manager?.Dispose();
    }
}
