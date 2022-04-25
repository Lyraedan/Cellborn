using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponProperties : MonoBehaviour
{
    public string weaponName = "Untitled weapon";
    public int weaponId = 0;
    public string colour = "";
    public GameObject redRing;
    public GameObject yellowRing;
    public GameObject blueRing;
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

    public WeaponBase functionality;

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

        if (!functionality.isInPlayerInventory)
        {
            if (colour == "")
            {
                int colourID = Random.Range(0, 3);
                if (colourID == 0)
                {
                    colour = "Red";
                    redRing.SetActive(true);
                }
                else if (colourID == 1)
                {
                    colour = "Blue";
                    blueRing.SetActive(true);
                }
                else if (colourID == 2)
                {
                    colour = "Yellow";
                    yellowRing.SetActive(true);
                }
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

    public void Shoot(System.Action<bool> afterShot)
    {
        if(functionality == null)
        {
            Debug.Log("Weapon has no functionality!");
            return;
        }

        if (!functionality.isInPlayerInventory)
            return;

        if(functionality.infiniteAmmo)
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
