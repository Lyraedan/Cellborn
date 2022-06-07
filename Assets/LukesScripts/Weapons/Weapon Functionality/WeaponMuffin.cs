using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponMuffin : WeaponBase
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
        GameObject proj = Instantiate(projectile, WeaponManager.instance.firepoint.transform.position, Quaternion.Euler(0, yRot, 0));
        proj.GetComponent<ProjectileBehaviour>().FireProjectile(targetDistance * 3);
        animController.SetBool("IsShooting", true);
    }
}
