using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponProperties : MonoBehaviour
{
    public string weaponName = "Untitled weapon";
    public int weaponId = 0;
    public int currentAmmo = 30;
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

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            WeaponManager.instance.toPickup = this;
            WeaponManager.instance.pickupText.text = $"{WeaponManager.instance.pickupKey.ToString()} - Pick Up {weaponName}";
        }        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            WeaponManager.instance.toPickup = null;
            WeaponManager.instance.pickupText.text = string.Empty;
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
        return remainder;
    }

    public void RemoveAmmo(int amt)
    {
        int removed = currentAmmo - amt;
        if (removed <= 0) {
            removed = 0;
        }
        currentAmmo = removed;
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
    }

}
