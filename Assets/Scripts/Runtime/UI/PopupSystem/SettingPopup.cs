using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingPopup : MonoBehaviour
{
    [SerializeField] Button showButton;
    [SerializeField] Button hideButton;

    [SerializeField] GameObject PopupWindow;
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider sfxSlider;
    [SerializeField] Toggle bgmToggle;
    [SerializeField] Toggle sfxToggle;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        HidePopup();
    }
    private void Start()
    {
        showButton.onClick.AddListener(() => { ShowPopup(); AudioManager.Instance.PlaySFXNumber(0); });
        hideButton.onClick.AddListener(() => { HidePopup(); AudioManager.Instance.PlaySFXNumber(0); });

        bgmSlider.onValueChanged.AddListener(OnBGMValueChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXValueChanged);

        bgmToggle.onValueChanged.AddListener(ToggleBGM);
        sfxToggle.onValueChanged.AddListener(ToggleSFX);
    }

    void ShowPopup()
    {
        showButton.gameObject.SetActive(false);
        PopupWindow.SetActive(true);
    }
    void HidePopup()
    {
        showButton.gameObject.SetActive(true);
        PopupWindow.SetActive(false);
    }
    void OnBGMValueChanged(float value)
    {
        AudioManager.Instance.SetBGMVolume(value);
    }
    void OnSFXValueChanged(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
    }
    void ToggleBGM(bool value)
    {
        AudioManager.Instance.ToggleBGM(value);
        if(value)
        {
            OnBGMValueChanged(bgmSlider.value);
        }
    }
    void ToggleSFX(bool value)
    {
        AudioManager.Instance.ToggleSFX(value);
        //if (value)
        //{
        //    OnBGMValueChanged(sfxSlider.value);
        //}
    }
}
