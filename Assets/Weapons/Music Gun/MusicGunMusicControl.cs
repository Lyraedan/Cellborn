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

        if (WeaponManager.instance.currentWeapon == null)
            return;

        if (WeaponManager.instance.currentWeapon.weaponId != 12)
            return;

        if (WeaponManager.instance.currentWeapon.currentAmmo <= 0)
            return;

        if (Input.GetButton("Fire1"))
        {
            if (!source.isPlaying)
            {
                source.clip = song;
                source.Play();
                discoLights.SetActive(true);
            }
        }
        else
        {
            source.Stop();
            discoLights.SetActive(false);
        }
    }
}
