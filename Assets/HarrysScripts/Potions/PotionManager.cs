using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PotionManager : MonoBehaviour
{
    public static PotionManager instance;

    public GameObject player;

    public KeyCode[] useKeys = new KeyCode[] { KeyCode.Q, KeyCode.E };
    public KeyCode pickupKey = KeyCode.F;
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
    }

    void Update()
    {
        for (int i = 0; i < potionSlots.Length; i++)
        {
            if (potionSlots[i] != null)
            {
                potionSlots[i].number.text = useKeys[i].ToString();
                
                if (potionSlots[i].image.sprite != null)
                {
                    potionSlots[i].image.sprite = currentlyHeldPotions[i].icon;
                }
            }
        }

        if (Input.GetKey(useKeys[0]))
        {
            if (!healthScript.isDead)
            {
                currentlyHeldPotions[0].Consume();
                if (currentlyHeldPotions[0].functionality != null)
                {
                    currentlyHeldPotions[0] = emptyPotion;
                }
            }
        }

        if (Input.GetKey(useKeys[1]))
        {
            if (!healthScript.isDead)
            {
                currentlyHeldPotions[1].Consume();
                if (currentlyHeldPotions[1].functionality != null)
                {
                    currentlyHeldPotions[1] = emptyPotion;
                }
            }
        }

        if (Input.GetKeyDown(pickupKey))
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

            Destroy(potion.gameObject);
            pickupText.text = string.Empty;
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
