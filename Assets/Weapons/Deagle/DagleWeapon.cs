using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DagleWeapon : WeaponBase
{
    public WeaponProperties weaponProperties;
    public Animator animController;

    public override void Init()
    {

    }

    public override void Tick()
    {

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
