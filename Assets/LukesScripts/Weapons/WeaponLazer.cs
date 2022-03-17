using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponLazer : WeaponBase
{
    public bool isFiring = false;
    public LayerMask hitLayer;
    public int enemyDamage;
    public float damageInterval;
    public float fireDistance;    
    float t;
    bool weaponIsFiring;
    
    public override void Init()
    {
    }

    public override void Tick()
    {  
    }

    public override void Fire()
    {
        RaycastHit hit;
        GameObject hitEnemy;
        EnemyScript enemyScript;
        LaserControl laserControl = FindObjectOfType<LaserControl>();
        Debug.DrawRay(WeaponManager.instance.firepoint.transform.position, WeaponManager.instance.firepoint.transform.forward, Color.red);
        if (laserControl != null)
        {
            if (laserControl.isFiring)
            {
                if (Physics.Raycast(WeaponManager.instance.firepoint.transform.position, transform.TransformDirection(WeaponManager.instance.firepoint.transform.forward), out hit, fireDistance))
                {
                    if (hit.collider.tag == "Enemy")
                    {
                        Debug.Log("Enemy hit!");
                        hitEnemy = hit.transform.gameObject;

                        enemyScript = hitEnemy.GetComponent<EnemyScript>();

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
        else
        {
            Debug.LogWarning("No particles found!!!");
        }
    }
}
