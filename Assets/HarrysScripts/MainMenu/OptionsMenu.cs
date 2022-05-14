using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Rendering.PostProcessing;
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

<<<<<<< Updated upstream
    public AudioSource source;
    public AudioClip click, error;

    void Start()
    {
        OpenGraphics();

        #region Set Resolution Dropdown

=======
<<<<<<< Updated upstream
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
        
=======
    public TextMeshProUGUI pageText;
    public List<GameObject> pages;
    public GameObject page01Button;
    public GameObject page02Button;

    public PostProcessVolume volume;
    private AmbientOcclusion _ambientOcclusion;
    private Vignette _vignette;
    private Bloom _bloom;
    private ChromaticAberration _chromaticAberration;
    private DepthOfField _depthOfField;

    public Toggle ambientOcclusionToggle;
    public Toggle vignetteToggle;
    public Toggle bloomToggle;
    public Toggle chromaticAberrationToggle;
    public Toggle depthOfFieldToggle;

    [Header("Misc")]
    public AudioSource source;
    public AudioClip click, error;

    void Start()
    {
        OpenGraphics();

        #region Get Post Processing

        volume.profile.TryGetSettings(out _ambientOcclusion);
        volume.profile.TryGetSettings(out _vignette);
        volume.profile.TryGetSettings(out _bloom);
        volume.profile.TryGetSettings(out _chromaticAberration);
        volume.profile.TryGetSettings(out _depthOfField);

        #endregion

        #region Set Resolution Dropdown

>>>>>>> Stashed changes
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
=======
<<<<<<< Updated upstream
        /* masterSlider.value = Mathf.Pow(10, GetMasterVolume()) / 20;
        musicSlider.value = Mathf.Pow(10, GetMusicVolume()) / 20;
        soundSlider.value = Mathf.Pow(10, GetSFXVolume()) / 20;
        masterVolText.text = (int)(Mathf.Pow(10, GetMasterVolume()) / 20) + "%";
        musicVolText.text = (int)(Mathf.Pow(10, GetMusicVolume()) / 20) + "%";
        soundVolText.text = (int)(Mathf.Pow(10, GetSFXVolume()) / 20) + "%"; */
=======
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
        #endregion
=======
        ambientOcclusionToggle.isOn = _ambientOcclusion.active;
        vignetteToggle.isOn = _vignette.active;
        bloomToggle.isOn = _bloom.active;
        chromaticAberrationToggle.isOn = _chromaticAberration.active;
        depthOfFieldToggle.isOn = _depthOfField.active;

        #endregion
>>>>>>> Stashed changes
>>>>>>> Stashed changes
    }

    #region Page01

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

<<<<<<< Updated upstream
    public void SetTextureResolution (int textureResIndex)
    {
        QualitySettings.masterTextureLimit = textureResIndex;
    }

    public void RefreshValues()
=======
<<<<<<< Updated upstream
    public float GetMusicVolume()
=======
    #endregion

    #region Page02

    public void SetAmbientOcclusion(bool usesAO)
    {
        _ambientOcclusion.active = usesAO;
    }

    public void SetVignette(bool usesVignette)
    {
        _vignette.active = usesVignette;
    }

    public void SetBloom(bool usesBloom)
    {
        _bloom.active = usesBloom;
    }

    public void SetChromaticAberration(bool usesCA)
    {
        _chromaticAberration.active = usesCA;
    }

    public void SetDepthOfField(bool usesDOF)
    {
        _depthOfField.active = usesDOF;
    }

    #endregion

    public void RefreshValues()
>>>>>>> Stashed changes
>>>>>>> Stashed changes
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

    public void SetPage(int page)
    {
        foreach(GameObject pageGroup in pages)
        {
            pageGroup.SetActive(false);
        }
        pages[page].SetActive(true);

        switch (page)
        {
            case 0:
                page01Button.SetActive(false);
                page02Button.SetActive(true);
                pageText.text = "Page " + (page + 1) + "/" + pages.Count;
                break;
            case 1:
                page01Button.SetActive(true);
                page02Button.SetActive(false);
                pageText.text = "Page " + (page + 1) + "/" + pages.Count;
                break;
        }
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
