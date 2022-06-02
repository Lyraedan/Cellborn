using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoomMusic : MonoBehaviour
{
    public AudioClip bossMusic;

    void Start()
    {
        MusicManager.instance.SetClip(bossMusic);
        MusicManager.instance.source.Play();
    }
}
