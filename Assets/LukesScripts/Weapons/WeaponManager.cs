using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{

    public static WeaponManager instance;

    public GameObject player, target;

    public KeyCode pickupKey = KeyCode.F;
    public KeyCode dropKey = KeyCode.G;
    public KeyCode[] slotKeys = new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };
    public TextMeshProUGUI pickupText, weaponText, ammoText;
    public GameObject firepoint;
    public PlayerStats healthScript;
    public int currentlySelectedIndex = 0;

    public LaserControl laserController;

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
            
            if (value.weaponId != -1)
            {
                weaponText.text = "Equipped Weapon: " + value.weaponName;
            }
            else
            {
                weaponText.text = "";
            }
            
            if (value.functionality != null)
            {
                if (value.functionality.infiniteAmmo)
                {
                    ammoText.text = string.Empty;
                }
                else
                {
                    ammoText.text = "Ammo: " + value.currentAmmo + " / " + value.maxAmmo;
                }
            }
            else
            {
                ammoText.text = "";
            }
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
        for (int i = 0; i < currentlyHeldWeapons.Count; i++)
        {
            currentlyHeldWeapons[i] = FindWeapon(-1);
        }
        currentlyHeldWeapons[2] = FindWeapon(0); // Pebbles

        currentlyHeldWeapons[1] = FindWeapon(4);
        currentlyHeldWeapons[0] = FindWeapon(2);

        for (int i = 0; i < currentlyHeldWeapons.Count; i++)
        {
            var slot = Instantiate(slotPrefab, slotContainer.transform);
            var slotHolder = slot.GetComponent<SlotHolder>();
            slotHolder.number.text = $"{i + 1}";
            slotHolder.image.sprite = currentlyHeldWeapons[i].icon;
            slot.GetComponent<SlotHolder>().DeselectSlot();
            uiSlots.Add(slot);
        }

        uiSlots[2].GetComponent<SlotHolder>().SelectSlot();
        currentWeapon = currentlyHeldWeapons[2];
    }

    private void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            if (!healthScript.isDead)
            {
                currentWeapon.Shoot(delayed => {
                    if (!currentWeapon.functionality.infiniteAmmo)
                    {
                        ammoText.text = "Ammo: " + currentWeapon.currentAmmo + " / " + currentWeapon.maxAmmo;

                        if (currentWeapon.currentAmmo < 1 && currentWeapon.weaponId != 4) // no ammo and NOT crossbow
                        {
                            var empty = FindWeapon(-1);
                            currentlyHeldWeapons[currentlySelectedIndex] = empty;

                            var slot = uiSlots[currentlySelectedIndex];
                            var slotHolder = slot.GetComponent<SlotHolder>();
                            slotHolder.image.sprite = currentlyHeldWeapons[currentlySelectedIndex].icon;
                            weaponText.text = string.Empty;

                            currentWeapon = currentlyHeldWeapons[currentlySelectedIndex];
                        }
                    }
                });
            }
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
                uiSlots[currentlySelectedIndex].GetComponent<SlotHolder>().DeselectSlot();
                currentWeapon = currentlyHeldWeapons[i];
                currentlySelectedIndex = i;
                uiSlots[currentlySelectedIndex].GetComponent<SlotHolder>().SelectSlot();
            }
        }

        var scrollDelta = Input.mouseScrollDelta;
        if (scrollDelta != Vector2.zero)
        {
            uiSlots[currentlySelectedIndex].GetComponent<SlotHolder>().DeselectSlot();
            
            if(scrollDelta.y < 0)
            {
                currentlySelectedIndex++;
                if (currentlySelectedIndex >= currentlyHeldWeapons.Count)
                    currentlySelectedIndex = 0;

                uiSlots[currentlySelectedIndex].GetComponent<SlotHolder>().SelectSlot();

            } else if(scrollDelta.y > 0)
            {
                currentlySelectedIndex--;
                if (currentlySelectedIndex < 0)
                    currentlySelectedIndex = currentlyHeldWeapons.Count - 1;

                uiSlots[currentlySelectedIndex].GetComponent<SlotHolder>().SelectSlot();
            }
            currentWeapon = currentlyHeldWeapons[currentlySelectedIndex].GetComponent<WeaponProperties>();
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
            var wep = currentlyHeldWeapons[index];

            var slot = uiSlots[index];
            var slotHolder = slot.GetComponent<SlotHolder>();
            slotHolder.image.sprite = currentlyHeldWeapons[index].icon;

            wep.SetAmmo(weapon.currentAmmo);

            Destroy(weapon.gameObject);
            pickupText.text = string.Empty;
        }
        else if (hasWeapon)
        {
            var index = currentlyHeldWeapons.IndexOf(found);
            var wep = currentlyHeldWeapons[index];

            if (!wep.IsFull)
            {
                Debug.Log("Adding ammo!");
                int remaining = wep.AddAmmo(weapon.currentAmmo);
                Debug.Log("Added -> " + weapon.currentAmmo + " has " + remaining + " left");
                weapon.SetAmmo(remaining);

                if (weapon.IsEmpty)
                    Destroy(weapon.gameObject);
            } else
            {
                Debug.Log("Ammo is full!");
            }

            ammoText.text = "Ammo: " + wep.currentAmmo + " / " + wep.maxAmmo;
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

        GameObject drop = Instantiate(found.gameObject, firepoint.transform.position, Quaternion.identity);
        // Turn everything back on
        drop.GetComponent<BoxCollider>().enabled = true;
        drop.GetComponent<SphereCollider>().enabled = true;
        drop.GetComponent<Rigidbody>().useGravity = true;
        drop.GetComponent<WeaponProperties>().functionality.isInPlayerInventory = false;
        drop.transform.GetChild(0).gameObject.SetActive(true);
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

    public bool HasWeaponInInventory(int weaponId)
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
