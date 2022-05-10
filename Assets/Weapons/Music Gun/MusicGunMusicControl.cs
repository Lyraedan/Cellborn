using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicGunMusicControl : MonoBehaviour
{
    public string songName;
    public GameObject discoLights;
    
    void Update()
    {
        if (!PlayerStats.instance.isDead)
        {
            if (WeaponManager.instance.currentWeapon.weaponId == 12)
            {
                if (WeaponManager.instance.currentWeapon.currentAmmo > 0)
                {
                    if (Input.GetButton("Fire1"))
                    {
                        if (!AudioManager.instance.IsPlaying(songName))
                        {
                            AudioManager.instance.Play(songName);
                            discoLights.SetActive(true);
                        }
                    }
                    else
                    {
                        AudioManager.instance.Stop(songName);
                        discoLights.SetActive(false);
                    }
                }
                else
                {
                    AudioManager.instance.Stop(songName);
                    discoLights.SetActive(false);
                }
            }
            else
            {
                AudioManager.instance.Stop(songName);
                discoLights.SetActive(false);
            }
        }
        else
        {
            AudioManager.instance.Stop(songName);
            discoLights.SetActive(false);
        }
    }
}
