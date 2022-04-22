using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Potion_Lucky : PotionBase
{
    [Header("Potion Settings")]
    public int maxHPVariance;
    public int maxAmmoVariance;
    public float maxSpeedVariance;
    public float maxSpeedTimeVariance;
    public float maxDefenseVariance;
    public float maxDefenseTimeVariance;

    int effect;
    int healthMod;
    int ammoMod;
    float speedMod;
    float speedTimeMod;
    
    public override void Init()
    {
    }

    public override void Tick()
    {
    }

    public override void Use()
    {
        effect = Random.Range(0, 3);

        switch (effect)
        {
            case 0:
                healthMod = Random.Range(-maxHPVariance, maxHPVariance * 2);

                if (healthMod >= 0)
                {
                    PlayerStats.instance.currentHP += healthMod;
                }
                else if (healthMod < 0)
                {
                    PlayerStats.instance.DamagePlayer(-healthMod);
                }
                
                Debug.Log("Added " + healthMod + " HP");
                break;
            case 1:
                ammoMod = Random.Range(-maxAmmoVariance, maxAmmoVariance * 2);

                if (!WeaponManager.instance.currentWeapon.functionality.infiniteAmmo || WeaponManager.instance.currentWeapon.weaponId != 5)
                {
                    WeaponManager.instance.currentWeapon.AddAmmo(ammoMod);
                    WeaponManager.instance.ammoText.text = WeaponManager.instance.currentWeapon.currentAmmo + " / " + WeaponManager.instance.currentWeapon.maxAmmo;
                    Debug.Log("Adding " + ammoMod + " Ammo");
                }
                else
                {
                    effect = Random.Range(0, 2);
                    switch (effect)
                    {
                        case 0:
                            healthMod = Random.Range(-maxHPVariance, maxHPVariance * 2);

                            if (healthMod >= 0)
                            {
                                PlayerStats.instance.currentHP += healthMod;
                            }
                            else if (healthMod < 0)
                            {
                                PlayerStats.instance.DamagePlayer(-healthMod);
                            }

                            Debug.Log("Added " + healthMod + " HP");
                            break;

                        case 1:
                            speedMod = Random.Range(1f, maxSpeedVariance);
                            speedTimeMod = Random.Range(1f, maxSpeedTimeVariance);

                            PlayerMovementTest.instance.potionSpeedMultiplier = maxSpeedVariance;
                            PlayerMovementTest.instance.speedUpTime = maxSpeedTimeVariance;
                            PlayerMovementTest.instance.isSpedUp = true;
                            Debug.Log("Sped up by " + speedMod + "x for " + speedTimeMod + " seconds");
                            break;
                    }
                }
                break;
            case 2:
                speedMod = Random.Range(1f, maxSpeedVariance);
                speedTimeMod = Random.Range(1f, maxSpeedTimeVariance);

                PlayerMovementTest.instance.potionSpeedMultiplier = maxSpeedVariance;
                PlayerMovementTest.instance.speedUpTime = maxSpeedTimeVariance;
                PlayerMovementTest.instance.isSpedUp = true;
                Debug.Log("Sped up by " + speedMod + "x for " + speedTimeMod + " seconds");
                break;
        }
    }
}
