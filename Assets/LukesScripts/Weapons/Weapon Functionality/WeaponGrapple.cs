using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponGrapple : WeaponBase
{
    public GrappleHook grapple;

    public override void Init()
    {
    }

    public override void Tick()
    {
    }

    public override void Fire()
    {
        GameObject proj = Instantiate(projectile, WeaponManager.instance.firepoint.transform.position, Quaternion.Euler(0, yRot, 0));
        grapple = proj.GetComponent<GrappleHook>();
        proj.GetComponent<ProjectileBehaviour>().FireProjectile(targetDistance * 3);
        proj.GetComponent<ProjectileBehaviour>().colour = WeaponManager.instance.currentWeapon.colour;
    }
}
