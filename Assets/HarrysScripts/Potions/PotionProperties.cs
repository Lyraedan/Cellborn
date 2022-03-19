using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionProperties : MonoBehaviour
{
    [Header("Properties")]
    public string potionName = "";
    [TextArea(3, 10)]
    public string description = "";
    public int potionID;
    public int maxUses, currentUses;
    public Sprite icon;

    [Header("Set Functionality")]
    public PotionBase functionality;
    
    public void Consume()
    {
        if(functionality == null)
        {
            Debug.Log("This is a dud potion! (No functionality found!)");
            return;
        }

        if(functionality.infiniteUses)
            functionality.Use();
        else
        {
            if (currentUses > 0)
            {
                currentUses--;
                functionality.Use();
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
            PotionManager.instance.toPickup = this;
            PotionManager.instance.pickupText.text = $"F - Pick Up {potionName}";
        }        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PotionManager.instance.toPickup = null;
            PotionManager.instance.pickupText.text = string.Empty;
        }
    }
}
