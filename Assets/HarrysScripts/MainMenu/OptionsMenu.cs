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
    public Toggle anisotropicToggle;

    public TMP_Dropdown antiAliasingDropdown;
    public TMP_Dropdown shadowsDropdown;
    public TMP_Dropdown shadowResDropdown;
    public TMP_Dropdown textureResDropdown;
    public List<string> qualitySettings;
    public TMP_Dropdown qualityDropdown;

    public AudioSource source;
    public AudioClip click, error;

    void Start()
    {
        OpenGraphics();

        #region Set Resolution Dropdown

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

        #endregion

        #region Set Other Dropdowns

        switch(QualitySettings.antiAliasing)
        {
            case 0: antiAliasingDropdown.value = 0; break;
            case 2: antiAliasingDropdown.value = 1; break;
            case 4: antiAliasingDropdown.value = 2; break;
            case 8: antiAliasingDropdown.value = 3; break;
        } 
        antiAliasingDropdown.RefreshShownValue();

        switch(QualitySettings.shadows)
        {
            case ShadowQuality.Disable: shadowsDropdown.value = 0; break;
            case ShadowQuality.HardOnly: shadowsDropdown.value = 1; break;
            case ShadowQuality.All: shadowsDropdown.value = 2; break;
        }
        shadowsDropdown.RefreshShownValue();

        switch(QualitySettings.shadowResolution)
        {
            case ShadowResolution.Low: shadowResDropdown.value = 0; break;
            case ShadowResolution.Medium: shadowResDropdown.value = 1; break;
            case ShadowResolution.High: shadowResDropdown.value = 2; break;
            case ShadowResolution.VeryHigh: shadowResDropdown.value = 3; break;
        }
        shadowResDropdown.RefreshShownValue();

        textureResDropdown.value = QualitySettings.masterTextureLimit;
        textureResDropdown.RefreshShownValue();

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(qualitySettings);
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();

        #endregion

        #region Set Toggles

        fullscreenToggle.isOn = Screen.fullScreen;
        if (QualitySettings.anisotropicFiltering == AnisotropicFiltering.Enable)
        {
            anisotropicToggle.isOn = true;
        }
        else
        {
            anisotropicToggle.isOn = false;
        }

        #endregion
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
        RefreshValues();
    }

    public void SetAnisotropic (bool usesAnisotropic)
    {
        if (usesAnisotropic)
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
        }
        else
        {
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        }
    }

    public void SetAntiAliasing (int antiAliasingIndex)
    {
        switch(antiAliasingIndex)
        {
            case 0: QualitySettings.antiAliasing = 0; break;
            case 1: QualitySettings.antiAliasing = 2; break;
            case 2: QualitySettings.antiAliasing = 4; break;
            case 3: QualitySettings.antiAliasing = 8; break;
        }
    }

    public void SetShadows (int shadowIndex)
    {
        switch (shadowIndex)
        {
            case 0: QualitySettings.shadows = ShadowQuality.Disable; break;
            case 1: QualitySettings.shadows = ShadowQuality.HardOnly; break;
            case 2: QualitySettings.shadows = ShadowQuality.All; break;
        }
    }

    public void SetShadowResolution (int shadowResIndex)
    {
        switch (shadowResIndex)
        {
            case 0: QualitySettings.shadowResolution = ShadowResolution.Low; break;
            case 1: QualitySettings.shadowResolution = ShadowResolution.Medium; break;
            case 2: QualitySettings.shadowResolution = ShadowResolution.High; break;
            case 3: QualitySettings.shadowResolution = ShadowResolution.VeryHigh; break;
        }
    }

    public void SetTextureResolution (int textureResIndex)
    {
        QualitySettings.masterTextureLimit = textureResIndex;
    }

    public void RefreshValues()
    {
        #region Set Other Dropdowns

        switch(QualitySettings.antiAliasing)
        {
            case 0: antiAliasingDropdown.value = 0; break;
            case 2: antiAliasingDropdown.value = 1; break;
            case 4: antiAliasingDropdown.value = 2; break;
            case 8: antiAliasingDropdown.value = 3; break;
        }        
        antiAliasingDropdown.RefreshShownValue();

        switch(QualitySettings.shadows)
        {
            case ShadowQuality.Disable: shadowsDropdown.value = 0; break;
            case ShadowQuality.HardOnly: shadowsDropdown.value = 1; break;
            case ShadowQuality.All: shadowsDropdown.value = 2; break;
        }
        shadowsDropdown.RefreshShownValue();

        switch(QualitySettings.shadowResolution)
        {
            case ShadowResolution.Low: shadowResDropdown.value = 0; break;
            case ShadowResolution.Medium: shadowResDropdown.value = 1; break;
            case ShadowResolution.High: shadowResDropdown.value = 2; break;
            case ShadowResolution.VeryHigh: shadowResDropdown.value = 3; break;
        }
        shadowResDropdown.RefreshShownValue();

        textureResDropdown.value = QualitySettings.masterTextureLimit;
        textureResDropdown.RefreshShownValue();

        #endregion

        #region Set Toggles

        fullscreenToggle.isOn = Screen.fullScreen;
        if (QualitySettings.anisotropicFiltering == AnisotropicFiltering.Enable)
        {
            anisotropicToggle.isOn = true;
        }
        else
        {
            anisotropicToggle.isOn = false;
        }

        #endregion
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
