using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioMixer mixer;

    public AudioClip[] bgm_Clips;

    [SerializeField] AudioClip Btn_click01;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(Scene prevScene, Scene newScene)
    {
        if(newScene == SceneManager.GetSceneByName("Login"))
        {
            PlayMusic(bgm_Clips[0]);
        }
        else if(newScene == SceneManager.GetSceneByName("MainMenu"))
        {
            PlayMusic(bgm_Clips[1]);
        }
        else if (newScene == SceneManager.GetSceneByName("PostScene"))
        {
            PlayMusic(bgm_Clips[5]);
        }
    }
    public void PlayBGMNumber(int number)
    {
        PlayMusic(bgm_Clips[number]);
    }
    public void PlaySFXNumber(int number)
    {
        switch (number)
        {
            case 0:
                PlaySFX(Btn_click01);
                break; 
            case 1:
                break;
            default: 
                break;
        }
    }
    public void PlayMusic(AudioClip clip)
    {
        bgmSource.clip = clip;
        bgmSource.Play();
    }
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.clip = clip;
        sfxSource.Play();
    }

    public void SetBGMVolume(float volume)
    {
        mixer.SetFloat("BGM", Mathf.Log10(volume) * 10);
    }
    public void SetSFXVolume(float volume)
    {
        mixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
    }

    internal void ToggleBGM(bool value)
    {
        if(value)
        {
            mixer.SetFloat("BGM", -5);
        }
        else
        {
            mixer.SetFloat("BGM", -80);
        }    
    }

    internal void ToggleSFX(bool value)
    {
        if (value)
        {
            mixer.SetFloat("SFX", 0);
        }
        else
        {
            mixer.SetFloat("SFX", -80);
        }
    }
}
