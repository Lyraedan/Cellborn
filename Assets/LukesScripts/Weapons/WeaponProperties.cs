using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WeaponProperties : MonoBehaviour
{
    public string weaponName = "Untitled weapon";
    public int weaponId = 0;
    public Color colour = Color.white;
    public GameObject redRing;
    public GameObject yellowRing;
    public GameObject blueRing;
    public GameObject greyRing;
    [SerializeField] private int _currentAmmo = 30;
    public int currentAmmo {
        get {
            return _currentAmmo;
        }
        set
        {
            if (_currentAmmo != value)
            {
                UIController.instance.ammoBar.maxValue = maxAmmo;
                UIController.instance.ammoBar.value = currentAmmo;
                _currentAmmo = value;
            }
        }
    }
    public int maxAmmo = 30;
    public Sprite icon;
    public Sprite redIcon;
    public Sprite blueIcon;
    public Sprite yellowIcon;
    [Space(10)]
    public GameObject viewModel;

    public WeaponBase functionality;
    public Animator animController;
    int colourID = 99;

    public bool IsFull
    {
        get
        {
            return currentAmmo >= maxAmmo;
        }
    }

    public bool IsEmpty
    {
        get
        {
            return currentAmmo <= 0;
        }
    }

    public float DistanceFromPlayer
    {
        get
        {
            if (WeaponManager.instance == null)
                return Mathf.Infinity;

            return Vector3.Distance(transform.position, WeaponManager.instance.player.transform.position);
        }
    }

    private void Start()
    {
        animController = WeaponManager.instance.player.GetComponent<Animator>();
        if (!functionality.isInPlayerInventory)
        {
            try
            {
                if (colour == Color.white)
                {
                    colourID = Random.Range(0, 3);
                    if (colourID == 0)
                    {
                        colour = Color.red;
                        redRing.SetActive(true);
                        icon = redIcon;
                    }
                    else if (colourID == 1)
                    {
                        colour = Color.blue;
                        blueRing.SetActive(true);
                        icon = blueIcon;
                    }
                    else if (colourID == 2)
                    {
                        colour = Color.yellow;
                        yellowRing.SetActive(true);
                        icon = yellowIcon;
                    }
                }
                else if (colour != Color.red && colour != Color.blue && colour != Color.yellow)
                {
                    greyRing.SetActive(true);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception triggered by {gameObject.name} | {e.Message} : {e.StackTrace}");
            }
            
        }

        if (functionality != null)
        {
            if(!functionality.isInPlayerInventory)
                PlayerStats.instance.weaponsInScene.Add(this);
        }
    }

    private void OnDestroy()
    {
        if (functionality != null)
        {
            if (!functionality.isInPlayerInventory)
                PlayerStats.instance.weaponsInScene.Remove(this);
        }
    }

    public void Update()
    {
        animController.SetBool("IsShooting", false);
    }

    public void Shoot(System.Action<bool> afterShot)
    {
        if (functionality == null)
        {
            Debug.Log("Weapon has no functionality!");
            return;
        }

        if (!functionality.isInPlayerInventory)
            return;

        animController.SetBool("IsShooting", true);

        if (functionality.infiniteAmmo)
            functionality.Shoot(afterShot);
        else
        {
            if (currentAmmo > 0)
            {
                functionality.Shoot(delayed => {
                    if(delayed)
                    {
                        if(weaponId != 5)
                        {
                            currentAmmo--;
                        }
                    }
                    afterShot?.Invoke(delayed);
                });
            } else
            {
                //Debug.LogError("No ammo!");
            }
        }
    }

    public int AddAmmo(int amt)
    {
        int added = currentAmmo + amt;
        int remainder = 0;
        if(added >= maxAmmo)
        {
            remainder = added - maxAmmo;
            added = maxAmmo;
        }
        currentAmmo = added;
        UIController.instance.ammoBar.value = added;
        return remainder;
    }

    public void RemoveAmmo(int amt)
    {
        int removed = currentAmmo - amt;
        if (removed <= 0) {
            removed = 0;
        }
        currentAmmo = removed;
        UIController.instance.ammoBar.value = removed;
    }

    public void SetAmmo(int amt)
    {
        var newAmmo = amt;
        if(newAmmo >= maxAmmo)
        {
            newAmmo = maxAmmo;
        } else if(newAmmo <= 0)
        {
            newAmmo = 0;
        }
        currentAmmo = newAmmo;
        UIController.instance.ammoBar.value = newAmmo;
    }

}
