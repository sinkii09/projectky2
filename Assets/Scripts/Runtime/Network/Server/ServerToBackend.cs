using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class ServerToBackend 
{
    string domain = "https://projectky2-bdb1fda54766.herokuapp.com";
    public string token {  get; set; }
    public string sessionId { get; private set; }
    public void ServerSignIn(string serverId)
    {
        CoroutineRunner.Instance.StartCoroutine(ServerSignInRequest(serverId));
    }
    IEnumerator ServerSignInRequest(string serverId)
    {
        string url = $"{domain}/auth/serverSignIn/:{serverId}";
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseJson = request.downloadHandler.text;
                AccessToken token = JsonUtility.FromJson<AccessToken>(responseJson);
                this.token = token.access_token;
                Debug.Log("server signin success");
            }
            else
            {
                Debug.Log("server signin failed");
            }
        }
    }

    public void CreateGameSession(PlayMode mode, List<string> userIdList)
    {
        GameSessionRequest gameSessionRequest = new GameSessionRequest
        {
            gameMode = mode.ToString(),
            playerIds = userIdList
        };
        CoroutineRunner.Instance.StartCoroutine(CreateGameSessionRequest(gameSessionRequest));
    }
    IEnumerator CreateGameSessionRequest(GameSessionRequest body)
    {
        string url = $"{domain}/game-sessions/create-session";
        string json = JsonUtility.ToJson(body);
        byte[] requestBodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(requestBodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {token}");
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                sessionId = request.downloadHandler.text;
                Debug.Log("sessionId receive: " + sessionId);
            }
            else
            {
                Debug.Log("create session failed");
            }
        }
    }

    public void SendResultToBackend(List<PlayerStats> playerStats)
    {
        PlayerStatsRequest request = new PlayerStatsRequest
        {
            playerStats = playerStats
        };
        CoroutineRunner.Instance.StartCoroutine(SendResultRequest(request, sessionId));
    }
    IEnumerator SendResultRequest(PlayerStatsRequest playerStats, string sessionId)
    {
        sessionId = sessionId.Trim('\"');
        Debug.Log("sessionId send: " + sessionId);
        string url = $"{domain}/game-sessions/update-player-stats/{sessionId}";

        string json = JsonUtility.ToJson(playerStats);
        List<PlayerStats> log = JsonUtility.FromJson<PlayerStatsRequest>(json).playerStats;
        foreach(var item in log)
        {
            Debug.Log($"{item.playerId} + {item.kills} + {item.deaths}");
        }
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else if (request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error222: " + request.error);
        }
        else
        {
            Debug.Log("Response: " + request.downloadHandler.text);
        }
    }
}
[SerializeField]
[System.Serializable]
public class GameSessionRequest
{
    public string gameMode;
    public List<string> playerIds;
}
[System.Serializable]
public class PlayerStats
{
    public string playerId;
    public int kills;
    public int deaths;
}

[System.Serializable]
public class PlayerStatsRequest
{
    public List<PlayerStats> playerStats;
}
