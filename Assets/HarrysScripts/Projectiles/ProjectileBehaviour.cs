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

    public int enemyDamage;
    public int playerDamage;

    public bool canChangeDistance;
    
    void Awake()
    {
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

    public void FireProjectile(float horizontalVelocity)
    {
        if (canChangeDistance)
        {
            projRigidbody.AddForce((transform.forward * horizontalVelocity) + (transform.up * arcSize), ForceMode.Impulse);
        }
        else
        {
            projRigidbody.AddForce((transform.forward * throwStrength) + (transform.up * arcSize), ForceMode.Impulse);
        }
    }
}
