using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class UserData
{
    public string acesstoken;
    public string userId;
    public string userName;
    public string userAuthId;
    public ulong networkId;
    public GameInfo userGamePreferences;
    public int RankPoints = 0;
    public int Rating = 100;
    public int characterId = -1;
    public int playerKill = 0;
    public int playerDead = 0;
    public int playerScore = 0;
    public int playerGold = 0;
    public int playerPlace = 0;
    public UserData(LoginResponse response, string userAuthId, ulong networkId, GameInfo userGamePreferences)
    {
        this.acesstoken = response.access_token;
        this.userId = response.payload.id;
        this.userName = response.payload.ingameName;
        playerGold = response.payload.gold;
        this.userAuthId = userAuthId;
        this.networkId = networkId;
        this.userGamePreferences = userGamePreferences;
    }
    public UserData(string userName,ulong networkId, int rating, int rankPoints) 
    {
        RankPoints = rankPoints;
        Rating = rating;
        this.userName = userName;
        this.networkId = networkId;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("UserData: ");
        sb.AppendLine($"- User Name:             {userName}");
        sb.AppendLine($"- User Auth Id:          {userAuthId}");
        sb.AppendLine($"- User Game Preferences: {userGamePreferences}");
        return sb.ToString();
    }
}
