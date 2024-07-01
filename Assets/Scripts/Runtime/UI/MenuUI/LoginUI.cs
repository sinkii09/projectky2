using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [SerializeField] LoginAndRegisterLogic loginLogic;

    [SerializeField] TMP_InputField userNameInputField;
    [SerializeField] TMP_InputField passwordInputField;
    [SerializeField] Button loginButton;
    [SerializeField] Button registerButton;

    public Selectable[] UISelectables;
    private void Start()
    {
        loginButton.GetComponent<Button>().onClick.AddListener(Login);
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
            loginLogic.Login(userNameInputField.text, passwordInputField.text);
        }
    }
    public void Register()
    {
        loginLogic.ToRegister();
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
