using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleGravity : MonoBehaviour
{
    public float pullRadius, objectPullForce, playerPullForce;
    public GameObject destroyEffect;
    public float holeTime, playerHealthDrainMultiplier, enemyHealthDrainMultiplier;
    public int playerDamage, enemyDamage;
    float t, healthDrain;
    public Color colour = Color.white;

    bool playerIsDead;
       
    void FixedUpdate()
    {
        foreach (Collider collider in Physics.OverlapSphere(transform.position, pullRadius))
        {
            Vector3 forceDirection = gameObject.transform.position - collider.transform.position;

            var rigidBody = collider.GetComponent<Rigidbody>();
            var characterController = collider.GetComponent<CharacterController>();
            var playerStats = collider.GetComponent<PlayerStats>();
            var enemyStats = collider.GetComponent<EnemyScript>();

            float holeDistance = pullRadius - Vector3.Distance(gameObject.transform.position, collider.transform.position);
            holeDistance = holeDistance / pullRadius;
            float drainDistance = 0f;

            if (playerStats != null)
            {
                drainDistance = Vector3.Distance(gameObject.transform.position, collider.transform.position) / playerHealthDrainMultiplier;

                if (characterController != null)
                {
                    characterController.Move(forceDirection.normalized * holeDistance * playerPullForce * Time.deltaTime);

                    healthDrain += Time.deltaTime;
                    if (healthDrain >= drainDistance)
                    {
                        if (playerStats != null && playerStats.currentHP > 0)
                        {
                            playerStats.DamagePlayer(playerDamage);
                        }
                        healthDrain = 0;
                    }

                    playerIsDead = playerStats.isDead;
                }
            }
            
            if (enemyStats != null)
            {
                drainDistance = Vector3.Distance(gameObject.transform.position, collider.transform.position) / enemyHealthDrainMultiplier;

                if (rigidBody != null)
                {
                    rigidBody.AddForce(forceDirection.normalized * holeDistance * objectPullForce * Time.deltaTime); 

                    healthDrain += Time.deltaTime;
                    if (healthDrain >= drainDistance)
                    {
                        if (enemyStats != null && enemyStats.currentHP > 0)
                        {
                            if (colour == enemyStats.colour)
                            {
                                enemyStats.DamageEnemy(enemyDamage*2);
                            }
                            else if (colour == Color.grey)
                            {
                                enemyStats.DamageEnemy(enemyDamage);
                            }
                            else
                            {
                                enemyStats.DamageEnemy(enemyDamage/2);
                            }

                        }
                        healthDrain = 0;
                    }
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
        if (other.tag == "Pickup" || other.tag == "Prop" || other.tag == "Entity" || other.tag == "Projectile" || other.tag == "EnemyProjectile")
        {
            Instantiate(destroyEffect, other.transform.position, other.transform.rotation);
            Destroy(other.gameObject);
        }
    }
}
