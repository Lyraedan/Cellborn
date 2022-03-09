using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public int maxHP;
    public int currentHP;
    public TextMeshProUGUI healthText;
    public GameObject deathEffect;

    #region Player Components

    public MeshRenderer[] meshRenderers;
    public PlayerMovementTest moveScript;
    public ProjectileFire fireScript;

    #endregion

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
            currentHP = 0;
            
            if(!isDead)
            {
                KillPlayer();
            }
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
        currentHP = 0;
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].enabled = false;
        }
        moveScript.enabled = false;
        fireScript.enabled = false;
        isDead = true;
    }
}