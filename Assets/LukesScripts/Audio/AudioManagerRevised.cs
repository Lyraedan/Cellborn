using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManagerRevised : MonoBehaviour
{
    public Slider masterSlider, musicSlider, sfxSlider;
    public TextMeshProUGUI masterVolText, musicVolText, sfxVolText;
    private float master, music, sfx;

    public static AudioManagerRevised instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(this);

    }

    private void Start()
    {
        LoadSliders();
        SceneManager.sceneLoaded += LoadedScene;
    }

    void LoadSliders()
    {
        masterSlider.value = PlayerPrefs.HasKey("VOLUME_MASTER") ? PlayerPrefs.GetFloat("VOLUME_MASTER") : masterSlider.maxValue;
        musicSlider.value = PlayerPrefs.HasKey("VOLUME_MUSIC") ? PlayerPrefs.GetFloat("VOLUME_MUSIC") : musicSlider.maxValue;
        sfxSlider.value = PlayerPrefs.HasKey("VOLUME_SFX") ? PlayerPrefs.GetFloat("VOLUME_SFX") : sfxSlider.maxValue;

        master = masterSlider.value;
        music = musicSlider.value;
        sfx = sfxSlider.value;

        masterVolText.text = $"{Mathf.RoundToInt((master * 100) / masterSlider.maxValue)}%";
        musicVolText.text = $"{Mathf.RoundToInt((music * 100) / masterSlider.maxValue)}%";
        sfxVolText.text = $"{Mathf.RoundToInt((sfx * 100) / masterSlider.maxValue)}%";

        masterSlider.onValueChanged.AddListener(val =>
        {
            PlayerPrefs.SetFloat("VOLUME_MASTER", masterSlider.value);
            master = masterSlider.value;
            masterVolText.text = $"{Mathf.RoundToInt((master * 100) / masterSlider.maxValue)}%";
        });

        musicSlider.onValueChanged.AddListener(val =>
        {
            PlayerPrefs.SetFloat("VOLUME_MUSIC", musicSlider.value);
            music = musicSlider.value;
            musicVolText.text = $"{Mathf.RoundToInt((music * 100) / masterSlider.maxValue)}%";
        });

        sfxSlider.onValueChanged.AddListener(val =>
        {
            PlayerPrefs.SetFloat("VOLUME_SFX", sfxSlider.value);
            sfx = sfxSlider.value;
            sfxVolText.text = $"{Mathf.RoundToInt((sfx * 100) / masterSlider.maxValue)}%";
        });
    }

    private void LoadedScene(Scene scene, LoadSceneMode mode)
    {
        // is first scene aka menu
        if(scene.name == "MainMenu")
        {
            var cache = FindObjectOfType<SoundSliderCache>();
            masterSlider = cache.master;
            sfxSlider = cache.sfx;
            musicSlider = cache.music;

            masterVolText = cache.masterText;
            sfxVolText = cache.sfxText;
            musicVolText = cache.musicText;

            masterSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.RemoveAllListeners();

            LoadSliders();
        }
    }

    public float GetSfxVolume()
    {
        return sfx;
    }

    public float GetMusicVolume()
    {
        return music;
    }

    public float GetMasterVolume()
    {
        return master;
    }

}
