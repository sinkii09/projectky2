using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

public class LoginAndRegisterLogic : MonoBehaviour
{
    public enum LoginState
    {
        login,
        register,
        loading
    }
    private LoginState state;

    [SerializeField] LoginUI loginUI;
    [SerializeField] RegisterUI registerUI;
    [SerializeField] Transform loadingScreen;

    List<GameObject> UIList;
    private void Awake()
    {

        UIList = new List<GameObject>
        {
            loginUI.gameObject,
            registerUI.gameObject,
            loadingScreen.gameObject
        };
        SwitchState(LoginState.login);
    }
    #region Login

    public void Login(string username, string password)
    {
        ToLoading();
        UserManager.Instance.Login(username, password, CreateUserWithProfile, LoginError);
    }
    public void CreateUserWithProfile(LoginResponse response)
    {
        ClientSingleton.Instance.CreateClient(ToMenuAfterFinishCreateClient, response);
    }
    void ToMenuAfterFinishCreateClient()
    {
        ToMainMenu();
    }
    private void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    void LoginError()
    {
        ToLogin();
    }
    #endregion

    #region Register
    public void Register(CreateUserDto createUserDto)
    {
        ToLoading();
        UserManager.Instance.Register(createUserDto, SuccessRegister, FailedRegister);
    }
    void SuccessRegister(LoginResponse response)
    {
        CreateUserWithProfile(response);
    }
    void FailedRegister(string error)
    {
        ToRegister();
    }
    #endregion

    void SwitchState(LoginState state)
    {
        this.state = state;
        foreach (GameObject go in UIList)
        {
            go.SetActive(false);
        }
        switch(state)
        {
            case LoginState.login:
                loginUI.gameObject.SetActive(true);
                break;
            case LoginState.register:
                registerUI.gameObject.SetActive(true);
                break;
            case LoginState.loading:
                loadingScreen.gameObject.SetActive(true);
                break;
        }
    }
    public void ToLogin()
    {
        SwitchState(LoginState.login);
    }
    public void ToRegister()
    {
        SwitchState(LoginState.register);
    }
    public void ToLoading()
    {
        SwitchState(LoginState.loading);
    }

    internal void ExitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#endif
        Application.Quit();

    }
}
