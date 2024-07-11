using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkServer : IDisposable
{
    public static NetworkServer Instance { get; private set; }

    public Action<NetworkPlayer> OnServerPlayerSpawned;
    public Action<NetworkPlayer> OnServerPlayerDespawned;

    public Action<UserData> OnPlayerLeft;
    public Action<UserData> OnPlayerJoined;

    public int PlayerCount => m_NetworkManager.ConnectedClients.Count;
    SynchedServerData m_SynchedServerData;
    bool m_InitializedServer;
    NetworkManager m_NetworkManager;

    const int k_MaxConnectPayload = 1024;

    //private bool gameHasStarted;

    public Dictionary<string, UserData> m_ClientData = new Dictionary<string, UserData>();
    public Dictionary<ulong, string> m_NetworkIdToAuth = new Dictionary<ulong, string>();
    
    public List<UserData> UserDataList = new List<UserData>();
    public NetworkServer(NetworkManager manager)
    {
        m_NetworkManager = manager;
        m_NetworkManager.ConnectionApprovalCallback += ApprovalCheck;
        m_NetworkManager.OnServerStarted += OnNetworkReady;

        Instance = this;
    }
    public bool OpenConnection(string ip, int port, GameInfo startingGameInfo)
    {
        var unityTransport = m_NetworkManager.gameObject.GetComponent<UnityTransport>();
        m_NetworkManager.NetworkConfig.NetworkTransport = unityTransport;
        unityTransport.SetConnectionData(ip, (ushort)port);
        Debug.Log($"Starting server at {ip}:{port}\nWith: {startingGameInfo}");

        return m_NetworkManager.StartServer();
    }
    public async Task<SynchedServerData> ConfigureServer(GameInfo startingGameInfo)
    {
        m_NetworkManager.SceneManager.LoadScene(startingGameInfo.ToSceneName, LoadSceneMode.Single);

        var localNetworkedSceneLoaded = false;
        m_NetworkManager.SceneManager.OnLoadComplete += CreateAndSetSynchedServerData;

        void CreateAndSetSynchedServerData(ulong clientId, string sceneName, LoadSceneMode sceneMode)
        {
            if (clientId != m_NetworkManager.LocalClientId)
                return;
            localNetworkedSceneLoaded = true;
            
            m_NetworkManager.SceneManager.OnLoadComplete -= CreateAndSetSynchedServerData;
        }

        var waitTask = WaitUntilSceneLoaded();

        async Task WaitUntilSceneLoaded()
        {
            while (!localNetworkedSceneLoaded)
                await Task.Delay(50);
        }

        if (await Task.WhenAny(waitTask, Task.Delay(5000)) != waitTask)
        {
            Debug.LogWarning($"Timed out waiting for Server Scene Loading: Not able to Load Scene");
            return null;
        }
        m_SynchedServerData = GameObject.Instantiate(Resources.Load<SynchedServerData>("SynchedServerData"));
        m_SynchedServerData.GetComponent<NetworkObject>().Spawn();

        m_SynchedServerData.map.Value = startingGameInfo.map;
        m_SynchedServerData.gameMode.Value = startingGameInfo.gameMode;
        m_SynchedServerData.gameQueue.Value = startingGameInfo.gameQueue;
        Debug.Log(
            $"Synched Server Values: {m_SynchedServerData.map.Value} - {m_SynchedServerData.gameMode.Value} - {m_SynchedServerData.gameQueue.Value}",
            m_SynchedServerData.gameObject);
        return m_SynchedServerData;
    }
    public void CreateGameSession()
    {
        List<string> playerIdList = new List<string>();
        foreach (var user in UserDataList)
        {
            playerIdList.Add(user.userId);
        }
        ServerSingleton.Instance.ServerToBackend.CreateGameSession(m_SynchedServerData.gameMode.Value, playerIdList);
    }
    public void SetCharacter(ulong clientId, int characterId)
    {
        
        if (m_NetworkIdToAuth.TryGetValue(clientId, out string auth))
        {
            if (m_ClientData.TryGetValue(auth, out UserData data))
            {
                data.characterId = characterId;
            }
        }
    }
    public void SetPlayerResult(ulong clientId, int kill,int dead)
    {
        
        foreach (var data in m_ClientData.Values)
        {
            if(data.networkId == clientId)
            {
                data.playerKill = kill;
                data.playerDead = dead;
            }
        }
    }
    public void SendResultTobackend()
    {
        List<PlayerStats> stats = new List<PlayerStats>();
        Debug.Log("userDatalist count " + UserDataList.Count);
        foreach (var item in m_ClientData.Values)
        {
            PlayerStats playerStats = new PlayerStats
            {
                playerId = item.userId,
                kills = item.playerKill,
                deaths = item.playerDead
            };
            Debug.Log(item.userId);
            stats.Add(playerStats);
        }
        ServerSingleton.Instance.ServerToBackend.SendResultToBackend(stats);
    }
    public void StartGame()
    {
        Debug.Log("Start Game!!!");

        //NetworkManager.Singleton.SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
    }
    void OnNetworkReady()
    {
        m_NetworkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }
    void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (request.Payload.Length > k_MaxConnectPayload)
        {
            //Set response data
            response.Approved = false;
            response.CreatePlayerObject = false;
            response.Position = null;
            response.Rotation = null;
            response.Pending = false;

            Debug.LogError($"Connection payload was too big! : {request.Payload.Length} / {k_MaxConnectPayload}");
            return;
        }

        var payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        var userData = JsonUtility.FromJson<UserData>(payload);
        userData.networkId = request.ClientNetworkId;
        Debug.Log($"Host ApprovalCheck: connecting client: ({request.ClientNetworkId}) - {userData}");

        //Test for Duplicate Login.
        if (m_ClientData.ContainsKey(userData.userAuthId))
        {
            ulong oldClientId = m_ClientData[userData.userAuthId].networkId;
            Debug.Log($"Duplicate ID Found : {userData.userAuthId}, Disconnecting Old user");

            // kicking old client to leave only current
            SendClientDisconnected(request.ClientNetworkId, ConnectStatus.LoggedInAgain);
            WaitToDisconnect(oldClientId);
        }

        SendClientConnected(request.ClientNetworkId,ConnectStatus.Success);

        //Populate our dictionaries with the playerData
        m_NetworkIdToAuth[request.ClientNetworkId] = userData.userAuthId;
        m_ClientData[userData.userAuthId] = userData;
        UserDataList.Add(userData);
        OnPlayerJoined?.Invoke(userData);

        //Set response data
        response.Approved = true;
        response.CreatePlayerObject = true;
        response.Position = Vector3.zero;
        response.Rotation = Quaternion.identity;
        response.Pending = false;

        //connection approval will create a player object for you
        //Run an async 'fire and forget' task to setup the player network object data when it is intiialized, uses main thread context.
        var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
        Task.Factory.StartNew(
            async () => await SetupPlayerPrefab(request.ClientNetworkId, userData.userName),
            System.Threading.CancellationToken.None,
            TaskCreationOptions.None, scheduler
        );
    }
    private void OnClientDisconnect(ulong networkId)
    {
        SendClientDisconnected(networkId, ConnectStatus.GenericDisconnect);
        

        if (m_NetworkIdToAuth.TryGetValue(networkId, out var authId))
        {
            m_NetworkIdToAuth?.Remove(networkId);
            OnPlayerLeft?.Invoke(m_ClientData[authId]);
            if (m_ClientData[authId].characterId == -1)
            {
                UserDataList.Remove(m_ClientData[authId]);
            }
            if (m_ClientData[authId].networkId == networkId)
            {
                m_ClientData.Remove(authId);
            }
        }
        //var matchPlayerInstance = GetNetworkedPlayer(networkId);
        //OnServerPlayerDespawned?.Invoke(matchPlayerInstance);
    }

    async Task SetupPlayerPrefab(ulong networkId, string playerName)
    {
        NetworkObject playerNetworkObject;

        // Check player network object exists
        do
        {
            playerNetworkObject = m_NetworkManager.SpawnManager.GetPlayerNetworkObject(networkId);
            await Task.Delay(100);
        }
        while (playerNetworkObject == null);

        // get this client's player NetworkObject
        var networkedPlayer = GetNetworkedPlayer(networkId);
        networkedPlayer.PlayerName.Value = playerName;

        OnServerPlayerSpawned?.Invoke(networkedPlayer);
    }

    public NetworkPlayer GetNetworkedPlayer(ulong networkId)
    {
        var networkObject = m_NetworkManager.SpawnManager.GetPlayerNetworkObject(networkId);
        return networkObject.GetComponent<NetworkPlayer>();
    }

    async void WaitToDisconnect(ulong networkId)
    {
        await Task.Delay(500);
        m_NetworkManager.DisconnectClient(networkId);
    }
    void SendClientConnected(ulong networkId,ConnectStatus status)
    {

        var writer = new FastBufferWriter(sizeof(ConnectStatus), Allocator.Temp);
        writer.WriteValueSafe(status);
        Debug.Log($"Send Network Client Connected to : {networkId}");
        NetworkMessenger.SendMessageTo(NetworkMessage.LocalClientConnected, networkId, writer);
    }
    void SendClientDisconnected(ulong networkId, ConnectStatus status)
    {
        var writer = new FastBufferWriter(sizeof(ConnectStatus), Allocator.Temp);
        writer.WriteValueSafe(status);
        Debug.Log($"Send networkClient Disconnected to : {networkId}");
        NetworkMessenger.SendMessageTo(NetworkMessage.LocalClientDisconnected, networkId, writer);
    }
    public void Dispose()
    {
        if (m_NetworkManager == null)
            return;
        m_NetworkManager.ConnectionApprovalCallback -= ApprovalCheck;
        m_NetworkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        m_NetworkManager.OnServerStarted -= OnNetworkReady;
        if (m_NetworkManager.IsListening)
            m_NetworkManager.Shutdown();
    }
}
public struct ConnectionData
{
    public ConnectStatus Status;
    public string serverId;
}