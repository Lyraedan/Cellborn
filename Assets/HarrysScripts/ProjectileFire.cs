using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProjectileFire : MonoBehaviour
{
    [Header("Basic Settings")]    
    public GameObject projectile;
    public Weapon equippedWeapon;
    public TextMeshProUGUI weaponText;
    public Transform projectileSpawn, player, target;

    [Header("Ammo Settings")]
    public bool usesAmmo;
    public int maxAmmo, currentAmmo;
    public int shotsPerFire;
    public TextMeshProUGUI ammoText;
    public Color ammoFull, ammoNormal, ammoEmpty;

    [Header("Fire Rate")]
    public bool isRapidFire;
    public float fireRate;
    float tFire;

    [Header("Cooldown")]
    public float cooldownTime;
    float tCooldown;
    bool canFire;

    void Start()
    {
        UpdateParameters(equippedWeapon);
    }

    void Update()
    {              
        #region Aiming
        
        player.LookAt(target.position);
        player.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        #endregion

        #region Ammo Values

        if (currentAmmo > maxAmmo)
        {
            currentAmmo = maxAmmo;
        }
        if (currentAmmo == maxAmmo)
        {
            ammoText.color = ammoFull;
        }

        if (currentAmmo < 0)
        {
            currentAmmo = 0;
        }
        if (currentAmmo == 0)
        {
            ammoText.color = ammoEmpty;
        }

        if (currentAmmo < maxAmmo && currentAmmo > 0)
        {
            ammoText.color = ammoNormal;
        }

        #endregion

        if (usesAmmo)
        {
            ammoText.enabled = true;
            
            if (isRapidFire)
            {
                if (Input.GetButton("Fire1"))
                {
                    tFire += Time.deltaTime;

                    if (tFire >= fireRate)
                    {
                        if (currentAmmo > 0)
                        {
                            GameObject projInstance = Instantiate(projectile, transform.position, transform.rotation);
                            currentAmmo--;
                            equippedWeapon.ChangeAmmo(-1);
                            tFire = 0;
                        }
                    }
                }
            }
            else
            {
                if (Input.GetButtonDown("Fire1") && canFire)
                {
                    if (currentAmmo > 0)
                    {
                        GameObject projInstance = Instantiate(projectile, transform.position, transform.rotation);
                        currentAmmo--;
                        equippedWeapon.ChangeAmmo(-1);
                        canFire = false;
                        tCooldown = cooldownTime;
                    }
                }

                tCooldown -= Time.deltaTime;

                if(tCooldown <= 0)
                {
                    canFire = true;
                } 
            }
        }
        else
        {
            ammoText.enabled = false;

            if (isRapidFire)
            {
                tFire += Time.deltaTime;

                if (tFire >= fireRate)
                {
                    if (Input.GetButton("Fire1"))
                    {
                        GameObject projInstance = Instantiate(projectile, transform.position, transform.rotation);
                        canFire = false;                        
                        tCooldown = cooldownTime;
                    } 
                }
            }
            else
            {
                if (Input.GetButtonDown("Fire1") && canFire)
                {
                    GameObject projInstance = Instantiate(projectile, transform.position, transform.rotation);
                    canFire = false;
                    tCooldown = cooldownTime;
                }

                tCooldown -= Time.deltaTime;

                if(tCooldown <= 0)
                {
                    canFire = true;
                } 
            }            
        }
    
        weaponText.text = "Equipped weapon: " + equippedWeapon.name.ToString();
        ammoText.text = "Ammo: " + currentAmmo + " / " + maxAmmo;
    }

    public void UpdateParameters(Weapon weapon)
    {
        projectile = weapon.projectile;

        usesAmmo = weapon.usesAmmo;
        maxAmmo = weapon.maxAmmo;
        currentAmmo = weapon.currentAmmo;
        shotsPerFire = weapon.shotsPerFire;

        isRapidFire = weapon.isRapidFire;
        fireRate = weapon.fireRate;

        cooldownTime = weapon.cooldownTime;
    }
}
