using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicGunMusicControl : MonoBehaviour
{
    public string songName;
    
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
                        }
                    }
                    else
                    {
                        AudioManager.instance.Stop(songName);
                    }
                }
                else
                {
                    AudioManager.instance.Stop(songName);
                }
            }
            else
            {
                AudioManager.instance.Stop(songName);
            }
        }
        else
        {
            AudioManager.instance.Stop(songName);
        }
    }
}
