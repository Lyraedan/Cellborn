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

    public WeaponProperties empty;

    public AudioClip laserSound, warmupSound, cooldownSound;
    public AudioSource fireSource, warmupSource, cooldownSource;

    void Start()
    {
        laserParticles.Stop();

        fireSource.clip = laserSound;
        warmupSource.clip = warmupSound;
        cooldownSource.clip = cooldownSound;

        fireSource.Stop();
        warmupSource.Stop();
        cooldownSource.Stop();
    }

    void Update()
    {
        if (WeaponManager.instance.currentWeapon == null)
            return;

        var psMain = laserParticles.main;

        if (WeaponManager.instance.currentWeapon.weaponId == 5)
        {
            Debug.Log("Agh a laser");
            if (Input.GetButton("Fire1") && hasCooledDown && !PlayerStats.instance.isDead && !PauseMenu.isPaused)
            {
                hasWarmedUp = false;
                cooldownTimer = 0f;
                warmupTimer += 1f * Time.deltaTime;

                if (!laserParticles.isPlaying && WeaponManager.instance.currentWeapon.currentAmmo > 0)
                {
                    laserParticles.Play();
                    if (!warmupSource.isPlaying)
                    {
                        warmupSource.Play();
                    }
                }
                
                if (WeaponManager.instance.currentWeapon.currentAmmo <= 1)
                {
                    EmptyLaser();                   
                }

                if (warmupTimer >= warmup && !hasWarmedUp)
                {
                    hasWarmedUp = true;

                }

                if (hasWarmedUp)
                {
                    //psMain.simulationSpace = ParticleSystemSimulationSpace.Local;
                    if (!fireSource.isPlaying)
                    {
                        fireSource.Play();
                    }
                    isFiring = true;                  
                }
            }
            else
            {
                laserParticles.Stop();
                fireSource.Stop();
                isFiring = false;
                hasWarmedUp = false;

                if (!hasCooledDown)
                {
                    if (!cooldownSource.isPlaying)
                    {
                        cooldownSource.Play();
                    }
                    
                    cooldownTimer += 1f * Time.deltaTime;

                    if (cooldownTimer >= cooldown)
                    {
                        hasCooledDown = true;
                        laserParticles.Clear();
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

    void EmptyLaser()
    {
        laserParticles.Stop();

        var manInst = WeaponManager.instance;
        var currentSlot = manInst.uiSlots[manInst.currentlySelectedIndex].GetComponent<SlotHolder>();

        manInst.currentWeapon.SetAmmo(0);
        currentSlot.image.sprite = empty.icon;
        manInst.currentlyHeldWeapons[manInst.currentlySelectedIndex] = empty;
        isFiring = false;
        hasWarmedUp = false;
        warmupTimer = 0;
        hasCooledDown = true; 

        fireSource.Stop();
        if (!cooldownSource.isPlaying)
        {
            cooldownSource.Play();
        }
    }
}
