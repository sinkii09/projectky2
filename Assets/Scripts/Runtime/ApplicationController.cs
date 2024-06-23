using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    [SerializeField]
    ServerSingleton m_ServerPrefab;
    [SerializeField]
    ClientSingleton m_ClientPrefab;

    ApplicationData m_AppData;
    public static bool IsServer;
    async void Start()
    {
        DontDestroyOnLoad(gameObject);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        if (Application.isEditor)
            return;

        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }
    public void OnParrelSyncStarted(bool isServer, string cloneName)
    {
#pragma warning disable 4014
        LaunchInMode(isServer, cloneName);
#pragma warning restore 4014
    }

    async Task LaunchInMode(bool isServer, string profileName = "default")
    {
        m_AppData = new ApplicationData();
        IsServer = isServer;
        if (isServer)
        {
            var serverSingleton = Instantiate(m_ServerPrefab);
            await serverSingleton.CreateServer();

            var defaultGameInfo = new GameInfo
            {
                gameMode = GameMode.Default,
                map = Map.Map1,
                gameQueue = GameQueue.Solo
            };

            await serverSingleton.Manager.StartGameServerAsync(defaultGameInfo);
        }
        else
        {
            var clientSingleton = Instantiate(m_ClientPrefab);
            clientSingleton.ToMainMenu();
        }
    }

}
