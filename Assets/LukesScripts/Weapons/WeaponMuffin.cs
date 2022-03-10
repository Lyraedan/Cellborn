using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponMuffin : WeaponBase
{

    public override void Init()
    {
    }

    public override void Tick()
    {
    }

    public override void Fire()
    {
        GameObject proj = Instantiate(projectile, WeaponManager.instance.firepoint.transform.position, Quaternion.Euler(0, yRot, 0));
        proj.GetComponent<ProjectileBehaviour>().FireProjectile(targetDistance * 2);
    }
}
