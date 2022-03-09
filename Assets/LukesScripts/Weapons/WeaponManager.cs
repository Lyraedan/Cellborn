using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{

    public static WeaponManager instance;

    public KeyCode pickupKey = KeyCode.E;
    public KeyCode dropKey = KeyCode.G;
    public KeyCode[] slotKeys = new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };
    public TextMeshProUGUI pickupText, weaponText, ammoText;
    public Transform firepoint;

    public List<GameObject> possibleWeapons = new List<GameObject>();

    public List<WeaponProperties> currentlyHeldWeapons = new List<WeaponProperties>();

    public GameObject slotContainer;
    public GameObject slotPrefab;
    public List<GameObject> uiSlots = new List<GameObject>();

    public WeaponProperties _currentWeapon;
    public WeaponProperties currentWeapon { 
        get {
            return _currentWeapon;
        } 
        set {
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

    public bool isInventoryFull { private get; set; }
    public bool isDuplicate { private get; set; }

    public void GetWeaponsInLevel()
    {
        currentlyHeldWeapons[0] = possibleWeapons.Find(w => w.gameObject.GetComponent<WeaponProperties>().weaponId == 0).GetComponent<WeaponProperties>();

        for (int i = 1; i < currentlyHeldWeapons.Count; i++)
        {
            currentlyHeldWeapons[i] = possibleWeapons.Find(w => w.gameObject.GetComponent<WeaponProperties>().weaponId == -1).GetComponent<WeaponProperties>();
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
            Debug.Log("Shooting");
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

    /// <summary>
    /// Todo needs work
    /// </summary>
    /// <param name="weapon"></param>
    public void Pickup(WeaponProperties weapon)
    {
        var found = possibleWeapons.Find(w => w.gameObject.GetComponent<WeaponProperties>().weaponId == weapon.weaponId).GetComponent<WeaponProperties>();

        try
        {
            var firstEmpty = currentlyHeldWeapons.First(w => w.gameObject.GetComponent<WeaponProperties>().weaponId == -1);
            isInventoryFull = firstEmpty == null;

            var check = currentlyHeldWeapons.Find(w => w.weaponId == weapon.weaponId);
            isDuplicate = check != null;

            WeaponProperties firstFound = null;
            try
            {
                firstFound = currentlyHeldWeapons.First(w => w.gameObject.GetComponent<WeaponProperties>().weaponId == weapon.weaponId);
            } catch(Exception e1)
            {
                Debug.LogError("Weapon not found");
            }

                if (!isInventoryFull && !isDuplicate)
            {
                var index = currentlyHeldWeapons.IndexOf(firstEmpty);
                currentlyHeldWeapons[index] = found;

                var slot = uiSlots[index];
                var slotHolder = slot.GetComponent<SlotHolder>();
                slotHolder.image.sprite = currentlyHeldWeapons[index].icon;

                Destroy(weapon.gameObject);
                pickupText.text = string.Empty;
            }
            else if (isDuplicate)
            {
                var index = currentlyHeldWeapons.IndexOf(firstFound);
                currentlyHeldWeapons[index].currentAmmo += weapon.currentAmmo;
                Debug.Log("Adding ammo!");
                Destroy(weapon.gameObject);
            }
            else
            {
                Debug.LogError("Inventory is full!");
            }
        } catch(Exception e)
        {
            Debug.LogError("Inventory is full!");
        }
    }

    /// <summary>
    /// Todo rotation and position is scuffed - Affects player rotation?
    /// </summary>
    /// <param name="weapon"></param>
    public void Drop(WeaponProperties weapon)
    {
        var found = possibleWeapons.Find(w => w.gameObject.GetComponent<WeaponProperties>().weaponId == weapon.weaponId).GetComponent<WeaponProperties>();
        var empty = possibleWeapons.Find(w => w.gameObject.GetComponent<WeaponProperties>().weaponId == -1).GetComponent<WeaponProperties>();
        var index = currentlyHeldWeapons.IndexOf(found);
        currentlyHeldWeapons[index] = empty;

        var slot = uiSlots[index];
        var slotHolder = slot.GetComponent<SlotHolder>();
        slotHolder.image.sprite = currentlyHeldWeapons[index].icon;

        currentWeapon = currentlyHeldWeapons[index];

        var drop = Instantiate(found.gameObject, firepoint.position, Quaternion.identity);
        drop.SetActive(true);
    }
}
