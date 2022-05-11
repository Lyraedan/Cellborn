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

    public AudioSource source;
    public AudioClip click, error;

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

        // This is a very complicated way of doing "(value * 100) / MaxValue" - Luke

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

    #region Section Swapping

    public void OpenGraphics()
    {
        source.clip = click;
        source.Play();
        graphicsGroup.SetActive(true);
        audioGroup.SetActive(false);
        controlsGroup.SetActive(false);
        otherGroup.SetActive(false);
    }

    public void OpenAudio()
    {
        source.clip = click;
        source.Play();
        graphicsGroup.SetActive(false);
        audioGroup.SetActive(true);
        controlsGroup.SetActive(false);
        otherGroup.SetActive(false);
    }

    public void OpenControls()
    {
        source.clip = click;
        source.Play();
        graphicsGroup.SetActive(false);
        audioGroup.SetActive(false);
        controlsGroup.SetActive(true);
        otherGroup.SetActive(false);
    }

    public void OpenOther()
    {
        source.clip = click;
        source.Play();
        graphicsGroup.SetActive(false);
        audioGroup.SetActive(false);
        controlsGroup.SetActive(false);
        otherGroup.SetActive(true);
    }

    // Why the fuck does this exist...
    public void NoFunction()
    {
        source.clip = error;
        source.Play();
    }

    #endregion
}
