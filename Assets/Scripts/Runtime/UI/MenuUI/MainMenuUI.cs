using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    ClientGameManager gameManager;
    bool m_LocalLaunchMode;

    [SerializeField] TMP_Dropdown m_MapDropDown;
    [SerializeField] TMP_Dropdown m_ModeDropDown;
    [SerializeField] TMP_Dropdown m_QueueDropDown;
    [SerializeField] Button findmatchButton;

    [SerializeField] TextMeshProUGUI timer_TMP;
    [SerializeField] TextMeshProUGUI profileName_TMP;
    [SerializeField]
    GameObject[] UIList;

    [SerializeField]
    ProfileUI profileUI;
    UserManager userManager;
    public UserManager UserManager => userManager;
    private void Awake()
    {
        userManager = new UserManager();
    }
    void Start()
    {
        findmatchButton.onClick.AddListener(PlayButtonPressed);
        m_MapDropDown.AddOptions(new List<string>(typeof(Map).GetEnumNames()));
        m_MapDropDown.onValueChanged.AddListener(delegate { MapDropDownChanged(m_MapDropDown); });
        m_QueueDropDown.AddOptions(new List<string>(typeof(GameQueue).GetEnumNames()));
        m_QueueDropDown.onValueChanged.AddListener(delegate { QueueDropDownChanged(m_QueueDropDown); });


        if(AuthenticationWrapper.AuthorizationState != AuthState.Authenticated)
        {
            SwitchUI(0);
        }
        else
        {
            SwitchUI(1);
        }

    }
    private void OnDestroy()
    {
        findmatchButton.onClick.RemoveListener(PlayButtonPressed);
    }
    #region Login
    public void SetUpInitialState()
    {
        gameManager = ClientSingleton.Instance.Manager;

        gameManager.SetGameMode(Enum.Parse<GameMode>(m_ModeDropDown.value.ToString()));
        gameManager.SetGameMap(Enum.Parse<Map>(m_MapDropDown.value.ToString()));
        gameManager.SetGameQueue(Enum.Parse<GameQueue>(m_QueueDropDown.value.ToString()));
    }
    public void Login(string username, string password)
    {
        LoadingScreen();
        userManager.Login(username, password, CreateUserWithProfile, LoginError);
    }
    public void CreateUserWithProfile(LoginResponse response)
    {
        ClientSingleton.Instance.CreateClient(ToMenuAfterFinishCreateClient, response);

    }
    void ToMenuAfterFinishCreateClient()
    {
        userManager.AccessToken = ClientSingleton.Instance.Manager.User.AcessToken;
        SetUpInitialState();
        profileUI.SetUserProfile(ClientSingleton.Instance.Manager.User);
        ToMainMenu();
    }
    void LoginError()
    {
        SwitchUI(0);
    }
    #endregion

    #region Register
    public void Register(CreateUserDto createUserDto)
    {
        LoadingScreen();
        userManager.Register(createUserDto, SuccessRegister, FailedRegister);
    }
    void SuccessRegister(LoginResponse response)
    {
        CreateUserWithProfile(response);
    }
    void FailedRegister(string error)
    {
        Debug.LogError(error);
        SwitchUI(3);
    }
    #endregion

    #region Profile
    public void FindPlayer(string playerInput,Action<GetPlayerResponse> success,Action<string> failed)
    {
        userManager.FindUserByNameOrId(playerInput,success,failed);
    }
    public void FetchFriendList(Action<List<FriendData>> success, Action<string> failed)
    {
        userManager.FetchFriendList(success,failed);
    }
    public void FetchFriendRequestList(Action<List<FriendData>> success, Action<string> failed)
    {
        userManager.FetchFriendRequestList(success,failed);
    }
    #endregion
    #region MatchMake
    void UpdateTimer(int elapsedSeconds)
    {
        TimeSpan elapsedTime = TimeSpan.FromSeconds(elapsedSeconds);
        timer_TMP.text = (string.Format("{0:D2}:{1:D2}", elapsedTime.Minutes, elapsedTime.Seconds));
    }
    public async void CancelMatchFinding()
    {

await        gameManager.CancelMatchmaking();
        SwitchUI(1);

    }
    void OnMatchMade(MatchmakerPollingResult result)
    {
        switch(result)
        {
            case MatchmakerPollingResult.Success:
                Debug.Log("Match making success");
                break;
            default:
                SwitchUI(1);
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }
    }
    #endregion

    #region MainMenu
    void PlayButtonPressed()
    {
        SwitchUI(2);
#pragma warning disable 4014
        gameManager.MatchmakeAsync(UpdateTimer, OnMatchMade);
#pragma warning restore 4014
    }
    void MapDropDownChanged(TMP_Dropdown dropdown)
    {
        if (!Enum.TryParse(dropdown.value.ToString(), out Map selectedMap))
            return;
        gameManager.SetGameMap(selectedMap);
    }
    void QueueDropDownChanged(TMP_Dropdown dropdown)
    {
        if (!Enum.TryParse(dropdown.value.ToString(), out GameQueue selectedQueue
            ))
            return;
        gameManager.SetGameQueue(selectedQueue);
    }

    #endregion
    public void ExitApplication()
    {
        if (gameManager != null)
        {
            gameManager.ExitGame();
        }
        else
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#endif
            Application.Quit();
        }
    }
    void SwitchUI(int idx)
    {
        for(int i = 0; i < UIList.Length; i++) 
        {
            UIList[i].SetActive(false);
        }
        UIList[idx].SetActive(true);
    }
    public void LoadingScreen()
    {
        SwitchUI(4);
    }
    public void ToMainMenu()
    {
        profileName_TMP.text = "Hello " + ClientSingleton.Instance.Manager.User.Name;
        SwitchUI(1);
    }
    public void ToRegiter()
    {
        SwitchUI(3);
    }
    public void ToProfile()
    {
        SwitchUI(5);
    }
    public void ToLoginPage()
    {
        if(gameManager!=null)
        {
            gameManager.Dispose();
            AuthenticationWrapper.SignOut();    
        }
        SwitchUI(0);
    }
}
