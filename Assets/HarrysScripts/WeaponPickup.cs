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
    public UIWeaponSlot[] weaponSlots;
    public KeyCode pickupKey;

    ProjectileFire fireScript;

    public TextMeshProUGUI pickupText;

    [SerializeField]bool canPickUp;
    [SerializeField]bool isInventoryFull;
    [SerializeField]bool isDuplicate;

    void Start()
    {
        weaponSlots = slotContainer.GetComponentsInChildren<UIWeaponSlot>();
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
                    pickupText.text = pickupKey.ToString() + " - Pick up " + weaponPickup.name.ToString();
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
            //CheckForMultipleWeapons(weaponSlots, weaponPickup, pickUpWeapon, pickUpPrefab);
            
            if(!isDuplicate)
            {
                AddToInventory(weaponSlots, weaponPickup, pickUpWeapon, pickUpPrefab);
            }
            else
            {
                //AddAmmoToDuplicate(weaponSlots, weaponPickup, pickUpWeapon, pickUpPrefab);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        pickUpPrefab = other.gameObject;
        pickUpWeapon = other.gameObject.GetComponent<WeaponObject>().weaponAsset;
        weaponPickup = other.gameObject.GetComponent<WeaponObject>().weaponScript;
        canPickUp = true;
    }

    private void OnTriggerExit(Collider other)
    {
        pickUpWeapon = null;
        pickUpPrefab = null;
        weaponPickup = null;
        canPickUp = false;
    }

    public bool CheckForMultipleWeapons(UIWeaponSlot[] slots, Weapon weapon, GameObject weaponPrefab, GameObject pickupPrefab)
    {
        Debug.Log("No slots: " + slots.Length);
        foreach(UIWeaponSlot slot in slots) 
        {
            Debug.Log("Weapon is null ? " + (slot.weapon == null));
        }
        
        var list = slots.Select((weapon, index) => new { Weapon = weapon, Index = index }).Where(w => w.Weapon.weapon.Equals(weapon) && w.Weapon.weapon != null);
            
        if (list.Any())
        {
            var selected = list.First();
            var index = selected.Index;

            Debug.Log("Found already existing item @ " + index);
            SetIsDuplicate(true);
            return true; 
        } 
        else
        {
            SetIsDuplicate(false);
            return false;
        }            
    }

    public bool AddAmmoToDuplicate(UIWeaponSlot[] slots, Weapon weapon, GameObject weaponPrefab, GameObject pickupPrefab)
    {
        var list = slots.Select((weapon, index) => new { Weapon = weapon, Index = index }).Where(w => w.Weapon.weapon.Equals(weapon) && w.Weapon.weapon != null);
        if (list.Any())
        {
            var selected = list.First();
            var index = selected.Index;

            slots[index].weapon.ChangeAmmo(weapon.currentAmmo);
            canPickUp = false;
            weaponPickup = null;
            Destroy(pickupPrefab);

            return true; 
        }
        else
        {
            return false;
        }  
    }

    public bool AddToInventory(UIWeaponSlot[] slots, Weapon weapon, GameObject weaponPrefab, GameObject pickupPrefab)
    {
        var list = slots.Select((weapon, index) => new { Weapon = weapon, Index = index }).Where(w => w.Weapon.weapon == null);

        if (list.Any())
        {
            var selected = list.First();
            var index = selected.Index;

            Debug.Log("Found free slot @ " + index);

            if (slots[index].weapon == null)
            {
                slots[index].weapon = weapon;
                fireScript.UpdateParameters(weapon);
                canPickUp = false;
                weaponPickup = null;
                Destroy(pickupPrefab);
            }            

            return true;
        }
        else
            SetInventoryIsFull(true);
            return false;
    }

    public void SetInventoryIsFull(bool value)
    {
        isInventoryFull = value;
    }

    public void SetIsDuplicate(bool value)
    {
        isDuplicate = value;
    }
}
