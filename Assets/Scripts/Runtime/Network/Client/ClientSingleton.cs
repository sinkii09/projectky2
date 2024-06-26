using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientSingleton : MonoBehaviour
{
    static ClientSingleton instance;

    ChatManager chatManager;

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

    public ChatManager ChatManager
    {
        get { return chatManager; }
        set { chatManager = value; }
    }

    public void CreateClient(Action InitCallback, LoginResponse reponse)
    {
        m_GameManager = new ClientGameManager(chatManager,InitCallback,reponse);
    }
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void findSynchData()
    {
    }
    public void ToLoginScene()
    {
        SceneManager.LoadScene("Login", LoadSceneMode.Single);
    }
    public void Logout()
    {
        Manager?.Dispose();
        ToLoginScene();
    }
    void OnDestroy()
    {
        Manager?.Dispose();
    }
}
