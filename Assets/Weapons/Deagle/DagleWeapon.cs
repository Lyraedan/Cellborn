using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DagleWeapon : WeaponBase
{
    public override void Init()
    {

    }

    public override void Tick()
    {

    }

    public override void Fire()
    {
        AudioManager.instance.Play("PistolFire");
    }
}
