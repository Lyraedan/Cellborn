using UnityEngine;

public class WeaponObject : MonoBehaviour
{
    public GameObject weaponAsset;
    public GameObject weapon;   
    public Weapon weaponScript;
    public ProjectileFire fireScript; 

    [SerializeField]int currentAmmoValue;

    void Awake()
    {
        fireScript = FindObjectOfType<ProjectileFire>();

        if (fireScript.equippedWeapon == null)
        {
            weapon = Object.Instantiate(weaponAsset);
            weaponScript = weapon.GetComponent<Weapon>();
        }
        else
        {
            weaponScript = fireScript.equippedWeapon;
            currentAmmoValue = fireScript.equippedWeapon.currentAmmo;
        }              
    }        

    void Start()
    {        
        //CreateUniqueInstance(currentAmmoValue);
    }

    public void CreateUniqueInstance(int ammo)
    {
        //weapon = Object.Instantiate(weaponAsset);
        //fireScript.equippedWeapon.currentAmmo = ammo;
    }

    public void DestroyPrefab()
    {
        Destroy(weapon);
    }
}
