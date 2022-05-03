using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBouncy : WeaponBase
{
    public float coneSize = 1f;
    public uint numberOfShots = 10;
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
        for (int i = 0; i < numberOfShots; i++)
        {
            /*float xSpread = Random.Range(-1f, 1f) * coneSize;
            float zSpread = Random.Range(-1f, 1f) * coneSize;
            float x = xSpread * Mathf.Cos(Random.Range(0, 2 * Mathf.PI));
            float z = zSpread * Mathf.Sin(Random.Range(0, 2 * Mathf.PI));
            Vector3 direction = new Vector3(x, yRot, z);*/

            accuracy = (random.Next(0, AccuracyRange));
            accuracy = accuracy - (AccuracyRange / 2);
            angle = accuracy;

            GameObject proj = Instantiate(projectile, WeaponManager.instance.firepoint.transform.position, Quaternion.Euler(0, yRot, 0));
            proj.GetComponent<ProjectileBehaviour>().FireProjectile(targetDistance*2);
            proj.GetComponent<ProjectileBehaviour>().colour = WeaponManager.instance.currentWeapon.colour;
        }
        AudioManager.instance.Play("SirBoingsFire");
    }
}
