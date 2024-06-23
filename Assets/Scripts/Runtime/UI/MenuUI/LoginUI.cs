using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [SerializeField] MainMenuUI mainMenuUI;

    [SerializeField] TMP_InputField userNameInputField;
    [SerializeField] TMP_InputField passwordInputField;
    [SerializeField] Button loginButton;
    [SerializeField] Button exitButton;
    [SerializeField] Button registerButton;

    public Selectable[] UISelectables;
    private void Start()
    {
        loginButton.GetComponent<Button>().onClick.AddListener(Login);
        exitButton.GetComponent<Button>().onClick.AddListener(Exit);
        registerButton.GetComponent<Button>().onClick.AddListener(Register);   
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            OnSwitchInputField();
        }
    }
    public void Login()
    {
        if(userNameInputField.text != string.Empty &&  passwordInputField.text != string.Empty)
        {
            mainMenuUI.Login(userNameInputField.text, passwordInputField.text);
        }
    }
    public void Register()
    {
        mainMenuUI.ToRegiter();
    }
    void Exit()
    {
        mainMenuUI.ExitApplication();
    }
    private void OnSwitchInputField()
    {
        for (int i = 0; i < UISelectables.Length; i++)
        {
            if (UISelectables[i].gameObject == EventSystem.current.currentSelectedGameObject)
            {
                UISelectables[(i + 1) % UISelectables.Length].Select();
                break;
            }
        }
    }
}
