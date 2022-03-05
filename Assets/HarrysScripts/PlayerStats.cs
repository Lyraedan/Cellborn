using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public int maxHP;
    int currentHP;
    public TextMeshProUGUI healthText;

    bool isDead;
    
    void Start()
    {
        currentHP = maxHP;
    }

    void Update()
    {
        healthText.text = "HP: " + currentHP + " / " + maxHP;
        
        if (currentHP <= 0)
        {
            KillPlayer();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        var collObj = collision.gameObject;

        if (collObj.tag == "Projectile")
        {     
            currentHP -= collObj.GetComponentInChildren<ProjectileBehaviour>().playerDamage; 
        }
    }

    public void KillPlayer()
    {
        GetComponentInChildren<MeshRenderer>().enabled = false;
        GetComponent<PlayerMovementTest>().enabled = false;
        isDead = true;
    }
}