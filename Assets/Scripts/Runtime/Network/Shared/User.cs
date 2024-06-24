using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class User 
{
    public UserData Data { get; }
    public Action<string> onNameChanged;

    public User(LoginResponse response)
    {
        var tempId = Guid.NewGuid().ToString();
        Data = new UserData(response.access_token,response.payload.sub,response.payload.ingameName, tempId, 0, new GameInfo());
    }
    public string AcessToken
    {
        get => Data.acesstoken;
    }
    public string UserId
    {
        get => Data.userId;
    }
    public string Name
    {
        get => Data.userName;
        set
        {
            Data.userName = value;
            onNameChanged?.Invoke(Data.userName);
        }
    }
    public string AuthId
    {
        get => Data.userAuthId;
        set => Data.userAuthId = value;
    }

    public Map MapPreferences
    {
        get => Data.userGamePreferences.map;
        set { Data.userGamePreferences.map = value; }
    }

    public PlayMode GameModePreferences
    {
        get => Data.userGamePreferences.gameMode;
        set => Data.userGamePreferences.gameMode = value;
    }

    public GameQueue QueuePreference
    {
        get => Data.userGamePreferences.gameQueue;
        set => Data.userGamePreferences.gameQueue = value;
    }

    public override string ToString()
    {
        var userData = new StringBuilder("User: ");
        userData.AppendLine($"- {Data}");
        return userData.ToString();
    }
}
