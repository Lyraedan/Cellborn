using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Basic Settings")]  
    public new string name;
    [TextArea(3, 10)]
    public string description;
    public Sprite icon;
    public GameObject projectile;

    [Header("Ammo Settings")]
    public bool usesAmmo;
    public int maxAmmo;
    public int currentAmmo;
    public int shotsPerFire;

    [Header("Fire Rate")]
    public bool isRapidFire;
    public float fireRate;

    [Header("Cooldown")]
    public float cooldownTime;

    [Header("Multiple Projectiles")]
    [Range(0.0f, 360.0f)]
    public float fireAngle = 90f;

    [Header("Weapon Prefab")]
    public GameObject prefab;

    /* private void OnEnable()
    {
        currentAmmo = maxAmmo;
    } */

    public void ChangeAmmo(int ammoVal)
    {
        currentAmmo += ammoVal;

        if (currentAmmo > maxAmmo)
        {
            currentAmmo = maxAmmo;
        }
        
        if (currentAmmo < 0)
        {
            currentAmmo = 0;
        }
    }

    public int GetAmmo()
    {
        return currentAmmo;
    }
}
