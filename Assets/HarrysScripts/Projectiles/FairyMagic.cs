using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FairyMagic : ProjectileBehaviour
{
    public GameObject destroyEffect;
    PlayerStats player;
    public bool canDamage = false;
    public float noDamageTime;
    float damageTimer;
    
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerStats>();
    }

    // Update is called once per frame
    void Update()
    {
        damageTimer += 1f * Time.deltaTime;

        if (damageTimer >= noDamageTime)
        {
            canDamage = true;
        }
    }

    public void HitPlayer()
    {
        if (player != null)
        {
            player.DamagePlayer(playerDamage);
            Instantiate(destroyEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }   
    }

    void OnCollisionEnter(Collision other)
    {
        if (canDamage)
        {
            if (other.gameObject.tag == "Untagged" || other.gameObject.tag == "Prop" || other.gameObject.tag == "Environment")
            {
                Instantiate(destroyEffect, transform.position, transform.rotation);
                Destroy(gameObject);
            }
            else if (other.gameObject.tag == "Enemy")
            {
                EnemyScript enemyScript = other.gameObject.GetComponent<EnemyScript>();
                enemyScript.currentHP = enemyScript.currentHP - enemyDamage;
                Instantiate(destroyEffect, transform.position, transform.rotation);
                Destroy(gameObject);
            }
        }
    }
    
}
