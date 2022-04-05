using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : ProjectileBehaviour
{
    //float t; // Time

    [HideInInspector]
    public bool canDamage; 
    
    void Update()
    {
        t += Time.deltaTime;

        if (t >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        var collObj = collision.gameObject;

        if (!collision.gameObject.GetComponent<EnemyScript>())
        {
            canDamage = true;
        }

        gameObject.GetComponent<Rigidbody>().useGravity = true;
    }
}
