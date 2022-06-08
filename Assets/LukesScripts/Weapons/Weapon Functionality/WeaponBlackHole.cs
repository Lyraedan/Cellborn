using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBlackHole : WeaponBase
{
    public override void Init()
    {

    }

    public override void Tick()
    {
    }

    public override void Fire()
    {
        GameObject proj = Instantiate(projectile, WeaponManager.instance.BHGFirepoint.transform.position, Quaternion.Euler(0, yRot, 0));
        proj.GetComponent<ProjectileBehaviour>().FireProjectile(targetDistance);
        proj.GetComponent<ProjectileBehaviour>().colour = WeaponManager.instance.currentWeapon.colour;    }
}
