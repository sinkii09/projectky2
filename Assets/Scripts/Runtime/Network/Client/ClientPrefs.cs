using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientPrefs
{
    public static void SetName(string name)
    {
        PlayerPrefs.SetString("player_name", name);
    }

    public static string PlayerName => PlayerPrefs.GetString("player_name");

    public static string GetGuid()
    {
        if (PlayerPrefs.HasKey("client_guid"))
        {
            return PlayerPrefs.GetString("client_guid");
        }

        var guid = System.Guid.NewGuid();
        var guidString = guid.ToString();

        PlayerPrefs.SetString("client_guid", guidString);
        return guidString;
    }
}
