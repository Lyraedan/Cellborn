using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Objects/New Weapon")]
public class Weapon : ScriptableObject
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

    private void OnEnable()
    {
        currentAmmo = maxAmmo;
    }

    public void ChangeAmmo(int ammoVal)
    {
        currentAmmo += ammoVal;
    }
}
