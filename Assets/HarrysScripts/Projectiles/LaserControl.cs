using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserControl : MonoBehaviour
{
    public ParticleSystem laserParticles;

    public float warmup, cooldown;

    [HideInInspector] public bool isFiring;

    float warmupTimer, cooldownTimer;
    bool hasWarmedUp, hasCooledDown;

    void Start()
    {
        laserParticles.Stop();
    }

    void Update()
    {
        if (WeaponManager.instance.currentWeapon == null)
            return;

        var psMain = laserParticles.main;

        if (WeaponManager.instance.currentWeapon.weaponId == 5)
        {
            Debug.Log("Agh a laser");
            if (Input.GetButton("Fire1") && WeaponManager.instance.currentWeapon.currentAmmo > 0 && hasCooledDown)
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
