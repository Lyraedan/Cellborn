using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellMusicController : MonoBehaviour
{
    bool hasExited;
    public AudioSource source;
    public AudioClip cellMusic;
    public AudioClip levelMusic;

    void Start()
    {
        /* source.Stop();
        source.clip = cellMusic;
        source.Play(); */

        MusicManager.instance.SetClip(cellMusic);
        MusicManager.instance.source.Play();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            if (!hasExited)
            {
                /* source.Stop();
                source.clip = levelMusic;
                source.Play(); */

                MusicManager.instance.SetClip(levelMusic);
                MusicManager.instance.source.Play();

                hasExited = true;
            }
        }
    }
}
