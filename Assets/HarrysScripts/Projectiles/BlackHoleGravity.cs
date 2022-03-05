using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleGravity : MonoBehaviour
{
    public float pullRadius, objectPullForce, playerPullForce;
    public GameObject destroyEffect;
    public float holeTime;
    float t;
       
    void FixedUpdate()
    {
        foreach (Collider collider in Physics.OverlapSphere(transform.position, pullRadius))
        {
            Vector3 forceDirection = gameObject.transform.position - collider.transform.position;

            var rigidBody = collider.GetComponent<Rigidbody>(); 
            var characterController = collider.GetComponent<CharacterController>();

            if (rigidBody != null)
            {
                rigidBody.AddForce(forceDirection.normalized * objectPullForce * Time.deltaTime); 
            }             

            if (characterController != null)
            {
                characterController.Move(forceDirection.normalized * playerPullForce * Time.deltaTime); 
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
