using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleGravity : MonoBehaviour
{
    public float pullRadius, objectPullForce, playerPullForce;
    public GameObject destroyEffect;
    public float holeTime, healthDrainMultiplier;
    float t, healthDrain;
       
    void FixedUpdate()
    {
        foreach (Collider collider in Physics.OverlapSphere(transform.position, pullRadius))
        {
            Vector3 forceDirection = gameObject.transform.position - collider.transform.position;

            float holeDistance = pullRadius - Vector3.Distance(gameObject.transform.position, collider.transform.position);
            float drainDistance = Vector3.Distance(gameObject.transform.position, collider.transform.position) / healthDrainMultiplier;
            holeDistance = holeDistance / pullRadius;

            var rigidBody = collider.GetComponent<Rigidbody>();
            var characterController = collider.GetComponent<CharacterController>();
            var playerStats = collider.GetComponent<PlayerStats>();

            if (rigidBody != null)
            {
                rigidBody.AddForce(forceDirection.normalized * holeDistance * objectPullForce * Time.deltaTime); 
            }             

            if (characterController != null)
            {
                characterController.Move(forceDirection.normalized * holeDistance * playerPullForce * Time.deltaTime);

                healthDrain += Time.deltaTime;
                if (healthDrain >= drainDistance)
                {
                    if (playerStats != null)
                    {
                        playerStats.currentHP -= 1;
                    }
                    healthDrain = 0;
                }
            }    
        }

        t += Time.deltaTime;
        if (t >= holeTime)
        {
            Destroy(gameObject);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        Instantiate(destroyEffect, other.transform.position, other.transform.rotation);
        
        if (other.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerStats>().KillPlayer();
        }
        else
        {
            Destroy(other.gameObject);
        }
    }
}
