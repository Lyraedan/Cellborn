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
        if(Input.GetKeyDown(slotKey) && weapon != null)
        {
            EquipWeapon(weapon);
        }

        if (weapon != null)
        {
            imageSlot.sprite = weapon.icon;
            imageSlot.enabled = true;
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

    public void EquipWeapon(Weapon newWeapon)
    {
        fireScript.equippedWeapon = weapon;
        fireScript.UpdateParameters(weapon);

        imageSlot.sprite = weapon.icon;
        imageSlot.enabled = true;
        
    }

    public void ResetParameters()
    {
        fireScript.equippedWeapon = null;
        fireScript.SetParametersToNull();

        imageSlot.sprite = null;
        imageSlot.enabled = false;
    }

    public void SelectSlot()
    {
        gameObject.GetComponent<Image>().CrossFadeAlpha(1f, 0.01f, false);
    }

    public void DeselectSlot()
    {
        gameObject.GetComponent<Image>().CrossFadeAlpha(0.75f, 0.01f, false);
    }
}
