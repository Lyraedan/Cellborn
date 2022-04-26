using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponGrapple : WeaponBase
{

    public override void Init()
    {
    }

    public override void Tick()
    {
    }

    public override void Fire()
    {
        AudioManager.instance.Play("ThrowFire");
        GameObject proj = Instantiate(projectile, WeaponManager.instance.firepoint.transform.position, Quaternion.Euler(0, yRot, 0));
        proj.GetComponent<ProjectileBehaviour>().FireProjectile(targetDistance * 3);
    }
}
