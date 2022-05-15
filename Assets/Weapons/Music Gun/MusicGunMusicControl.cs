using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicGunMusicControl : MonoBehaviour
{
    public GameObject discoLights;

    public AudioSource source;
    public AudioClip song;

    void Update()
    {
        if (PlayerStats.instance == null)
            return;

        if (WeaponManager.instance == null)
            return;

        if (PlayerStats.instance.isDead)
            return;

        if (PauseMenu.isPaused)
            return;

        if (WeaponManager.instance.currentWeapon == null)
            return;

        if (WeaponManager.instance.currentWeapon.weaponId != 12)
            return;

        if (WeaponManager.instance.currentWeapon.currentAmmo <= 0)
            return;

        if (Input.GetButton("Fire1"))
        {
            discoLights.SetActive(true);
            /* source.clip = song;
            if (!source.isPlaying)
            {                
                source.Play();
            } */
        }
        else
        {
            //source.Stop();
            discoLights.SetActive(false);
        }
    }
}
