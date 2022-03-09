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

    public float fireRate = 0;

    public WeaponBase functionality;

    public void Shoot()
    {
        if(functionality == null)
        {
            Debug.Log("Weapon has no functionality!");
            return;
        }

        if(functionality.infiniteAmmo)
            functionality.Fire();
        else
        {
            if (currentAmmo > 0)
            {
                currentAmmo--;
                functionality.Fire();
            } else
            {
                Debug.LogError("No ammo!");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            WeaponManager.instance.toPickup = this;
            WeaponManager.instance.pickupText.text = $"Pick up {weaponName}";
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
