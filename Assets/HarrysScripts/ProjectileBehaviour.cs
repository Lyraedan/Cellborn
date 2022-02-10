using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    public Rigidbody projRigidbody;
    public float throwStrength, arcSize;

    public float lifetime;
    float t;
    
    void Awake()
    {
        projRigidbody.AddForce((transform.forward * throwStrength) + (transform.up * arcSize), ForceMode.Impulse);
    }

    void Update()
    {
        t += Time.deltaTime;

        if (t >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
