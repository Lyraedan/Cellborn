using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncyBall : ProjectileBehaviour
{
    public List<Color> ballColors;
    Light ballLight;
    int colorIndex;
    float timer; // Time
    public bool wizard = false;

    Material instMat;

    [HideInInspector]
    public bool canDamage;
    
    void Awake()
    {
        if (!wizard)
        {
            //projRigidbody.AddForce((transform.forward * throwStrength) + (transform.up * arcSize), ForceMode.Impulse);
        }

        instMat = gameObject.GetComponent<Renderer>().material;
    }

    void Start()
    {
        colorIndex = Random.Range(0, ballColors.Count);
        instMat.EnableKeyword("_EMISSION");
        instMat.SetColor("_Color", ballColors[colorIndex]);
        instMat.SetColor("_EmissionColor", ballColors[colorIndex]);
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
        var collObj = collision.gameObject;

        if (!collision.gameObject.GetComponent<EnemyScript>())
        {
            canDamage = true;
        }

        if (collObj.tag == "Player")
        {
            Destroy(this);
        }
    }
}
