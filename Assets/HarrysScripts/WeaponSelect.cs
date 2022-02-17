using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSelect : MonoBehaviour
{
    public GameObject slotContainer;
    public UIWeaponSlot[] weaponSlots;
    public WeaponPickup weaponPickup;
    public KeyCode dropKey;
    [SerializeField]int currentSlot;

    public Transform dropPosition;

    void Start()
    {
        weaponSlots = slotContainer.GetComponentsInChildren<UIWeaponSlot>();

        for (int i = 0; i < weaponSlots.Length; i++)
        {
            weaponSlots[i].DeselectSlot();
        }

        weaponSlots[0].SelectSlot();        
    }
    
    void Update()
    {       
        #region Select Slot

        if (Input.GetAxis("Mouse ScrollWheel") < 0f ) // forward
        {
            weaponSlots[currentSlot].DeselectSlot();
            currentSlot++;
            if (currentSlot < weaponSlots.Length)
            {
                weaponSlots[currentSlot].SelectSlot();
            }
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0f ) // backwards
        {
            weaponSlots[currentSlot].DeselectSlot();
            currentSlot--;

            if (currentSlot >= 0)
            {
                weaponSlots[currentSlot].SelectSlot();
            }
        }

        #endregion

        #region Value Clamps

        if (currentSlot >= (weaponSlots.Length))
        {
            currentSlot = 0;
            weaponSlots[currentSlot].SelectSlot();
        }

        if (currentSlot < 0)
        {
            currentSlot = weaponSlots.Length - 1;
            weaponSlots[currentSlot].SelectSlot();
        }

        #endregion

        if (weaponSlots[currentSlot].weapon != null)
        {
            weaponSlots[currentSlot].EquipWeapon(weaponSlots[currentSlot].weapon);
        }
        else
        {
            weaponSlots[currentSlot].ResetParameters();
        }

        if (Input.GetKeyDown(dropKey) && currentSlot != 0 && weaponSlots[currentSlot].weapon != null)
        {
            weaponSlots[currentSlot].weapon.prefab.GetComponent<WeaponObject>().CreateUniqueInstance(weaponSlots[currentSlot].weapon.currentAmmo);
            Instantiate(weaponSlots[currentSlot].weapon.prefab, dropPosition.position, dropPosition.rotation);
            weaponSlots[currentSlot].weapon = null;
            weaponPickup.SetInventoryIsFull(false);
        }
    }
}
