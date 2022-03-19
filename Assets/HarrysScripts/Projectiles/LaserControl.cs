using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserControl : MonoBehaviour
{
    public ParticleSystem laserParticles;
    public WeaponManager weaponManager;

    public float warmup, cooldown;

    [HideInInspector]public bool isFiring;

    WeaponProperties currentWeapon;
    float warmupTimer, cooldownTimer;
    bool hasWarmedUp, hasCooledDown;

    void Start()
    {
        laserParticles.Stop();
        weaponManager = FindObjectOfType<WeaponManager>();
    }

    void Update()
    {
        currentWeapon = weaponManager.currentWeapon;
        var psMain = laserParticles.main;

        if (currentWeapon == null)
        {
            Debug.LogWarning("No weapon selected!");
        }
        else
        {
            if (currentWeapon.weaponId == 5)
            {
                if (Input.GetButton("Fire1") && currentWeapon.currentAmmo > 0 && hasCooledDown)
                {
                    hasWarmedUp = false;
                    cooldownTimer = 0f;
                    warmupTimer += 1f * Time.deltaTime;

                    if (warmupTimer >= warmup && !hasWarmedUp)
                    {
                        hasWarmedUp = true;
                        
                    }
                    
                    if (hasWarmedUp)
                    {
                        //psMain.simulationSpace = ParticleSystemSimulationSpace.Local;
                        laserParticles.Play();
                        isFiring = true;
                    }
                }
                else
                {
                    laserParticles.Stop();
                    isFiring = false;
                    hasWarmedUp = false;

                    if (!hasCooledDown)
                    {
                        cooldownTimer += 1f * Time.deltaTime;

                        if (cooldownTimer >= cooldown)
                        {
                            hasCooledDown = true;
                            cooldownTimer = 0f;
                        }
                    }
                }

                if (Input.GetButtonUp("Fire1"))
                {
                    hasCooledDown = false;
                    hasWarmedUp = false;
                    warmupTimer = 0f;
                }
            }
        }    
    }
}
