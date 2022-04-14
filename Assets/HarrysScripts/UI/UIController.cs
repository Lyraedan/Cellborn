using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Player Scripts")]
    public WeaponManager weaponManager;
    public PotionManager potionManager;
    public PlayerStats playerStats;

    [Header("Health")]
    public Slider healthBar;

    [Header("Ammo")]
    public GameObject ammoContainer;
    public Slider ammoBar;

    [Header("Minimap")]
    public Image minimap;
    public Image minimapBG;

    void Start()
    {
        #region Minimap

        minimapBG.SetNativeSize();

        #endregion
    }

    void Update()
    {
        #region Health Bar

        healthBar.maxValue = playerStats.maxHP;
        healthBar.value = playerStats.currentHP;        

        #endregion

        #region Ammo Bar

        if(weaponManager.currentWeapon.functionality.infiniteAmmo)
        {
            ammoContainer.SetActive(false);
        }
        else if (weaponManager.currentWeapon.currentAmmo == 0)
        {
            ammoContainer.SetActive(false);
        }
        else if (weaponManager.currentWeapon == null)
        {
            ammoContainer.SetActive(false);
        }
        else
        {
            ammoContainer.SetActive(true);
        }

        ammoBar.maxValue = weaponManager.currentWeapon.maxAmmo;
        ammoBar.value = weaponManager.currentWeapon.currentAmmo;

        #endregion

        #region Minimap

        minimapBG.rectTransform.position = minimap.rectTransform.position;
        minimapBG.rectTransform.sizeDelta = new Vector2(minimap.sprite.rect.width * 4, minimap.sprite.rect.height * 4);

        #endregion
    }
}
