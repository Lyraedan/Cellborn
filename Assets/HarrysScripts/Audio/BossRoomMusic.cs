using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoomMusic : MonoBehaviour
{
    public AudioClip bossMusic;

    void Start()
    {
        MusicManager.instance.source.clip = bossMusic;
        MusicManager.instance.source.Play();
    }
}
