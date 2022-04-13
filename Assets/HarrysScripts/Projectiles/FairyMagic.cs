using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FairyMagic : ProjectileBehaviour
{
    public GameObject destroyEffect;
    PlayerStats player;
    public bool canDamage = true;
    
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerStats>();
    }

    // Update is called once per frame
    void Update()
    {
        
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
        if (other.gameObject.tag == "Untagged")
        {
            Instantiate(destroyEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
    
}
