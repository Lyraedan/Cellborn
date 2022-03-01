using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncyBall : ProjectileBehaviour
{
    public List<Color> ballColors;
    Light ballLight;
    int colorIndex;
    float t;
<<<<<<< Updated upstream
=======

    [HideInInspector]
    public bool canDamage;
>>>>>>> Stashed changes
    
    void Awake()
    {
        projRigidbody.AddForce((transform.forward * throwStrength) + (transform.up * arcSize), ForceMode.Impulse);
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
        t += Time.deltaTime;

        if (t >= lifetime)
        {
            Destroy(gameObject);
        }
    }
<<<<<<< Updated upstream
=======

    public void OnCollisionEnter(Collision collision)
    {
        if(!collision.gameObject.GetComponent<EnemyScript>())
        {
            canDamage = true;
        }
    }
>>>>>>> Stashed changes
}
