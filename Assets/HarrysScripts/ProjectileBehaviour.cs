using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    public Rigidbody projRigidbody;
    public ProjectileFire fireScript;
    public float throwStrength, arcSize;

    public float shotDirection;

    public float lifetime;
    float t;

    public int damage;
    
    void Awake()
    {
        projRigidbody.AddForce((transform.forward * throwStrength) + (transform.up * arcSize), ForceMode.Impulse);
        fireScript = FindObjectOfType<ProjectileFire>();
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
