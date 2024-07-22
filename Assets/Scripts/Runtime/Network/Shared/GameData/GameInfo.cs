using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum Map
{
    Map1,
    Map2,
    Map3,
}
public enum PlayMode
{
    Default,
    Ranked   
}

public enum GameQueue
{
    Solo,
    Team
}

[Serializable]
public class GameInfo
{
    public Map map;
    public PlayMode gameMode;
    public GameQueue gameQueue;

    public int MaxUsers = 10;
    public string ToSceneName => ConvertToScene(map);

    const string k_MultiplayCasualQueue = "solo-queue";
    const string k_MultiplayCompetetiveQueue = "team-queue";

    static readonly Dictionary<string, GameQueue> k_MultiplayToLocalQueueNames = new Dictionary<string, GameQueue>
        {
            { k_MultiplayCasualQueue, GameQueue.Solo },
            { k_MultiplayCompetetiveQueue, GameQueue.Team }
        };

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("GameInfo: ");
        sb.AppendLine($"- map:        {map}");
        sb.AppendLine($"- gameMode:   {gameMode}");
        sb.AppendLine($"- gameQueue:  {gameQueue}");
        return sb.ToString();
    }

    public static string ConvertToScene(Map map)
    {
        switch (map)
        {
            case Map.Map1:
                return "Game_Map1";
            case Map.Map2:
                return "Game_Map2";
            default:
                Debug.LogWarning($"{map} - is not supported.");
                return "";
        }
    }

    public string ToMultiplayQueue()
    {
        return gameQueue switch
        {
            GameQueue.Solo => k_MultiplayCasualQueue,
            GameQueue.Team => k_MultiplayCompetetiveQueue,
            _ => k_MultiplayCasualQueue
        };
    }
    public static GameQueue ToGameQueue(string multiplayQueue)
    {
        if (!k_MultiplayToLocalQueueNames.ContainsKey(multiplayQueue))
        {
            Debug.LogWarning($"No QueuePreference that maps to {multiplayQueue}");
            return GameQueue.Solo;
        }

        return k_MultiplayToLocalQueueNames[multiplayQueue];
    }
}
