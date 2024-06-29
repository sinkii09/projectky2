using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSpawner : NetworkBehaviour,IDisposable
{
    [Header("References")]
    [SerializeField] private CharacterDatabase characterDatabase;

    [SerializeField]
    private Transform[] m_PlayerSpawnPoints;
    private List<Transform> m_PlayerSpawnPointsList = null;

    [SerializeField]
    private float m_ReviveDuration;


    public NetworkList<CharacterInGameState> Players;
    public List<NetworkObject> networkObjects;
    public Dictionary<ulong, ServerCharacter> PlayerCharacters = new Dictionary<ulong, ServerCharacter>();

    public event Action OnSpawnComplete;


    private void Awake()
    {
        Players = new NetworkList<CharacterInGameState>();
        networkObjects = new List<NetworkObject>();
    }

    public override void OnDestroy()
    {
        Dispose();
    }

    public override void OnNetworkSpawn()
    {
        if(!IsServer) return;
        SpawnPlayerCharacter();

        GamePlayBehaviour.Instance.OnGameOver += GamePlayerBehaviour_OnGameOver;
    }

    public override void OnNetworkDespawn()
    {
        
        foreach (NetworkObject obj in networkObjects)
        {
            Destroy(obj);
        }
        GamePlayBehaviour.Instance.OnGameOver += GamePlayerBehaviour_OnGameOver;
    }

    public void SpawnPlayerCharacter()
    {
        foreach (var client in NetworkServer.Instance.m_ClientData)
        {
            var character = characterDatabase.GetCharacterById(client.Value.characterId);
            if (character != null)
            {
                var spawnPos = GetRandomTransformInList().position;
                var characterInstance = Instantiate(character.GameplayPrefab, spawnPos, Quaternion.identity);
                characterInstance.SpawnAsPlayerObject(client.Value.networkId);
                
                AddToPlayerList(client.Value, characterInstance);
            }
        }
        LoadCompleteNotifyClientRpc();
    }
    private void AddToPlayerList(UserData data, NetworkObject characterInstance)
    {
        ServerCharacter serverCharacter = characterInstance.gameObject.GetComponent<ServerCharacter>();
        Players.Add(new CharacterInGameState(data.networkId,data.userName,data.characterId,true,serverCharacter.CharacterStats.BaseHP));
        networkObjects.Add(characterInstance);
        PlayerCharacters[data.networkId] = serverCharacter;
    }
    public void UpdateHealth(ulong id, int health,bool isAlive = true)
    {
        for(int i = 0; i < Players.Count; i++)
        {
            if (Players[i].ClientId == id)
            {
                Players[i] = new CharacterInGameState
                    (Players[i].ClientId,
                    Players[i].ClientName,
                    Players[i].CharacterId, 
                    isAlive, 
                    health,
                    Players[i].Kill,
                    isAlive ? Players[i].Dead : Players[i].Dead + 1);

                if (!Players[i].IsAlive && PlayerCharacters.ContainsKey(Players[i].ClientId))
                {
                    StartCoroutine(RevivePlayer(PlayerCharacters[Players[i].ClientId]));
                }
            }
        }
    }
    IEnumerator RevivePlayer(ServerCharacter serverCharacter)
    {
        yield return new WaitForSeconds(m_ReviveDuration);
        serverCharacter.Revive(GetRandomTransformInList().position);
    }
    public void UpdateKill(ulong id)
    {
        for (int i = 0;i < Players.Count;i++)
        {
            if (Players[i].ClientId == id)
            {
                Players[i] = new CharacterInGameState
                    (Players[i].ClientId,
                    Players[i].ClientName,
                    Players[i].CharacterId,
                    Players[i].IsAlive,
                    Players[i].Health,
                    Players[i].Kill + 1,
                    Players[i].Dead);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void LoadCompleteNotifyClientRpc()
    {
        OnSpawnComplete?.Invoke();
    }
    Transform GetRandomTransformInList()
    {
        Transform spawnPoint = null;
        if (m_PlayerSpawnPointsList == null || m_PlayerSpawnPointsList.Count == 0)
        {
            m_PlayerSpawnPointsList = new List<Transform>(m_PlayerSpawnPoints);
        }
        int index = UnityEngine.Random.Range(0, m_PlayerSpawnPointsList.Count);
        spawnPoint = m_PlayerSpawnPointsList[index];
        m_PlayerSpawnPointsList.RemoveAt(index);
        return spawnPoint;
    }
    private void GamePlayerBehaviour_OnGameOver()
    {
        foreach(var player in Players)
        {
            NetworkServer.Instance.SetPlayerResult(player.ClientId, player.Kill, player.Dead);
        }
        NetworkServer.Instance.SendResultTobackend();
    }

    public void Dispose()
    {
        Players.Dispose();
    }
}
