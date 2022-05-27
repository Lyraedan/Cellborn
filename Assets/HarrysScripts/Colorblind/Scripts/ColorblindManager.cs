using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorblindManager : MonoBehaviour
{
    public static ColorblindManager instance;

    public int colorblindType;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        DontDestroyOnLoad(this);
    }
    
    public void SetColorblindMode(int colorType)
    {
        colorblindType = colorType;
    }
}
