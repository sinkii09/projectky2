using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager : IDisposable
{
    public event Action<NetworkPlayer> MatchPlayerSpawned;
    public event Action<NetworkPlayer> MatchPlayerDespawned;
    public User User { get; private set; }
    public NetworkClient NetworkClient { get; private set; }
    public Matchmaker Matchmaker { get; private set; }
    public ChatManager ChatManager { get; private set; }
    public bool Initialized { get; private set; } = false;

    private AuthState authResult = AuthState.NotAuthenticated;

    string connectIp = string.Empty;
    private string connectPort = string.Empty;
    public ClientGameManager(ChatManager chatManager,Action InitCallback,LoginResponse response,string profileName = "default")
    {
        User = new User(response);
        Debug.Log($"Beginning with new Profile:{User.UserId}");
        Debug.Log($"token: {User.AcessToken}");
        UserManager.Instance.AccessToken = User.AcessToken;
        
        ChatManager = chatManager;
        ChatManager.InitializeClient(User.AcessToken, User);

#pragma warning disable 4014
        InitAsync(InitCallback);
#pragma warning restore 4014
    }
    
    async Task InitAsync(Action InitCallback)
    {
        var unityAuthenticationInitOptions = new InitializationOptions();
        unityAuthenticationInitOptions.SetProfile($"{User.UserId}{LocalProfileTool.LocalProfileSuffix}");
        await UnityServices.InitializeAsync(unityAuthenticationInitOptions);

        NetworkClient = new NetworkClient();
        Matchmaker = new Matchmaker();
        authResult = await AuthenticationWrapper.DoAuth(User.UserId);
        
        if (authResult == AuthState.Authenticated)
            User.AuthId = AuthenticationWrapper.PlayerID();
        else
            User.AuthId = Guid.NewGuid().ToString();
        Debug.Log($"did Auth?{authResult} {User.AuthId}");
        Initialized = true;
        InitCallback?.Invoke();

       
    }

    public void BeginConnection(string ip, int port)
    {
        Debug.Log($"Starting networkClient @ {ip}:{port}\nWith : {User}");
        NetworkClient.StartClient(ip, port);
        connectIp = ip;
        connectPort = port.ToString();
    }

    public void Disconnect()
    {
        NetworkClient.DisconnectClient();
    }
    public async Task MatchmakeAsync(Action<int> onMatchmakerTicked, Action<MatchmakerPollingResult> onMatchmakerResponse = null)
    {
        if (Matchmaker.IsMatchmaking)
        {
            Debug.LogWarning("Already matchmaking, please wait or cancel.");
            return;
        }

        var matchResult = await GetMatchAsync(onMatchmakerTicked);
        onMatchmakerResponse?.Invoke(matchResult);
    }

    public async Task CancelMatchmaking()
    {
        await Matchmaker.CancelMatchmaking();
    }

    public void AddMatchPlayer(NetworkPlayer player)
    {
        MatchPlayerSpawned?.Invoke(player);
    }

    public void RemoveMatchPlayer(NetworkPlayer player)
    {
        MatchPlayerDespawned?.Invoke(player);
    }

    public void SetGameMode(PlayMode gameMode)
    {
        User.GameModePreferences = gameMode;
    }

    public void SetGameMap(Map map)
    {
        User.MapPreferences = map;
    }

    public void SetGameQueue(GameQueue queue)
    {
        User.QueuePreference = queue;
    }
    async Task<MatchmakerPollingResult> GetMatchAsync(Action<int> onMatchmakerTicked)
    {
        Debug.Log($"Beginning Matchmaking with {User}");
        var matchmakingResult = await Matchmaker.Matchmake(User.Data, onMatchmakerTicked);

        if (matchmakingResult.result == MatchmakerPollingResult.Success)
            BeginConnection(matchmakingResult.ip, matchmakingResult.port);
        else
            Debug.LogWarning($"{matchmakingResult.result} : {matchmakingResult.resultMessage}");

        return matchmakingResult.result;
    }

    public void Dispose()
    {
        if (authResult == AuthState.Authenticated)
        {
            AuthenticationWrapper.SignOut();
        }
        NetworkClient?.Dispose();
        Matchmaker?.Dispose();
        ChatManager.OnLogout();
    }
}
