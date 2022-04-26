using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellMusicController : MonoBehaviour
{
    public string cellMusicName, levelMusicName;
    bool hasExited;

    void Start()
    {
        AudioManager.instance.Stop(levelMusicName);
        AudioManager.instance.Play(cellMusicName);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            if (!hasExited)
            {
                AudioManager.instance.Stop(cellMusicName);
                AudioManager.instance.Play(levelMusicName);
                hasExited = true;
            }
        }
    }
}
