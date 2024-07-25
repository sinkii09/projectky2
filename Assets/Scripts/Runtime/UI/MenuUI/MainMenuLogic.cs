using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuLogic : MonoBehaviour
{
    public event Action<int> OnTimeLapse;

    ClientGameManager gameManager;
    public enum MenuState
    {
        MainMenu,
        Loading
    }
    MenuState state;

    [SerializeField] TextMeshProUGUI profileName_TMP;

    [SerializeField] GameObject MainMenu ,Loading;

    [SerializeField] Transform ModelHolder;
    [SerializeField] CharacterDatabase characterDatabase;
    
    List<GameObject> UIList;

    [SerializeField] ProfileUI profileUI;
    [SerializeField] ShopUI shopUI;
    [SerializeField] Button showProfileBtn;
    [SerializeField] Button shopButton;
    [SerializeField] Button exitBtn;
    [SerializeField] Button changeHatButton;
    public GameQueue GameQueue {  get; set; }
    public PlayMode PlayMode { get; set; }

    private GameObject model;

    bool isfirstTime = true;
    private void Awake()
    {
        UIList = new List<GameObject>
        {
            MainMenu,
            Loading
        };
        gameManager = ClientSingleton.Instance.Manager;
        profileUI.SetUserProfile(gameManager.User); 
    }
    void Start()
    {
        showProfileBtn.onClick.AddListener(()=> { ShowProfile(); AudioManager.Instance.PlaySFX("Btn_click01"); });
        exitBtn.onClick.AddListener(() => { ExitApplication(); AudioManager.Instance.PlaySFX("Btn_click01"); });
        shopButton.onClick.AddListener(()=> { ShowShop(); AudioManager.Instance.PlaySFX("Btn_click01"); });
        ToMainMenu();
        InventoryManager.Instance.FetchInventory();

        InventoryManager.OnFetchInventorySuccess += InventoryManager_OnFetchInventorySuccess;
        
    }

    private void OnDestroy()
    {
        showProfileBtn?.onClick.RemoveAllListeners();
        exitBtn.onClick.RemoveAllListeners();
        shopButton.onClick.RemoveAllListeners();
        InventoryManager.OnFetchInventorySuccess -= InventoryManager_OnFetchInventorySuccess;
    }

    private void ShowShop()
    {
        ShopManager.Instance.FetchShopItem();
        shopUI.gameObject.SetActive(true);
    }

    #region Inventory

    private void InventoryManager_OnFetchInventorySuccess()
    {
        if(isfirstTime)
        {
            isfirstTime = false;
            changeHatButton.gameObject.SetActive(true);
            changeHatButton.onClick.AddListener(() => { InventoryManager.Instance.ChangeEquipItem("hat"); AudioManager.Instance.PlaySFX("Btn_click01"); });
            SpawnRandomModel();
        }
    }

    #endregion
    #region Profile
    private void ShowProfile()
    {
        profileUI.gameObject.SetActive(true);
        showProfileBtn.gameObject.SetActive(false);
        AudioManager.Instance.PlaySFX("Btn_click01");
    }
    public void HideProfile()
    {
        profileUI?.gameObject.SetActive(false);
        showProfileBtn?.gameObject.SetActive(true);
    }
    public void FindPlayer(string playerInput,Action<GetPlayerResponse> success,Action<string> failed)
    {
        UserManager.Instance.FindUserByNameOrId(playerInput,success,failed);
    }
    public void FetchFriendList(Action<List<FriendData>> success, Action<string> failed)
    {
        UserManager.Instance.FetchFriendList(success,failed);
    }
    public void FetchFriendRequestList(Action<List<FriendData>> success, Action<string> failed)
    {
        UserManager.Instance.FetchFriendRequestList(success,failed);
    }
    public void FetchLeaderBoard(Action<List<LeadUser>> success, Action<string> failed)
    {
        UserManager.Instance.FetchLeaderboard(success,failed);
    }
    public void FetchUserRank(Action<UserRank> success, Action failed)
    {
        UserManager.Instance.FetchUserRank(success, failed);
    }
    #endregion

    #region matchmake
    public void PlayButtonPressed(Map map)
    {
        gameManager.SetGameMap(map);
        gameManager.SetGameQueue(GameQueue);
        gameManager.SetGameMode(PlayMode);
        
#pragma warning disable 4014
        gameManager.MatchmakeAsync(UpdateTimer, OnMatchMade);
#pragma warning restore 4014
    }
    void UpdateTimer(int elapsedSeconds)
    {
        OnTimeLapse?.Invoke(elapsedSeconds);
    }
    void OnMatchMade(MatchmakerPollingResult result)
    {
        switch (result)
        {
            case MatchmakerPollingResult.Success:
                Debug.Log("Match making success");
                break;
            case MatchmakerPollingResult.TicketCancellationError:
                Debug.Log("ticket cacnel error");
                break;
            default:
                PopupManager.Instance.ShowPopup(result.ToString());
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }
    }

    public async void CancelMatchFinding()
    {
        await gameManager.CancelMatchmaking();
    }
    #endregion


    #region UI switch
    public void ExitApplication()
    {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#endif
            Application.Quit();
        
    }
    void SwitchUI(MenuState state)
    {
        this.state = state;
        foreach (GameObject go in UIList)
        {
            go.SetActive(false);
        }
        switch (state)
        {
            case MenuState.MainMenu:
                MainMenu.SetActive(true);
                break;
            case MenuState.Loading: 
                Loading.SetActive(true);
                break;    
        }
    }
    public void ToLoading()
    {
        SwitchUI(MenuState.Loading);
    }
    public void ToMainMenu()
    {
        profileName_TMP.text = ClientSingleton.Instance.Manager.User.Name;
        SwitchUI(MenuState.MainMenu);
        HideProfile();
        ShopManager.Instance.UpdateBalance(ClientSingleton.Instance.Manager.User.Data.playerGold);
        shopUI.gameObject.SetActive(false);
    }
    internal void Logout()
    {
        ClientSingleton.Instance.Logout();
    }
    #endregion

    void SpawnRandomModel()
    {
        if (model != null)
        {
            return;
        }
        var characters = characterDatabase.GetAllCharacters();
        int idx = UnityEngine.Random.Range(0, characters.Length);
        model = Instantiate(characters[idx].IntroPrefab, ModelHolder);
        model.GetComponent<HatSlot>().Initialize();
    }
}
