using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    private AudioSource source;

    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    void Update()
    {
        source.volume = (AudioManagerRevised.instance.GetMasterVolume() * AudioManagerRevised.instance.GetMusicVolume()) / 1f;
    }
}
