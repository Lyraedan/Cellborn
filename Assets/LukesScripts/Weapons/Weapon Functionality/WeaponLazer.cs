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
    }

    public override void Fire()
    {
        if (WeaponManager.instance.laserController.isFiring)
        {
            WeaponProperties weaponProperties = gameObject.GetComponent<WeaponProperties>();
            weaponProperties.RemoveAmmo(1);
            
            RaycastHit hit;
            GameObject hitEnemy;
            Vector3 direction = WeaponManager.instance.firepoint.transform.forward;
            int layerMask = (1 << LayerMask.NameToLayer("Enemy") | 1 << LayerMask.NameToLayer("Default")); // Only collide with entities with Layers Enemy and Default
            if (Physics.Raycast(WeaponManager.instance.firepoint.transform.position, direction, out hit, Mathf.Infinity, layerMask))
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
                        if (weaponProperties.colour == enemyScript.colour)
                        {
                            enemyScript.currentHP -= (enemyDamage*2);
                        }
                        else if (weaponProperties.colour == Color.grey)
                        {
                            enemyScript.currentHP -= enemyDamage;
                        }
                        else
                        {
                            enemyScript.currentHP -= (enemyDamage/2);
                        }

                        if (WeaponManager.instance.currentWeapon.colour == Color.red)
                        {
                            enemyScript.temperature.temperature += 10;
                        }
                        else if (WeaponManager.instance.currentWeapon.colour == Color.blue)
                        {
                            enemyScript.temperature.temperature -= 10;
                        }

                        if (hitEnemy.GetComponent<AITest>() != null)
                        {
                            hitEnemy.GetComponent<AITest>().Hit();
                        }
                        else if (hitEnemy.GetComponent<AIFairy>() != null)
                        {
                            hitEnemy.GetComponent<AIFairy>().Hit();
                        }
                        else if (hitEnemy.GetComponent<AIWizard>() != null)
                        {
                            hitEnemy.GetComponent<AIWizard>().Hit();
                        }
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
}
