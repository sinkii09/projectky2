using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioMixer mixer;
    public AudioMixerGroup mixerGroup;

    public AudioClip[] bgm_Clips;

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        public bool loop = false;
    }
    public Sound[] sounds;
    public int poolSize = 10;
    public GameObject audioSourcePrefab;
    private Queue<AudioSource> audioPool;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;

        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        InitializeAudioPool();

        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void InitializeAudioPool()
    {
        audioPool = new Queue<AudioSource>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(audioSourcePrefab, transform);
            AudioSource audioSource = obj.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.playOnAwake = false;
                audioSource.gameObject.SetActive(false);
                audioPool.Enqueue(audioSource);
            }

        }
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
    public void PlayMusic(AudioClip clip)
    {
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void PlaySFX(string name)
    {
        Sound sound = System.Array.Find(sounds, s => s.name == name);
        if (sound != null)
        {
            AudioSource audioSource = GetPooledAudioSource();
            audioSource.clip = sound.clip;
            audioSource.loop = sound.loop;
            audioSource.outputAudioMixerGroup = mixerGroup;
            audioSource.Play();
            if (!sound.loop)
            {
                StartCoroutine(ReturnAudioSourceToPool(audioSource, sound.clip.length));
            }
        }
        else
        {
            Debug.LogWarning("Sound not found: " + name);
        }
    }
    public void PlaySFXAtPosition(string name, Vector3 position)
    {
        Sound sound = Array.Find(sounds, s => s.name == name);
        if (sound != null)
        {
            AudioSource audioSource = GetPooledAudioSource();
            audioSource.clip = sound.clip;
            audioSource.loop = sound.loop;
            audioSource.outputAudioMixerGroup = mixerGroup;
            audioSource.spatialBlend = 1.0f;
            audioSource.gameObject.transform.position = position;
            audioSource.Play();
            if (!sound.loop)
            {
                StartCoroutine(ReturnAudioSourceToPool(audioSource, sound.clip.length));
            }
        }
        else
        {
            Debug.LogWarning("Sound not found: " + name);
        }
    }
    private AudioSource GetPooledAudioSource()
    {
        if (audioPool.Count > 0)
        {
            AudioSource audioSource = audioPool.Dequeue();
            audioSource.gameObject.SetActive(true);
            return audioSource;
        }
        else
        {
            GameObject obj = Instantiate(audioSourcePrefab, transform);
            AudioSource audioSource = obj.GetComponent<AudioSource>();
            return audioSource;
        }
    }

    private IEnumerator ReturnAudioSourceToPool(AudioSource audioSource, float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.gameObject.SetActive(false);
        audioPool.Enqueue(audioSource);
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
