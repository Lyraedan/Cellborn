using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserControl : MonoBehaviour
{
    public ParticleSystem laserParticles;
    public ParticleSystem redLaserParticles;
    public ParticleSystem blueLaserParticles;
    public ParticleSystem yellowLaserParticles;

    public float warmup, cooldown;

    [HideInInspector] public bool isFiring;

    float warmupTimer, cooldownTimer;
    bool hasWarmedUp, hasCooledDown;

    public WeaponProperties empty;

    public AudioClip laserSound, warmupSound, cooldownSound;
    public AudioSource fireSource, warmupSource, cooldownSource;

    bool hasPlayedCooldownSound;
    bool hasPlayedWarmupSound;

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
            if (WeaponManager.instance.currentWeapon.colour == Color.red)
            {
                laserParticles = redLaserParticles;
            }
            else if (WeaponManager.instance.currentWeapon.colour == Color.blue)
            {
                laserParticles = blueLaserParticles;
            }
            else if (WeaponManager.instance.currentWeapon.colour == Color.yellow)
            {
                laserParticles = yellowLaserParticles;
            }

            Debug.Log("Agh a laser");
            if (Input.GetAxisRaw(ControlManager.INPUT_FIRE) > 0 && hasCooledDown && !PlayerStats.instance.isDead && !PauseMenu.isPaused)
            {
                hasWarmedUp = false;
                //cooldownTimer = 0f;
                warmupTimer += 1f * Time.deltaTime;
                hasPlayedCooldownSound = false;
                if (!warmupSource.isPlaying && !hasPlayedWarmupSound)
                {
                    warmupSource.Play();
                    hasPlayedWarmupSound = true;
                }

                if (!laserParticles.isPlaying && WeaponManager.instance.currentWeapon.currentAmmo > 0)
                {
                    laserParticles.Play();
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
                warmupSource.Stop();
                isFiring = false;
                hasWarmedUp = false;
                hasPlayedWarmupSound = false;

                if (!hasCooledDown)
                {        
                    cooldownTimer += 1f * Time.deltaTime;

                    if (cooldownTimer >= cooldown)
                    {
                        hasCooledDown = true;
                        laserParticles.Clear();
                        cooldownTimer = 0f;
                    }
                }
            }

            if (Input.GetButtonUp(ControlManager.INPUT_FIRE))
            {
                hasCooledDown = false;
                hasWarmedUp = false;
                warmupTimer = 0f;
                if (!hasCooledDown && !hasPlayedCooldownSound)
                {
                    cooldownSource.Play();
                    hasPlayedCooldownSound = true;
                }
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
