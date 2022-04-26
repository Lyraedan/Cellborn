using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponMusicGun : WeaponBase
{
    public int AccuracyRange;
    public float accuracy;
    System.Random random = new System.Random();

    public override void Init()
    {
        
    }

    public override void Tick()
    {
        if (Input.GetButton("Fire1"))
        {
            if (secondsBetweenShots >= 0.06f)
            {
                secondsBetweenShots = secondsBetweenShots - 0.0001f;
            }
        }
        else
        {
            secondsBetweenShots = 0.2f;
        }
    }

    public override void Fire()
    {

    }
}
