using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RegisterUI : MonoBehaviour
{
    [SerializeField] LoginAndRegisterLogic loginLogic;

    [SerializeField] TMP_InputField userNameInputField;
    [SerializeField] TMP_InputField passwordInputField;
    [SerializeField] TMP_InputField rePasswordInputField;
    [SerializeField] TMP_InputField nicknameInputField;

    [SerializeField] Button signInButton;
    [SerializeField] Button backButton;

    private List<TMP_InputField> inputFields;
    private string allowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private void Start()
    {
        signInButton.GetComponent<Button>().onClick.AddListener(SignIn);
        backButton.GetComponent<Button>().onClick.AddListener(BackToLogin);

        InitializeInputFields();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (inputFields.Count > 0)
            {
                OnSwitchInputField();
            }
        }
    }
    private void InitializeInputFields()
    {
        userNameInputField.onValueChanged.AddListener(ValidateInput);
        passwordInputField.onValueChanged.AddListener(ValidateInput);
        rePasswordInputField.onValueChanged.AddListener(ValidateInput);
        nicknameInputField.onValueChanged.AddListener(ValidateInput);

        inputFields = new List<TMP_InputField>
        {
            userNameInputField,
            passwordInputField,
            rePasswordInputField,
            nicknameInputField
        };
    }
    private void ValidateInput(string input)
    {
        TMP_InputField inputField = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
        string filteredInput = "";
        foreach (char c in input)
        {
            if (allowedCharacters.Contains(c.ToString()))
            {
                filteredInput += c;
            }
        }
        inputField.text = filteredInput;
    }
    void BackToLogin()
    {
        loginLogic.ToLogin();
        AudioManager.Instance.PlaySFXNumber(0);
    }
    void SignIn()
    {
        foreach (var field in inputFields)
        {
            if (field.text == string.Empty)
            {
                Debug.LogError($"{field.name} cannot empty, please fill in");
                return;
            }
        }
        if(!string.Equals(passwordInputField.text,rePasswordInputField.text))
        {
            Debug.LogError($"confirm password is incorrect");
            return;
        }
        
        CreateUserDto createUserDto = new CreateUserDto();
        createUserDto.username = userNameInputField.text;
        createUserDto.password = passwordInputField.text;
        createUserDto.name = nicknameInputField.text;

        loginLogic.Register(createUserDto);
        AudioManager.Instance.PlaySFXNumber(0);
    }

    private void OnSwitchInputField()
    {
        for (int i = 0; i < inputFields.Count; i++)
        {
            if (inputFields[i].gameObject == EventSystem.current.currentSelectedGameObject)
            {
                inputFields[(i + 1) % inputFields.Count].Select();
                break;
            }
        }
    }
}
