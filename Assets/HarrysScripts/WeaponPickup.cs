using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class WeaponPickup : MonoBehaviour
{
    public GameObject slotContainer, pickUpObject;
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
        fireScript = FindObjectOfType<ProjectileFire>();
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
                pickupText.enabled = true;
                pickupText.text = pickupKey.ToString() + " - Pick up " + weaponPickup.name.ToString();
            }
        }
        else
        {
            pickupText.enabled = false;
            pickupText.text = "";
        }
        
        if (Input.GetKeyDown(pickupKey) && canPickUp && !isInventoryFull)
        {
            CheckForMultipleWeapons(weaponSlots, weaponPickup);
            
            if(!isDuplicate)
            {
                AddToInventory(weaponSlots, weaponPickup, pickUpObject);
            }
            else
            {
                AddAmmoToDuplicate(weaponSlots, weaponPickup, pickUpObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        pickUpObject = other.gameObject;
        weaponPickup = pickUpObject.GetComponent<WeaponObject>().weapon;
        canPickUp = true;
    }

    private void OnTriggerExit(Collider other)
    {
        pickUpObject = null;
        weaponPickup = null;
        canPickUp = false;
    }

    public bool CheckForMultipleWeapons(UIWeaponSlot[] slots, Weapon weapon)
    {
        var listA = slots.Select((weapon, index) => new { Weapon = weapon, Index = index }).Where(w => w.Weapon.weapon != null);
        if (listA.Any())
        {
            var listB = slots.Select((weapon, index) => new { Weapon = weapon, Index = index }).Where(w => w.Weapon.weapon.name == weapon.name);
            
            if (listB.Any())
            {
                var selected = listB.First();
                var index = selected.Index;

                Debug.Log("Found already existing item @ " + index);
                SetIsDuplicate(true);
                return true; 
            }
            else
            {
                return false;
            }            
        }
        else
        {
            SetIsDuplicate(false);
            return false;
        }            
    }

    public bool AddAmmoToDuplicate(UIWeaponSlot[] slots, Weapon weapon, GameObject prefab)
    {
        var list = slots.Select((weapon, index) => new { Weapon = weapon, Index = index }).Where(w => w.Weapon.weapon.name == weapon.name);
        if (list.Any())
        {
            var selected = list.First();
            var index = selected.Index;

            slots[index].weapon.ChangeAmmo(prefab.GetComponent<WeaponObject>().currentAmmo);
            canPickUp = false;
            Destroy(prefab);

            return true; 
        }
        else
        {
            return false;
        }  
    }

    public bool AddToInventory(UIWeaponSlot[] slots, Weapon weapon, GameObject prefab)
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
                fireScript.UpdateParameters(prefab.GetComponent<WeaponObject>().weapon);
                canPickUp = false;
                Destroy(prefab);
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
