using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    public List<Resolution> resolutions;
    public List<string> resolutionStr;
    public TMP_Dropdown resolutionDropdown;
    
    void Start()
    {
        resolutions = Screen.resolutions.ToList();

        resolutionDropdown.ClearOptions();
                
        int currentResolutionIndex = 0;        
        for(int i = 0; i < resolutions.Count; i++)
        {
            string resString = resolutions[i].width + " x " + resolutions[i].height;
            resolutionStr.Add(resString);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        
        resolutionDropdown.AddOptions(resolutionStr);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
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
}
