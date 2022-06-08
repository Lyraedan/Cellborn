using Bolt;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using EventHooks = LukesScripts.Blueprints.EventHooks;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{

    public static WeaponManager instance;

    public GameObject player, target;
    public TargetProperties targetProperties;

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

    public Animator animController;

    private WeaponProperties _currentWeapon;
    public WeaponProperties currentWeapon
    {
        get
        {
            return _currentWeapon;
        }
        set
        {
            Debug.Log("Updating current weapon to " + value.weaponName);

            if (currentViewModel != null)
            {
                currentViewModel.SetActive(false);
                // Reset the view model animator
                currentViewModelAnimator = null;
            }

            currentViewModel = value.viewModel;

            if (currentViewModel != null)
            {
                currentViewModel.SetActive(true);
                // If the view model has an animator
                if (currentViewModel.GetComponent<Animator>())
                {
                    currentViewModelAnimator = currentViewModel.GetComponent<Animator>();
                }
            }
            _currentWeapon = value;

            if (value.weaponId != -1)
            {
                weaponText.text = value.weaponName;
            }
            else
            {
                weaponText.text = "";
            }

            animController.SetLayerWeight(animController.GetLayerIndex("Bouncy Ball Gun"), 0);
            animController.SetLayerWeight(animController.GetLayerIndex("BHG"), 0);
            animController.SetLayerWeight(animController.GetLayerIndex("Boom Box Gun"), 0);
            animController.SetLayerWeight(animController.GetLayerIndex("Boomer"), 0);
            animController.SetLayerWeight(animController.GetLayerIndex("Bren"), 0);
            animController.SetLayerWeight(animController.GetLayerIndex("Crossbow"), 0);
            animController.SetLayerWeight(animController.GetLayerIndex("Deagle"), 0);
            animController.SetLayerWeight(animController.GetLayerIndex("Dualzi"), 0);
            animController.SetLayerWeight(animController.GetLayerIndex("G11"), 0);
            animController.SetLayerWeight(animController.GetLayerIndex("LBR"), 0);
            animController.SetLayerWeight(animController.GetLayerIndex("Muffin Sniper"), 0);
            animController.SetLayerWeight(animController.GetLayerIndex("Triple Barrel Shotgun"), 0);

            switch(value.weaponId)
            {
                case 1:
                    animController.SetLayerWeight(animController.GetLayerIndex("Bouncy Ball Gun"), 1);
                    break;
                case 2:
                    animController.SetLayerWeight(animController.GetLayerIndex("BHG"), 1);
                    break;
                case 3:
                    animController.SetLayerWeight(animController.GetLayerIndex("Muffin Sniper"), 1);
                    break;
                case 4:
                    animController.SetLayerWeight(animController.GetLayerIndex("Crossbow"), 1);
                    break;
                case 5:
                    animController.SetLayerWeight(animController.GetLayerIndex("LBR"), 1);
                    break;
                case 6:
                    animController.SetLayerWeight(animController.GetLayerIndex("Triple Barrel Shotgun"), 1);
                    break;
                case 7:
                    animController.SetLayerWeight(animController.GetLayerIndex("Dualzi"), 1);
                    break;
                case 8:
                    animController.SetLayerWeight(animController.GetLayerIndex("Boomer"), 1);
                    break;
                case 9:
                    animController.SetLayerWeight(animController.GetLayerIndex("Bren"), 1);
                    break;
                case 10:
                    animController.SetLayerWeight(animController.GetLayerIndex("G11"), 1);
                    break;
                case 11:
                    animController.SetLayerWeight(animController.GetLayerIndex("Deagle"), 1);
                    break;
                case 12:
                    animController.SetLayerWeight(animController.GetLayerIndex("Boom Box Gun"), 1);
                    break;
                default:
                    Debug.Log("No anim layer index to update");
                    break;
            }

            if (value.functionality != null)
            {
                if (value.functionality.infiniteAmmo)
                {
                    ammoText.text = string.Empty;
                    UIController.instance.ammoContainer.SetActive(false);
                }
                else
                {
                    ammoText.text = value.currentAmmo + " / " + value.maxAmmo;
                    UIController.instance.ammoContainer.SetActive(value.currentAmmo > 0);
                    UIController.instance.ammoBar.maxValue = value.maxAmmo;
                    UIController.instance.ammoBar.value = value.currentAmmo;
                }

                ParticleSystem.MainModule innerRingMain = WeaponManager.instance.targetProperties.inner.main;
                ParticleSystem.MainModule ringMain = WeaponManager.instance.targetProperties.outer.main;

                if (_currentWeapon.colour == Color.red)
                {
                    innerRingMain.startColor = Color.red;
                    ringMain.startColor = Color.red;
                }
                else if (_currentWeapon.GetComponent<WeaponProperties>().colour == Color.blue)
                {
                    innerRingMain.startColor = Color.blue;
                    ringMain.startColor = Color.blue;
                }
                else if (_currentWeapon.GetComponent<WeaponProperties>().colour == Color.yellow)
                {
                    innerRingMain.startColor = Color.yellow;
                    ringMain.startColor = Color.yellow;
                }
                else
                {
                    innerRingMain.startColor = Color.white;
                    ringMain.startColor = Color.white;
                }

                animController.SetBool("IsHoldingWeapon", true);
            }
            else
            {
                ammoText.text = "";
                UIController.instance.ammoContainer.SetActive(false);
                animController.SetBool("IsHoldingWeapon", false);

            }
        }
    }
    public WeaponProperties toPickup;

    public GameObject currentViewModel;
    public Animator currentViewModelAnimator;

    public AudioSource source;
    public AudioClip pickupSound, pickupAmmoSound;

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
        targetProperties = target.GetComponent<TargetProperties>();
    }

    public bool isInventoryFull
    {
        get
        {
            return !HasWeaponInInventory(-1);
        }
    }

    public void OnPlayerDeath()
    {
        currentWeapon = currentlyHeldWeapons[2];
    }

    public void GetWeaponsInLevel()
    {
        for (int i = 0; i < currentlyHeldWeapons.Count; i++)
        {
            currentlyHeldWeapons[i] = FindWeapon(-1);
        }
        currentlyHeldWeapons[2] = FindWeapon(0); // Pebbles 
        currentlyHeldWeapons[0] = FindWeapon(7); // Pebbles 
        //currentlyHeldWeapons[0] = FindWeapon(6);
        //currentlyHeldWeapons[1] = FindWeapon(2);

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
        if (!PauseMenu.isPaused)
        {
            if (PlayerStats.instance.isDead)
                return;

            if (Input.GetButton(ControlManager.INPUT_FIRE))
            {
                if (RoomGenerator.instance.cutscenePlaying)
                    return;

                if (!healthScript.isDead)
                {
                    currentWeapon.Shoot(delayed =>
                    {
                        if (!currentWeapon.functionality.infiniteAmmo)
                        {
                            ammoText.text = currentWeapon.currentAmmo + " / " + currentWeapon.maxAmmo;

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
        }

        if (Input.GetButtonDown(ControlManager.INPUT_PICKUP))
        {
            if (toPickup != null)
                Pickup(toPickup);
        }

        if (Input.GetAxisRaw("Drop") > 0)
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
        if (scrollDelta != Vector2.zero && !PauseMenu.isPaused)
        {
            uiSlots[currentlySelectedIndex].GetComponent<SlotHolder>().DeselectSlot();

            if (scrollDelta.y < 0)
            {
                currentlySelectedIndex++;
                if (currentlySelectedIndex >= currentlyHeldWeapons.Count)
                    currentlySelectedIndex = 0;

                uiSlots[currentlySelectedIndex].GetComponent<SlotHolder>().SelectSlot();

            }
            else if (scrollDelta.y > 0)
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
        found.colour = weapon.colour;

        //Turn on Ring
        if (weapon.GetComponent<WeaponProperties>().colour == Color.red)
        {
            found.GetComponent<WeaponProperties>().icon = weapon.GetComponent<WeaponProperties>().redIcon;
        }
        else if (weapon.GetComponent<WeaponProperties>().colour == Color.blue)
        {
            found.GetComponent<WeaponProperties>().icon = weapon.GetComponent<WeaponProperties>().blueIcon;
        }
        else if (weapon.GetComponent<WeaponProperties>().colour == Color.yellow)
        {
            found.GetComponent<WeaponProperties>().icon = weapon.GetComponent<WeaponProperties>().yellowIcon;
        }

        if (!isInventoryFull && !hasWeapon)
        {
            var index = currentlyHeldWeapons.IndexOf(FindWeapon(-1));
            currentlyHeldWeapons[index] = found;
            var wep = currentlyHeldWeapons[index];

            var slot = uiSlots[index];
            var slotHolder = slot.GetComponent<SlotHolder>();
            slotHolder.image.sprite = currentlyHeldWeapons[index].icon;

            wep.SetAmmo(weapon.currentAmmo);
            CustomEvent.Trigger(gameObject, EventHooks.OnWeaponGrabbed);

            Destroy(weapon.gameObject);
            pickupText.text = string.Empty;

            // Update current weapon
            if (index == currentlySelectedIndex)
                currentWeapon = wep;

            source.clip = pickupSound;
            source.Play();
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
                CustomEvent.Trigger(gameObject, EventHooks.OnAmmoReplenished, remaining, weapon.IsEmpty);
                weapon.SetAmmo(remaining);

                source.clip = pickupAmmoSound;
                source.Play();

                if (weapon.IsEmpty)
                    Destroy(weapon.gameObject);
            }
            else
            {
                Debug.Log("Ammo is full!");
                CustomEvent.Trigger(gameObject, EventHooks.OnAmmoFull);
            }

            ammoText.text = wep.currentAmmo + " / " + wep.maxAmmo;
        }
        else
        {
            // Swap weapon
            if (currentWeapon.weaponId != 0)
            {
                Drop(currentWeapon);
                Pickup(weapon);
                Debug.LogError("Inventory is full!");
                CustomEvent.Trigger(gameObject, EventHooks.InventoryFull);
            }
        }
    }

    public void Drop(WeaponProperties weapon)
    {
        WeaponProperties found = FindWeapon(weapon.weaponId);
        found.colour = weapon.colour;
        bool hasWeapon = HasWeaponInInventory(weapon.weaponId);
        WeaponProperties empty = FindWeapon(-1);

        var index = currentlyHeldWeapons.IndexOf(found);
        currentlyHeldWeapons[index] = empty;

        var slot = uiSlots[index];
        var slotHolder = slot.GetComponent<SlotHolder>();
        slotHolder.image.sprite = currentlyHeldWeapons[index].icon;

        currentWeapon = currentlyHeldWeapons[index];

        GameObject drop = Instantiate(found.gameObject, firepoint.transform.position, Quaternion.identity);
        drop.tag = "Weapon";

        //Turn on Ring
        if (drop.GetComponent<WeaponProperties>().colour == Color.red)
        {
            drop.GetComponent<WeaponProperties>().redRing.SetActive(true);
        }
        else if (drop.GetComponent<WeaponProperties>().colour == Color.blue)
        {
            drop.GetComponent<WeaponProperties>().blueRing.SetActive(true);
        }
        else if (drop.GetComponent<WeaponProperties>().colour == Color.yellow)
        {
            drop.GetComponent<WeaponProperties>().yellowRing.SetActive(true);
        }

        //Turn on Everything Else
        drop.GetComponent<BoxCollider>().enabled = true;
        drop.GetComponent<SphereCollider>().enabled = true;
        drop.GetComponent<Rigidbody>().useGravity = true;
        drop.GetComponent<WeaponProperties>().functionality.isInPlayerInventory = false;
        drop.transform.GetChild(0).gameObject.SetActive(true);
        drop.SetActive(true);
        CustomEvent.Trigger(gameObject, EventHooks.OnWeaponDropped);
    }

    public WeaponProperties FindWeapon(int weaponId)
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
