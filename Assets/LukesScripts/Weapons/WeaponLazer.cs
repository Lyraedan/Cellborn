using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponLazer : WeaponBase
{
    public bool isFiring = false;
    public LayerMask hitLayer;
    public int enemyDamage;
    public float damageInterval;
    float t;
    bool weaponIsFiring;

    public override void Init()
    {
        secondsBetweenShots = 0;
    }

    public override void Tick()
    {
        if (WeaponManager.instance.laserController == null)
            return;

        if (WeaponManager.instance.laserController.isFiring)
        {
            Debug.Log("FIRING THE LASER");
            RaycastHit hit;
            GameObject hitEnemy;
            Debug.DrawRay(WeaponManager.instance.firepoint.transform.position, WeaponManager.instance.firepoint.transform.forward, Color.green);
            if (Physics.Raycast(WeaponManager.instance.firepoint.transform.position, transform.TransformDirection(WeaponManager.instance.firepoint.transform.forward), out hit, Mathf.Infinity))
            {
                Debug.Log("Laser hit: " + (hit.collider != null ? hit.collider.tag : "nothing"));
                if (hit.collider.CompareTag("Enemy"))
                {
                    Debug.Log("Enemy hit!");
                    hitEnemy = hit.transform.gameObject;

                    var enemyScript = hitEnemy.GetComponent<EnemyScript>();

                    t += 1f * Time.deltaTime;
                    if (t >= damageInterval)
                    {
                        enemyScript.currentHP -= enemyDamage;
                        t = 0;
                    }
                }
                else
                {
                    Debug.Log("Not an enemy... :(");
                }
            }
            else
            {
                hitEnemy = null;
                return;
            }
        }
    }

    public override void Fire()
    {

    }
}
