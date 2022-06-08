using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBoxRifle : WeaponBase
{
    public int AccuracyRange;
    public float accuracy;
    System.Random random = new System.Random();

    public override void Init()
    {
    }

    public override void Tick()
    {
    }

    public override void Fire()
    {
        accuracy = (random.Next(0, AccuracyRange));
        accuracy = accuracy - (AccuracyRange / 2);
        angle += accuracy;
        GameObject proj = Instantiate(projectile, WeaponManager.instance.f11Firepoint.transform.position, Quaternion.Euler(0, yRot, 0));
        proj.GetComponent<ProjectileBehaviour>().FireProjectile(targetDistance * 2);
        proj.GetComponent<ProjectileBehaviour>().colour = WeaponManager.instance.currentWeapon.colour;
    }
}
