using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using UnityEngine.VFX;

public class test : MonoBehaviour
{
    List<string> UserDataList;
    string domain = "https://projectky2-bdb1fda54766.herokuapp.com";
    private void Start()
    {
       UserDataList = new List<string>
       {
           "667567c4172f5b201ee7d84d",
           "66757d33f589298466f38b6d"
        };
        
    }
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            testss();
        }
    }


    void testss()
    {
        List<PlayerStats> stats = new List<PlayerStats>();
        Debug.Log("userDatalist count " + UserDataList.Count);
        foreach (var item in UserDataList)
        {
            PlayerStats playerStats = new PlayerStats
            {
                playerId = item,
                kills = 1,
                deaths = 2,
            };
            stats.Add(playerStats);
        }
        SendResultToBackend(stats);
    }
    public void SendResultToBackend(List<PlayerStats> playerStats)
    {
        Debug.Log("send result");
        PlayerStatsRequest request = new PlayerStatsRequest
        {
            playerStats = playerStats
        };
        StartCoroutine(SendResultRequest(request, "668ea91efb05f7c003bb1ec4")); 
    }
    IEnumerator SendResultRequest(PlayerStatsRequest playerStats, string sessionId)
    {
        Debug.Log("sessionId send: " + sessionId);
        string url = $"{domain}/game-sessions/update-player-stats/{sessionId}";

        string json = JsonUtility.ToJson(playerStats);
        PlayerStatsRequest log = JsonUtility.FromJson<PlayerStatsRequest>(json);
        Debug.Log(log.ToString());
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            Debug.Log("Response: " + request.downloadHandler.text);
        }
    }
}

