using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponProperties : MonoBehaviour
{
    public string weaponName = "Untitled weapon";
    public int weaponId = 0;
    public int currentAmmo = 0;
    public int maxAmmo = 30;
    public Sprite icon;

    public WeaponBase functionality;

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

}
