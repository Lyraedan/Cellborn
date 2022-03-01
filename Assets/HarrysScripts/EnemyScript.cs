using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public float maxHP, currentHP, projectileDamage;
    public GameObject deathSmoke;

    void Update()
    {
        if(currentHP > maxHP)
        {
            currentHP = maxHP;
        }
        if(currentHP <= 0)
        {
            Instantiate(deathSmoke, gameObject.transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        var collObj = collision.gameObject;

        if (collObj.tag == "Projectile")
        {     
            if (collObj.GetComponent<BouncyBall>())
            {
                if (collObj.GetComponent<BouncyBall>().canDamage && collObj.GetComponent<BouncyBall>().canDamage)
                {
                    currentHP -= 1; 
                }     
            }
            else
            {
                currentHP -= 1; 
            }
        }
    }
}
