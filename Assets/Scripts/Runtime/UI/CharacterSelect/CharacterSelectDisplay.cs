using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectDisplay : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Camera overlayCAM;
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private Transform charactersHolder;
    [SerializeField] private CharacterSelectButton selectButtonPrefab;
    [SerializeField] private PlayerCard[] playerCards;
    [SerializeField] private GameObject characterInfoPanel;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private Transform[] introSpawnPointArray;
    [SerializeField] private Button lockInButton;
    [SerializeField] private TextMeshProUGUI countdownText;
    private CountDownTimer countDownTimer;
    private Dictionary<CharacterSelectState,Transform> introPointDictionary = new Dictionary<CharacterSelectState,Transform>();
    private Dictionary<ulong,GameObject> introInstanceDictionary = new Dictionary<ulong,GameObject>();
    private List<CharacterSelectButton> characterButtons = new List<CharacterSelectButton>();
    private NetworkList<CharacterSelectState> players;
    private NetworkList<int> characterIdList;

    bool isGameStart;
    private void Awake()
    {
        players = new NetworkList<CharacterSelectState>();
        characterIdList = new NetworkList<int>();

        characterInfoPanel.SetActive(false);
    }
    private void Update()
    {
        if (IsClient)
        {
            if(countDownTimer)
            {
                if(countDownTimer.GetRemainingTime() <= 0)
                {
                    lockInButton.interactable = false;
                    foreach (CharacterSelectButton item in characterButtons)
                    {
                        if (!item.IsDisabled)
                        {
                            item.SetDisabled();
                        }
                    }
                }
                countdownText.text = Math.Truncate(countDownTimer.GetRemainingTime()).ToString();
            }
        }
    }
    public override void OnNetworkSpawn()
    {
        countDownTimer = FindObjectOfType<CountDownTimer>();
        if (IsClient)
        {
            Character[] allCharacters = characterDatabase.GetAllCharacters();

            foreach (var character in allCharacters)
            {
                var selectbuttonInstance = Instantiate(selectButtonPrefab, charactersHolder);
                selectbuttonInstance.SetCharacter(this, character);
                characterButtons.Add(selectbuttonInstance);
            }

            players.OnListChanged += HandlePlayersStateChanged;
            
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
            countDownTimer.OnTimeExpired += CountDownTimer_OnTimeExpired;
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                HandleClientConnected(clientId);
            }
            foreach (var character in characterDatabase.GetAllCharacters())
            {
                characterIdList.Add(character.Id);
            }
        }
        
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            players.OnListChanged -= HandlePlayersStateChanged;
            for (int i = 0; i < players.Count; i++)
            {
                if (introInstanceDictionary.ContainsKey(players[i].ClientId) && introInstanceDictionary[players[i].ClientId])
                {
                    Destroy(introInstanceDictionary[players[i].ClientId]);
                }
            }
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
            countDownTimer.OnTimeExpired -= CountDownTimer_OnTimeExpired;
        }
        
    }
    private void HandleClientConnected(ulong clientId)
    {
        var clientName = NetworkServer.Instance.GetNetworkedPlayer(clientId).PlayerName.Value;
        players.Add(new CharacterSelectState(clientId,clientName));
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }

            players.RemoveAt(i);
            break;
        }
    }
    public void Select(Character character)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != NetworkManager.Singleton.LocalClientId) { continue; }

            if (players[i].IsLockedIn) { return; }

            if (players[i].CharacterId == character.Id) { return; }

            if (IsCharacterTaken(character.Id, false)) { return; }
        }

        //characterNameText.text = character.DisplayName;

        //characterInfoPanel.SetActive(true);

        //if (introInstance != null)
        //{
        //    Destroy(introInstance);
        //}

        //introInstance = Instantiate(character.IntroPrefab, introSpawnPoint);

        SelectServerRpc(character.Id);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SelectServerRpc(int characterId, ServerRpcParams serverRpcParams = default)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != serverRpcParams.Receive.SenderClientId) { continue; }

            if (!characterDatabase.IsValidCharacterId(characterId)) { return; }

            if (IsCharacterTaken(characterId, true)) { return; }

            players[i] = new CharacterSelectState(
                players[i].ClientId,
                players[i].ClientName,
                characterId,
                players[i].IsLockedIn
            );
        }
    }
    public void LockIn()
    {
        AudioManager.Instance.PlaySFXNumber(0);
        lockInButton.interactable = false;
        LockInServerRpc();
    }
    private void CountDownTimer_OnTimeExpired()
    {
        if (isGameStart) { return; }
        ServerForceLockIn();
    }
    void ServerForceLockIn()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (!players[i].IsLockedIn)
            {
                int idx = UnityEngine.Random.Range(0, characterIdList.Count);
                players[i] = new CharacterSelectState(
                players[i].ClientId,
                players[i].ClientName,
                characterIdList[idx],
                true);
                characterIdList.RemoveAt(idx);
            }
        }
        
        foreach (var player in players)
        {
            NetworkServer.Instance.SetCharacter(player.ClientId, player.CharacterId);
        }
        isGameStart = true;
        GamePlayBehaviour.Instance.LoadSceneDelay(GamePlayState.PlayGame);

    }
    [ServerRpc(RequireOwnership = false)]
    private void LockInServerRpc(ServerRpcParams serverRpcParams = default)
    {

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != serverRpcParams.Receive.SenderClientId) { continue; }

            if (!characterDatabase.IsValidCharacterId(players[i].CharacterId)) { return; }

            if (IsCharacterTaken(players[i].CharacterId, true)) { return; }

            players[i] = new CharacterSelectState(
                players[i].ClientId,
                players[i].ClientName,
                players[i].CharacterId,
                true
            );
        }

        foreach (var player in players)
        {
            if (!player.IsLockedIn) { return; }
        }

        foreach (var player in players)
        {
            NetworkServer.Instance.SetCharacter(player.ClientId, player.CharacterId);
        }
        isGameStart = true;
        GamePlayBehaviour.Instance.LoadSceneDelay(GamePlayState.PlayGame);
    }

    private void HandlePlayersStateChanged(NetworkListEvent<CharacterSelectState> changeEvent)
    {
        for (int i = 0; i < playerCards.Length; i++)
        {
            if (players.Count > i)
            { 
                playerCards[i].UpdateDisplay(players[i]);
            }
            else
            {
                playerCards[i].DisableDisplay();
            }
        }
        for (int i = 0; i < players.Count; i++)
        {
            introPointDictionary[players[i]] = introSpawnPointArray[i];
        }
        UpdateIntroInstance(changeEvent.Value);
        foreach (var button in characterButtons)
        {
            if (button.IsDisabled) { continue; }

            if (IsCharacterTaken(button.Character.Id, false))
            {
                button.SetDisabled();
            }
        }

        foreach (var player in players)
        {
            if (player.ClientId != NetworkManager.Singleton.LocalClientId) { continue; }

            if (player.IsLockedIn)
            {
                lockInButton.interactable = false;
                break;
            }

            if (IsCharacterTaken(player.CharacterId, false))
            {
                lockInButton.interactable = false;
                break;
            }

            lockInButton.interactable = true;

            break;
        }
    }

    private bool IsCharacterTaken(int characterId, bool checkAll)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (!checkAll)
            {
                if (players[i].ClientId == NetworkManager.Singleton.LocalClientId) { continue; }
            }

            if (players[i].IsLockedIn && players[i].CharacterId == characterId)
            {
                if(IsServer)
                {
                    characterIdList.Remove(players[i].CharacterId);
                }
                return true;
            }
        }

        return false;
    }
    void UpdateIntroInstance(CharacterSelectState player)
    {
        if(player.CharacterId == -1) { return; }
        
        if (introInstanceDictionary.ContainsKey(player.ClientId) && introInstanceDictionary[player.ClientId]!=null)
        {
            Destroy(introInstanceDictionary[player.ClientId]);
            introInstanceDictionary.Remove(player.ClientId);
        }
        var character = characterDatabase.GetCharacterById(player.CharacterId);
        var screenRect = introPointDictionary[player];
        var instance = Instantiate(character.IntroPrefab,screenRect);
        introInstanceDictionary.Add(player.ClientId, instance);
        if(player.IsLockedIn)
        {
            foreach (var other in players)
            {
                if (other.ClientId != player.ClientId && other.CharacterId == player.CharacterId)
                {
                    if (introInstanceDictionary.ContainsKey(other.ClientId) && introInstanceDictionary[other.ClientId])
                    {
                        Destroy(introInstanceDictionary[other.ClientId]);
                        introInstanceDictionary.Remove(other.ClientId);
                    }
                }
            }
        }
    }
    Vector3 GetWorldPositionFromUI(RectTransform uiElement, Camera uiCamera, Camera worldCamera)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, uiElement.position);
        Debug.Log(screenPoint);
        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(uiElement, screenPoint, worldCamera, out Vector3 worldPosition))
        {
            Debug.Log("do nothing");
        }
        Vector3 pos = new Vector3(worldPosition.x, worldPosition.y, 0);
        return pos;
    }
}
