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
            RaycastHit hit;
            GameObject hitEnemy;
            Vector3 direction = WeaponManager.instance.firepoint.transform.forward;
            int layerMask = (1 << LayerMask.NameToLayer("Enemy") | 1 << LayerMask.NameToLayer("Default")); // Only collide with entities with Layers Enemy and Default
            if (Physics.Raycast(WeaponManager.instance.firepoint.transform.position, direction, out hit, Mathf.Infinity))
            {
                Debug.Log("Laser hit: " + (hit.collider != null ? hit.collider.name : "nothing"));
                Debug.DrawRay(WeaponManager.instance.firepoint.transform.position, direction * hit.distance, Color.yellow);

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
                Debug.DrawRay(WeaponManager.instance.firepoint.transform.position, direction, Color.green);
                hitEnemy = null;
                return;
            }
        }
    }

    public override void Fire()
    {

    }
}
