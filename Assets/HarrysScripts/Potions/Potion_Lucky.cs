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
    public int maxDefenseVariance;
    public float maxDefenseTimeVariance;

    int effect;
    int healthMod;
    int ammoMod;
    float speedMod;
    float speedTimeMod;
    int defMod;
    float defTimeMod;
    
    public override void Init()
    {
    }

    public override void Tick()
    {
    }

    public override void Use()
    {
        effect = Random.Range(0, 4);

        switch (effect)
        {
            case 0:
                healthMod = Random.Range(-maxHPVariance, maxHPVariance + 1);

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
                ammoMod = Random.Range(-maxAmmoVariance, maxAmmoVariance + 1);

                if (!WeaponManager.instance.currentWeapon.functionality.infiniteAmmo || WeaponManager.instance.currentWeapon.weaponId != 5)
                {
                    WeaponManager.instance.currentWeapon.AddAmmo(ammoMod);
                    WeaponManager.instance.ammoText.text = WeaponManager.instance.currentWeapon.currentAmmo + " / " + WeaponManager.instance.currentWeapon.maxAmmo;
                    Debug.Log("Adding " + ammoMod + " Ammo");
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
            case 3:
                defMod = Random.Range(1, maxDefenseVariance + 1);
                defTimeMod = Random.Range(1f, maxDefenseTimeVariance);

                PlayerStats.instance.defenseMultiplier = defMod;
                PlayerStats.instance.defenseTime = defTimeMod;

                Debug.Log("Defense up by " + defMod + "x for " + defTimeMod + " seconds");

                break;
        }
    }
}
