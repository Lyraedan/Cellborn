using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Potion_Ammo : PotionBase
{
    [Header("Potion Settings")]
    public int ammoRefill;
    
    public override void Init()
    {
    }

    public override void Tick()
    {
    }

    public override void Use()
    {
        if (!WeaponManager.instance.currentWeapon.functionality.infiniteAmmo || WeaponManager.instance.currentWeapon.weaponId != 5)
        {
            WeaponManager.instance.currentWeapon.AddAmmo(ammoRefill);
            WeaponManager.instance.ammoText.text = WeaponManager.instance.currentWeapon.currentAmmo + " / " + WeaponManager.instance.currentWeapon.maxAmmo;
        }
    }
}
