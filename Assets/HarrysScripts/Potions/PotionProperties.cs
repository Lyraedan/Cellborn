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
    public Sprite icon;
    public Sprite identifier;

    [Header("Set Functionality")]
    public PotionBase functionality;

    public float DistanceFromPlayer
    {
        get
        {
            if (PotionManager.instance == null)
                return Mathf.Infinity;

            return Vector3.Distance(transform.position, PotionManager.instance.player.transform.position);
        }
    }
    
    private void Start()
    {
        if (functionality != null)
        {
            if(!functionality.isInPlayerInventory)
                PlayerStats.instance.potionsInScene.Add(this);
        }
    }

    private void OnDestroy()
    {
        if (functionality != null)
        {
            if (!functionality.isInPlayerInventory)
                PlayerStats.instance.potionsInScene.Remove(this);
        }
    }

    public void Consume()
    {
        if(functionality == null)
        {
            Debug.Log("This is a dud potion! (No functionality found!)");
            return;
        }

        AudioManager.instance.Play("PotionUse");
        functionality.Use();
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
