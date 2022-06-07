using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponMusicGun : WeaponBase
{
    public int AccuracyRange;
    public float accuracy;
    System.Random random = new System.Random();

    public float discoCooldown;
    float cdTimer;
    bool hasCooledDown;

    public WeaponProperties weaponProperties;
    public Animator animController;

    public override void Init()
    {
        hasCooledDown = true;
    }

    public override void Tick()
    {
        MusicGunMusicControl.instance.hasCooledDown = hasCooledDown;
        
        if (Input.GetButton(ControlManager.INPUT_FIRE))
        {
            if (hasCooledDown)
            {
                if (secondsBetweenShots >= 0.06f)
                {
                    secondsBetweenShots = secondsBetweenShots - 0.0001f;
                }
            }
        }
        else
        {
            secondsBetweenShots = 1000f;
            cdTimer = 0f;

            if (hasCooledDown)
            {
                hasCooledDown = false;
            }
        }

        if (!hasCooledDown)
            {
                cdTimer += 1f * Time.deltaTime;

                if (cdTimer >= discoCooldown)
                {
                    hasCooledDown = true;
                    cdTimer = 0f;
                    secondsBetweenShots = 0.2f;
                }
            }
    }

    public void Start()
    {
        animController = weaponProperties.animController;
    }

    public override void Fire()
    {
        animController.SetBool("IsShooting", true);
    }
}
