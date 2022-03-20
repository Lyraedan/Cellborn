using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBouncy : WeaponBase
{
    public float coneSize = 1f;

    public override void Init()
    {

    }

    public override void Tick()
    {
    }

    public override void Fire()
    {
        for (int i = 0; i < 1; i++)
        {
            float xSpread = Random.Range(-1f, 1f) * coneSize;
            float zSpread = Random.Range(-1f, 1f) * coneSize;
            float x = xSpread * Mathf.Cos(Random.Range(0, 2 * Mathf.PI));
            float z = zSpread * Mathf.Sin(Random.Range(0, 2 * Mathf.PI));
            Vector3 direction = new Vector3(x, yRot, z) * (i + 1);

            GameObject proj = Instantiate(projectile, WeaponManager.instance.firepoint.transform.position, Quaternion.Euler(direction));
            proj.GetComponent<ProjectileBehaviour>().FireProjectile(targetDistance * 2);
        }
    }
}
