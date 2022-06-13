using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PotionManager : MonoBehaviour
{
    public static PotionManager instance;

    public GameObject player;

    public TextMeshProUGUI useText, pickupText;
    public GameObject firePoint;
    public PlayerStats healthScript;

    public List<GameObject> possiblePotions = new List<GameObject>();
    
    public PotionProperties emptyPotion;
    public List<PotionProperties> currentlyHeldPotions = new List<PotionProperties>();

    public SlotHolder[] potionSlots;

    public PotionProperties _currentPotion;
    public PotionProperties currentPotion;
    public PotionProperties toPickup;

    public GameObject potionDrinkEffect;
    public ParticleSystem potionDrinkEffectPS;

    public AudioSource source01, source02, source03;
    public AudioClip pickupSound, drinkSound, effectSound;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public bool areSlotsFull {
        get {
            return !HasPotionInInventory(-1);
        }
    }

    void Start()
    {
        for (int i = 0; i < currentlyHeldPotions.Count; i++)
        {
            currentlyHeldPotions[i] = emptyPotion;
        }

        //currentlyHeldPotions[0] = FindPotion(1);
        //urrentlyHeldPotions[1] = FindPotion(4);

        source01.clip = pickupSound;
        source02.clip = drinkSound;
        source03.clip = effectSound;
    }

    void Update()
    {
        for (int i = 0; i < potionSlots.Length; i++)
        {
            if (potionSlots[i] != null)
            {
                potionSlots[i].number.text = new string[] { ControlManager.usePotionA, ControlManager.usePotionB }[i];
                
                if (potionSlots[i].image.sprite != null)
                {
                    potionSlots[i].image.sprite = currentlyHeldPotions[i].icon;
                }

                if (potionSlots[i].identifier.sprite != null)
                {
                    potionSlots[i].identifier.sprite = currentlyHeldPotions[i].identifier;
                }
            }
        }

        if (Input.GetButtonDown(ControlManager.INPUT_USE_POTION_A))
        {
            if (currentlyHeldPotions[0] == emptyPotion)
                return;

            if (currentlyHeldPotions[0].potionID == 1 && WeaponManager.instance.currentWeapon.infiniteAmmo)
                return;

            if (currentlyHeldPotions[0].potionID == 1 && WeaponManager.instance.currentWeapon.weaponId == 4)
                return;

            if (!healthScript.isDead)
            {
                var psMain = potionDrinkEffectPS.main;
                psMain.startColor = new ParticleSystem.MinMaxGradient(currentlyHeldPotions[0].drinkEffectColor);
                potionDrinkEffect.SetActive(true);
                currentlyHeldPotions[0].Consume();
                source02.Play();
                source03.Play();
                if (currentlyHeldPotions[0].functionality != null)
                {
                    currentlyHeldPotions[0] = emptyPotion;
                }
            }
        }

        if (Input.GetButtonDown(ControlManager.INPUT_USE_POTION_B))
        {
            if (currentlyHeldPotions[1] == emptyPotion)
                return;

            if (currentlyHeldPotions[0].potionID == 1 && WeaponManager.instance.currentWeapon.infiniteAmmo)
                return;

            if (currentlyHeldPotions[0].potionID == 1 && WeaponManager.instance.currentWeapon.weaponId == 4)
                return;

            if (!healthScript.isDead)
            {
                var psMain = potionDrinkEffectPS.main;
                psMain.startColor = new ParticleSystem.MinMaxGradient(currentlyHeldPotions[1].drinkEffectColor);
                potionDrinkEffect.SetActive(true);
                currentlyHeldPotions[1].Consume();
                source02.Play();
                source03.Play();
                if (currentlyHeldPotions[1].functionality != null)
                {
                    currentlyHeldPotions[1] = emptyPotion;
                }
            }
        }

        if (Input.GetButtonDown(ControlManager.INPUT_PICKUP))
        {
            if (toPickup != null)
                Pickup(toPickup);
        }
    }

    public void Pickup(PotionProperties potion)
    {
        PotionProperties found = FindPotion(potion.potionID);

        if(!areSlotsFull)
        {
            var index = currentlyHeldPotions.IndexOf(FindPotion(-1));
            currentlyHeldPotions[index] = found;

            var slot = potionSlots[index];
            slot.image.sprite = currentlyHeldPotions[index].icon;
            slot.identifier.sprite = currentlyHeldPotions[index].identifier;

            Destroy(potion.gameObject);
            pickupText.text = string.Empty;

            source01.Play();
        }
        else
        {
            Debug.LogError("Inventory is full!");
        }
    }

    PotionProperties FindPotion(int potionId)
    {
        PotionProperties result = null;
        foreach (GameObject property in possiblePotions)
        {
            PotionProperties prop = property.GetComponent<PotionProperties>();
            if (prop.potionID == potionId)
            {
                result = prop;
                break;
            }
        }
        return result;
    }

    public bool HasPotionInInventory(int potionId)
    {
        bool result = false;
        foreach (PotionProperties property in currentlyHeldPotions)
        {
            if (property.potionID == potionId)
            {
                result = true;
                break;
            }
        }
        return result;
    }
}
