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
    public TMP_Dropdown qualityDropdown;

    [Header("Audio")]
    public AudioMixer audioMixer;
    public TextMeshProUGUI masterVolText;
    public TextMeshProUGUI musicVolText;
    public TextMeshProUGUI soundVolText;
    
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

        qualityDropdown.value = QualitySettings.GetQualityLevel();
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
        audioMixer.SetFloat("MasterVol", volume);
        masterVolText.text = (int)(100f - ((volume / -80) * 100f)) + "%";
    }
    
    public void SetMusicVolume (float volume)
    {
        audioMixer.SetFloat("MusicVol", volume);
        musicVolText.text = (int)(100f - ((volume / -80) * 100f)) + "%";
    }

    public void SetSoundVolume (float volume)
    {
        audioMixer.SetFloat("SFXVol", volume);
        soundVolText.text = (int)(100f - ((volume / -80) * 100f)) + "%";
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
