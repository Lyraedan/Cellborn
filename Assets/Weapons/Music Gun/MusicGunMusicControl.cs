using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicGunMusicControl : MonoBehaviour
{
    public static MusicGunMusicControl instance;
    
    public GameObject discoLights;

    public AudioSource source;
    public AudioClip song;

    private bool playing = false;
    public bool hasCooledDown;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        source.loop = true;
        source.clip = song;
    }

    void Update()
    {
        if (PlayerStats.instance == null)
        {
            Debug.LogError("[Music Gun] PlayerStats is null");
            return;
        }

        if (WeaponManager.instance == null)
            return;

        if (PlayerStats.instance.isDead)
        {
            StopMusic();
            return;
        }

        if (PauseMenu.isPaused)
        {
            StopMusic();
            return;
        }

        if (WeaponManager.instance.currentWeapon == null)
        {
            StopMusic();
            return;
        }

        if (WeaponManager.instance.currentWeapon.weaponId != 12)
        {
            StopMusic();
            return;
        }

        if (WeaponManager.instance.currentWeapon.currentAmmo <= 0)
        {
            StopMusic();
            return;
        }

        if (!hasCooledDown)
        {
            StopMusic();
            return;
        }

        if (Input.GetAxisRaw(ControlManager.INPUT_FIRE) > 0)
        {
            if(!playing)
            {
                Debug.Log("[Music Gun] Fire1 is pressed");

                MusicManager.instance.source.Pause();

                source.Play();
                playing = true;
                discoLights.SetActive(true);
            }
        }
        else
        {
            StopMusic();
        }
    }

    void StopMusic()
    {
        if (playing)
        {
            Debug.Log("[Music Gun] Stopping music gun music");
            source.Stop();
            playing = false;
            discoLights.SetActive(false);
            MusicManager.instance.source.UnPause();
        }
    }
}
