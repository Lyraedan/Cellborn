using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncyBall : ProjectileBehaviour
{
    public List<Color> ballColors;
    int colorIndex;
    float t;
    bool canDamage;
    
    void Awake()
    {
        projRigidbody.AddForce((transform.forward * throwStrength) + (transform.up * arcSize), ForceMode.Impulse);
    }

    void Start()
    {
        colorIndex = Random.Range(0, ballColors.Count);
        gameObject.GetComponent<Renderer>().material.color = ballColors[colorIndex];
    }

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
        canDamage = true;
    }
}
