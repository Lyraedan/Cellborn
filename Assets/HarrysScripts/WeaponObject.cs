using UnityEngine;

public class WeaponObject : MonoBehaviour
{
    public Weapon weaponAsset;
    public Weapon weapon;   
    public ProjectileFire fireScript; 

    public int currentAmmo;

    void Awake()
    {
        weapon = Object.Instantiate(weaponAsset);
        fireScript = FindObjectOfType<ProjectileFire>();
        InitialiseAmmo();
    }

    void Start()
    {
        CreateUniqueInstance(currentAmmo);
    }

    void InitialiseAmmo()
    {
        weapon.currentAmmo = weapon.maxAmmo;
    }

    public void CreateUniqueInstance(int ammo)
    {
        weapon = Object.Instantiate(weaponAsset);
        weapon.currentAmmo = ammo;
        currentAmmo = weapon.currentAmmo;
    }
}
