using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class WeaponPickup : MonoBehaviour
{
    public GameObject slotContainer, pickUpWeapon, pickUpPrefab;
    public Weapon weaponPickup;
    public List<UIWeaponSlot> weaponSlots;
    public KeyCode pickupKey;

    ProjectileFire fireScript;

    public TextMeshProUGUI pickupText;

    public bool canPickUp { private get; set; }
    public bool isInventoryFull { private get; set; }
    public bool isDuplicate { private get; set; }

    void Start()
    {
        weaponSlots = slotContainer.GetComponentsInChildren<UIWeaponSlot>().ToList();
        fireScript = gameObject.GetComponentInChildren<ProjectileFire>();
    }

    void Update()
    {
        if (canPickUp)
        {
            if (isInventoryFull)
            {
                pickupText.enabled = true;
                pickupText.text = "Inventory Full!";
            }
            else
            {
                if (weaponPickup != null)
                {
                    pickupText.enabled = true;
                    pickupText.text = pickupKey.ToString() + " - Pick up " + weaponPickup.name;
                }
            }
        }
        else
        {
            pickupText.enabled = false;
            pickupText.text = "";
        }
        
        if (Input.GetKeyDown(pickupKey) && canPickUp && !isInventoryFull)
        {
            bool hasDuplicates = CheckForMultipleWeapons(weaponPickup, pickUpWeapon, pickUpPrefab);
            
            if(!hasDuplicates)
            {
                AddToInventory(weaponPickup, pickUpWeapon, pickUpPrefab);
            }
            else
            {
                AddAmmoToDuplicate(weaponPickup, pickUpWeapon, pickUpPrefab);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            pickUpPrefab = other.gameObject;
            
            if (other.GetComponent<WeaponObject>())
            {
                pickUpWeapon = other.gameObject.GetComponent<WeaponObject>().weaponAsset;
                weaponPickup = other.gameObject.GetComponent<WeaponObject>().weaponScript;
            }

            canPickUp = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        pickUpWeapon = null;
        pickUpPrefab = null;
        weaponPickup = null;
        canPickUp = false;
    }

    public bool CheckForMultipleWeapons(Weapon weapon, GameObject weaponPrefab, GameObject pickupPrefab)
    {
        bool found = false;
        foreach(UIWeaponSlot slot in weaponSlots)
        {
            if(slot.weapon != null)
            {
                if(slot.weapon.name.Equals(weapon.name))
                {
                    slot.weapon.ChangeAmmo(weapon.currentAmmo);
                    found = true;
                    break;
                }
            }
        }
        Debug.Log("Found multiple: " + found);
        return found;
    }

    public bool AddAmmoToDuplicate(Weapon weapon, GameObject weaponPrefab, GameObject pickupPrefab)
    {
        bool found = false;
        foreach(UIWeaponSlot slot in weaponSlots)
        {
            if(slot.weapon != null)
            {
                Debug.Log("Adding " + weapon.currentAmmo + " to your current weapon");
                slot.weapon.ChangeAmmo(weapon.currentAmmo);
                fireScript.UpdateParameters(slot.weapon);
                canPickUp = false;
                weaponPickup = null;
                Destroy(pickupPrefab);
                found = true;
                break;
            }
        }
        Debug.Log("Adding ammo to " + weapon.gameObject.name + ": " + found);
        return found;
    }

    public bool AddToInventory(Weapon weapon, GameObject weaponPrefab, GameObject pickupPrefab)
    {
        bool found = false;
        foreach(UIWeaponSlot slot in weaponSlots)
        {
            if(slot.weapon == null)
            {
                slot.weapon = weapon;
                fireScript.UpdateParameters(weapon);
                canPickUp = false;
                weaponPickup = null;
                Destroy(pickupPrefab);
                found = true;
                break;
            }
        }
        Debug.Log("Found weapon " + weapon.gameObject.name + ": " + found);
        return found;
    }
}
