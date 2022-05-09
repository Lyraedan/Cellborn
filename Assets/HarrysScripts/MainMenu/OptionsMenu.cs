using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [Header("Groups")]
    public GameObject graphicsGroup, audioGroup, controlsGroup, otherGroup;

    [Header("Graphics")]
    public List<Resolution> resolutions;
    public List<string> resolutionStr;
    public TMP_Dropdown resolutionDropdown;

    public Toggle fullscreenToggle;

    public List<string> qualitySettings;
    public TMP_Dropdown qualityDropdown;

    [Header("Audio")]
    public AudioMixer audioMixer;
    public TextMeshProUGUI masterVolText;
    public TextMeshProUGUI musicVolText;
    public TextMeshProUGUI soundVolText;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider soundSlider;
    
    void Start()
    {
        OpenGraphics();
        
        resolutions = Screen.resolutions.ToList();

        resolutionDropdown.ClearOptions();
                
        int currentResolutionIndex = 0;        
        for(int i = 0; i < resolutions.Count; i++)
        {
            string resString = resolutions[i].width + " x " + resolutions[i].height + " " + resolutions[i].refreshRate + "Hz";
            resolutionStr.Add(resString);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        
        resolutionDropdown.AddOptions(resolutionStr);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        fullscreenToggle.isOn = Screen.fullScreen;

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(qualitySettings);
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();

        /* masterSlider.value = Mathf.Pow(10, GetMasterVolume()) / 20;
        musicSlider.value = Mathf.Pow(10, GetMusicVolume()) / 20;
        soundSlider.value = Mathf.Pow(10, GetSFXVolume()) / 20;
        masterVolText.text = (int)(Mathf.Pow(10, GetMasterVolume()) / 20) + "%";
        musicVolText.text = (int)(Mathf.Pow(10, GetMusicVolume()) / 20) + "%";
        soundVolText.text = (int)(Mathf.Pow(10, GetSFXVolume()) / 20) + "%"; */
    }

    public void SetResolution (int resIndex)
    {
        Resolution resolution = resolutions[resIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetFullscreen (bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetQuality (int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        Debug.Log(QualitySettings.GetQualityLevel());
    }

    public void SetVSync (bool isVSync)
    {
        QualitySettings.vSyncCount = Convert.ToInt32(isVSync);
    }

    public void SetMasterVolume (float volume)
    {
        audioMixer.SetFloat("MasterVol", Mathf.Log10(volume) * 20);
        masterVolText.text = (int)(volume * 100) + "%";
    }
    
    public void SetMusicVolume (float volume)
    {
        audioMixer.SetFloat("MusicVol", Mathf.Log10(volume) * 20);
        musicVolText.text = (int)(volume * 100) + "%";
    }

    public void SetSoundVolume (float volume)
    {
        audioMixer.SetFloat("SFXVol", Mathf.Log10(volume) * 20);
        soundVolText.text = (int)(volume * 100) + "%";
    }

    public float GetMasterVolume()
    {
        float value;
        if(audioMixer.GetFloat("MasterVol", out value))
        {
            return value;
        }
        else
        {
            return 0f;
        }
    }

    public float GetMusicVolume()
    {
        float value;
        if(audioMixer.GetFloat("MusicVol", out value))
        {
            return value;
        }
        else
        {
            return 0f;
        }
    }

    public float GetSFXVolume()
    {
        float value;
        if(audioMixer.GetFloat("SFXVol", out value))
        {
            return value;
        }
        else
        {
            return 0f;
        }
    }

    #region Section Swapping

    public void OpenGraphics()
    {
        AudioManager.instance.Play("MenuClick");
        graphicsGroup.SetActive(true);
        audioGroup.SetActive(false);
        controlsGroup.SetActive(false);
        otherGroup.SetActive(false);
    }

    public void OpenAudio()
    {
        AudioManager.instance.Play("MenuClick");
        graphicsGroup.SetActive(false);
        audioGroup.SetActive(true);
        controlsGroup.SetActive(false);
        otherGroup.SetActive(false);
    }

    public void OpenControls()
    {
        AudioManager.instance.Play("MenuClick");
        graphicsGroup.SetActive(false);
        audioGroup.SetActive(false);
        controlsGroup.SetActive(true);
        otherGroup.SetActive(false);
    }

    public void OpenOther()
    {
        AudioManager.instance.Play("MenuClick");
        graphicsGroup.SetActive(false);
        audioGroup.SetActive(false);
        controlsGroup.SetActive(false);
        otherGroup.SetActive(true);
    }

    public void NoFunction()
    {
        AudioManager.instance.Play("MenuError");
    }

    #endregion
}
