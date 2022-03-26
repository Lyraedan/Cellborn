using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public float maxHP, currentHP, projectileDamage;
    public GameObject deathSmoke;
    private GameObject lastCollision;
    public AI functionality;

    void Update()
    {
        if(currentHP > maxHP)
        {
            currentHP = maxHP;
        }
        if(currentHP <= 0)
        {
            Instantiate(deathSmoke, gameObject.transform.position, Quaternion.identity);
            if (lastCollision != null)
            {
                lastCollision.transform.SetParent(null);
                lastCollision.GetComponent<Rigidbody>().useGravity = true;
                lastCollision.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            }
            if (functionality != null)
                functionality.Die();
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        var collObj = collision.gameObject;

        if (collObj.tag == "Projectile")
        {
            lastCollision = collObj;
            if (functionality != null)
                functionality.OnHit();
            currentHP -= collObj.GetComponent<ProjectileBehaviour>().enemyDamage; 
        }
    }
}
