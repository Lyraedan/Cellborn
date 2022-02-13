using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWeaponSlot : MonoBehaviour
{
    public Weapon weapon;
    public Image imageSlot;
    public KeyCode slotKey;

    public ProjectileFire fireScript;

    void Update()
    {
        imageSlot.sprite = weapon.icon;
        imageSlot.enabled = true;

        if(Input.GetKeyDown(slotKey))
        {
            fireScript.equippedWeapon = weapon;
            fireScript.UpdateParameters(weapon);
        }
    }

    void AddWeapon(Weapon newWeapon)
    {
        weapon = newWeapon;

        imageSlot.sprite = weapon.icon;
        imageSlot.enabled = true;
    }

    void RemoveWeapon()
    {
        weapon = null;

        imageSlot.sprite = null;
        imageSlot.enabled = false;
    }
}
