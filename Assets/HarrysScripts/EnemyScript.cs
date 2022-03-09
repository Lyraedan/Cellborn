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
            currentHP -= collObj.GetComponent<ProjectileBehaviour>().enemyDamage; 
        }
    }
}
