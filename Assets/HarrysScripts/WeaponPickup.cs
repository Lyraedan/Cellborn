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

    public TextMeshProUGUI pickupText;

    [SerializeField]bool canPickUp;
    [SerializeField]bool isInventoryFull;

    void Start()
    {
        weaponSlots = slotContainer.GetComponentsInChildren<UIWeaponSlot>();
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
            AddToInventory(weaponSlots, weaponPickup, pickUpObject);
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
                canPickUp = false;
                Destroy(prefab);
            }

            return true;
        }
        else
            InventoryIsFull();
            return false;
    }

    public void InventoryNotFull()
    {
        isInventoryFull = false;
    }

    public void InventoryIsFull()
    {
        isInventoryFull = true;
    }
}
