using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBlackHole : WeaponBase
{

    public override void Init()
    {
        angle = 45f;
    }

    public override void Tick()
    {
    }

    public override void Fire()
    {
        if (canFire)
        {
            GameObject proj = Instantiate(projectile, WeaponManager.instance.firepoint.transform.position, Quaternion.Euler(0, yRot, 0));
            proj.GetComponent<ProjectileBehaviour>().FireProjectile(targetDistance * 2);
            timer = 0;
        }
    }

}
