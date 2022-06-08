using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoomMusic : MonoBehaviour
{
    public static BossRoomMusic instance;
    public AudioClip bossMusic;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public void Play()
    {
        MusicManager.instance.source.clip = bossMusic;
        MusicManager.instance.source.Play();
    }
}
