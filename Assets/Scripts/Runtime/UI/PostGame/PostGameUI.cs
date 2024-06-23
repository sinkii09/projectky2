using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class PostGameUI : NetworkBehaviour
{
    [SerializeField] PostGamePlayerCard playerCardPrefab;
    [SerializeField] Transform cardHolder;

    NetworkList<CharacterPostGameState> Players;
    private void Awake()
    {
        Players = new NetworkList<CharacterPostGameState>();
    }

    public override void OnNetworkSpawn()
    {
        if(IsClient)
        {
            Players.OnListChanged += Players_OnListChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        if(IsClient)
        {
            Players.OnListChanged -= Players_OnListChanged;
        }
    }

    private void Players_OnListChanged(NetworkListEvent<CharacterPostGameState> changeEvent)
    {
        var card = Instantiate(playerCardPrefab, cardHolder);
        card.UpdateUI(changeEvent.Value);
        card.transform.SetSiblingIndex(changeEvent.Value.Place - 1);
    }

    public void SendDataToClient()
    {
        int i = 0;
        foreach (var player in NetworkServer.Instance.UserDataList)
        {
            i++;
            Debug.Log(i);
            
            Players.Add(new CharacterPostGameState(
                player.networkId,
                player.userName,
                player.characterId,
                player.playerKill,
                player.playerDead,
                player.playerScore,
                player.playerPlace,
                player.RankPoints));
        }
    }
    public void  OnExitBtnClick()
    {
        ClientSingleton.Instance.Manager.Disconnect();
    }

}
