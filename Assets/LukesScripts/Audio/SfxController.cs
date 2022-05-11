using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxController : MonoBehaviour
{
    private AudioSource source;

    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    void Update()
    {
        source.volume = (AudioManagerRevised.instance.GetMasterVolume() * AudioManagerRevised.instance.GetSfxVolume()) / 1f;
    }
}
