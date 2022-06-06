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

    [Header("Ammo UI Settings")]
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

    [Header("Multiple Projectiles")]
    [Range(0.0f, 360.0f)]
    public float fireAngle = 90f;

    [Header("Weapon Prefab")]
    public GameObject prefab;

    [Header("Aiming")]
    public float targetDistance;

    void Start()
    {
        if (equippedWeapon != null)
        {
            UpdateParameters(equippedWeapon);
        }
        else
        {
            SetParametersToNull();
        }
    }

    void Update()
    {              
        #region Aiming
        
        if(target == null)
        {
            Debug.LogError("Aiming target is unassigned!");
            return;
        }

        targetDistance = Vector3.Distance(player.transform.position, target.position);

        player.LookAt(target.position);
        player.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        #endregion

        #region Ammo Values

        /*
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
        */
        #endregion

        #region Firing Conditionals (WARNING! LOTS OF COMPOUND IF STATEMENTS! ENTER IF YOU DARE!)

        //Check if weapon uses ammo
        if (usesAmmo)
        {
            ammoText.enabled = true;
            
            //Check if weapon is rapidfire
            if (isRapidFire)
            {
                if (Input.GetButton(ControlManager.INPUT_FIRE))
                {
                    tFire += Time.deltaTime;

                    if (tFire >= fireRate)
                    {
                        if (currentAmmo > 0)
                        {
                            SpawnProjectile(projectile, shotsPerFire, fireAngle);
                            currentAmmo--;
                            equippedWeapon.ChangeAmmo(-1);
                            tFire = 0;
                        }
                    }
                }
            }
            else
            {
                if (Input.GetButtonDown(ControlManager.INPUT_FIRE) && canFire)
                {
                    if (currentAmmo > 0)
                    {
                        SpawnProjectile(projectile, shotsPerFire, fireAngle);
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
            //ammoText.enabled = false;

            if (isRapidFire)
            {
                tFire += Time.deltaTime;

                if (tFire >= fireRate)
                {
                    if (Input.GetButton(ControlManager.INPUT_FIRE))
                    {
                        SpawnProjectile(projectile, shotsPerFire, fireAngle);
                        canFire = false;                        
                        tCooldown = cooldownTime;
                    } 
                }
            }
            else
            {
                if (Input.GetButtonDown(ControlManager.INPUT_FIRE) && canFire)
                {
                    SpawnProjectile(projectile, shotsPerFire, fireAngle);
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

        #endregion
        
        /*
        if (equippedWeapon != null)
        {
            weaponText.text = "Equipped weapon: " + equippedWeapon.name.ToString();
        }
        else
        {
            weaponText.text = "";
        }

        ammoText.text = "Ammo: " + currentAmmo + " / " + maxAmmo;
        */
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

        fireAngle = weapon.fireAngle;

        prefab = weapon.prefab;
    }

    public void SetParametersToNull()
    {
        projectile = null;

        usesAmmo = false;
        maxAmmo = 0;
        currentAmmo = 0;
        shotsPerFire = 0;

        isRapidFire = false;
        fireRate = 0;

        cooldownTime = 0;

        fireAngle = 0;        
    }

    public void SpawnProjectile(GameObject projectile, int shots, float angle)
    {
        for (int i = 0; i < shots; i++)
        {
            float y = ((transform.eulerAngles.y - (angle / 2)) + ((angle / ((shots + 1)) * (i + 1))));

            Vector3 pos = transform.position;
            pos.y += -0.1f;

            GameObject projInstance = Instantiate(projectile, transform.position, Quaternion.Euler(0, y, 0));
            projInstance.GetComponent<ProjectileBehaviour>().FireProjectile(targetDistance * 2);
        }        
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
