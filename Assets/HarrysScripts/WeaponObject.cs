using UnityEngine;

public class WeaponObject : MonoBehaviour
{
    public GameObject weaponAsset;
    public GameObject weapon;   
    public Weapon weaponScript;
    public ProjectileFire fireScript; 

    [SerializeField]int currentAmmoValue;

    // Don't do this in Awake good lord! - Please find a better tutorial :) <3 :*
    void Start()
    {
        fireScript = FindObjectOfType<ProjectileFire>();

        if (fireScript.equippedWeapon == null)
        {
            weapon = Instantiate(weaponAsset);
            weaponScript = weapon.GetComponent<Weapon>();
        }
        else
        {
            weaponScript = fireScript.equippedWeapon;
            //currentAmmoValue = fireScript.equippedWeapon.currentAmmo;
        }              
    }        

    /*
    void Start()
    {        
        //CreateUniqueInstance(currentAmmoValue);
    }
    */

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
