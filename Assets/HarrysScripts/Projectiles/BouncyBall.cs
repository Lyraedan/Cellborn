using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncyBall : ProjectileBehaviour
{
    public List<Color> ballColors;
    Light ballLight;
    int colorIndex;
    float t; // Time
    public bool wizard = false;

    [HideInInspector]
    public bool canDamage;
    
    void Awake()
    {
        if (wizard == false)
        {
            projRigidbody.AddForce((transform.forward * throwStrength) + (transform.up * arcSize), ForceMode.Impulse);
        }
    }

    void Start()
    {
        colorIndex = Random.Range(0, ballColors.Count);
        gameObject.GetComponent<Renderer>().material.color = ballColors[colorIndex];
        ballLight = GetComponentInChildren<Light>();
        ballLight.color = ballColors[colorIndex];
    }

    void Update()
    {
        /*
        if (canDamage)
        {
            enemyDamage = 1;
            playerDamage = 1;
        }
        else
        {
            enemyDamage = 0;
            playerDamage = 1;
        }
        */
        
        t += Time.deltaTime;

        if (t >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(!collision.gameObject.GetComponent<EnemyScript>())
        {
            canDamage = true;
        }
    }
}
