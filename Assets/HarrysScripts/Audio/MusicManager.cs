using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;
    
    public AudioSource source;
    
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        DontDestroyOnLoad(this);
    }

    public void SetClip (AudioClip musicClip)
    {
        source.clip = musicClip;
    }
}
