using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{

    public static WeaponManager instance;

    public GameObject player, target;

    public KeyCode pickupKey = KeyCode.E;
    public KeyCode dropKey = KeyCode.G;
    public KeyCode[] slotKeys = new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };
    public TextMeshProUGUI pickupText, weaponText, ammoText;
    public GameObject firepoint;

    public List<GameObject> possibleWeapons = new List<GameObject>();

    public List<WeaponProperties> currentlyHeldWeapons = new List<WeaponProperties>();

    public GameObject slotContainer;
    public GameObject slotPrefab;
    public List<GameObject> uiSlots = new List<GameObject>();

    public WeaponProperties _currentWeapon;
    public WeaponProperties currentWeapon
    {
        get
        {
            return _currentWeapon;
        }
        set
        {
            Debug.Log("Updating current weapon to " + value.weaponName);
            _currentWeapon = value;
            weaponText.text = "Equipped weapon: " + value.weaponName;
            ammoText.text = "Ammo: " + value.currentAmmo + "/" + value.maxAmmo;
        }
    }
    public WeaponProperties toPickup;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        target = GameObject.Find("Target");
    }

    public bool isInventoryFull {
        get {
            return !HasWeaponInInventory(-1);
        }
    }

    public void GetWeaponsInLevel()
    {
        currentlyHeldWeapons[0] = FindWeapon(0);

        for (int i = 1; i < currentlyHeldWeapons.Count; i++)
        {
            currentlyHeldWeapons[i] = FindWeapon(-1);
        }

        for (int i = 0; i < currentlyHeldWeapons.Count; i++)
        {
            var slot = Instantiate(slotPrefab, slotContainer.transform);
            var slotHolder = slot.GetComponent<SlotHolder>();
            slotHolder.number.text = $"{i + 1}";
            slotHolder.image.sprite = currentlyHeldWeapons[i].icon;
            uiSlots.Add(slot);
        }

        currentWeapon = currentlyHeldWeapons[0];
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            currentWeapon.Shoot();
            ammoText.text = "Ammo: " + currentWeapon.currentAmmo + "/" + currentWeapon.maxAmmo;
        }

        if (Input.GetKeyDown(pickupKey))
        {
            if (toPickup != null)
                Pickup(toPickup);
        }

        if (Input.GetKeyDown(dropKey))
        {
            // Is not pebble bag
            if (currentWeapon.weaponId > 0)
                Drop(currentWeapon);
        }

        for (int i = 0; i < slotKeys.Length; i++)
        {
            if (Input.GetKeyDown(slotKeys[i]))
            {
                currentWeapon = currentlyHeldWeapons[i].GetComponent<WeaponProperties>();
            }
        }
    }

    public void Pickup(WeaponProperties weapon)
    {
        WeaponProperties found = FindWeapon(weapon.weaponId);
        bool hasWeapon = HasWeaponInInventory(weapon.weaponId);

        if (!isInventoryFull && !hasWeapon)
        {
            var index = currentlyHeldWeapons.IndexOf(FindWeapon(-1));
            currentlyHeldWeapons[index] = found;

            var slot = uiSlots[index];
            var slotHolder = slot.GetComponent<SlotHolder>();
            slotHolder.image.sprite = currentlyHeldWeapons[index].icon;

            Destroy(weapon.gameObject);
            pickupText.text = string.Empty;
        }
        else if (hasWeapon)
        {
            var index = currentlyHeldWeapons.IndexOf(found);
            var wep = currentlyHeldWeapons[index];
            wep.currentAmmo += weapon.currentAmmo;
            Destroy(weapon.gameObject);

            /* TODO Figure out the maths
            if(wep.currentAmmo <= 0)
            {
                wep.currentAmmo += weapon.currentAmmo;
            }
            else
            {
                // Math is off here
                var remaining = weapon.currentAmmo - wep.currentAmmo;
                wep.currentAmmo += remaining;
                weapon.currentAmmo -= remaining;
                // Play sound here
            }

            Debug.Log("New weapon ammo: " + weapon.currentAmmo);
            if(weapon.currentAmmo <= 0)
            {
                Destroy(weapon.gameObject);
            }*/

            ammoText.text = "Ammo: " + wep.currentAmmo + "/" + wep.maxAmmo;
            Debug.Log("Adding ammo!");
        }
        else
        {
            Debug.LogError("Inventory is full!");
        }
    }

    public void Drop(WeaponProperties weapon)
    {
        WeaponProperties found = FindWeapon(weapon.weaponId);
        bool hasWeapon = HasWeaponInInventory(weapon.weaponId);
        WeaponProperties empty = FindWeapon(-1);

        var index = currentlyHeldWeapons.IndexOf(found);
        currentlyHeldWeapons[index] = empty;

        var slot = uiSlots[index];
        var slotHolder = slot.GetComponent<SlotHolder>();
        slotHolder.image.sprite = currentlyHeldWeapons[index].icon;

        currentWeapon = currentlyHeldWeapons[index];

        var drop = Instantiate(found.gameObject, firepoint.transform.position, Quaternion.identity);
        drop.SetActive(true);
    }

    WeaponProperties FindWeapon(int weaponId)
    {
        WeaponProperties result = null;
        foreach (GameObject property in possibleWeapons)
        {
            WeaponProperties prop = property.GetComponent<WeaponProperties>();
            if (prop.weaponId == weaponId)
            {
                result = prop;
                break;
            }
        }
        return result;
    }

    bool HasWeaponInInventory(int weaponId)
    {
        bool result = false;
        foreach (WeaponProperties property in currentlyHeldWeapons)
        {
            if (property.weaponId == weaponId)
            {
                result = true;
                break;
            }
        }
        return result;
    }
}
