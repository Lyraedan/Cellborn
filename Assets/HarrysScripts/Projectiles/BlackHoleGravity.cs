using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BlackHoleGravity : MonoBehaviour
{
    public Color colour = Color.white;
    public GameObject destroyEffect;
    public float damage = 1f;
    public float pullForce = 10f;

    [Header("Force Multipliers")]
    public float playerForceMultiplier = 1f;
    public float enemyForceMultiplier = 25f;
    public float propForceMultiplier = 1f;
    public float projectileForceMultiplier = 1f;

    public AudioSource source;

    private void Start()
    {
        StartCoroutine(DestroyAfterSound());
    }

    public void OnTriggerStay(Collider other)
    {
        Debug.Log($"Succ the {other.name} -> {other.tag}!");
        var go = other.gameObject;
        float distance = Vector3.Distance(transform.position, go.transform.position);
        int damageAmount = (int) (damage / (distance + 1)) + 1; // distance + 1 because we can not divide by 0
        var rigidbody = go.GetComponent<Rigidbody>();

        if (rigidbody || other.CompareTag("Player"))
        {
            // Direction relative to the center
            Vector3 heading = (transform.position - go.transform.position).normalized;
            Vector3 force = heading * (pullForce / distance);

            if (other.CompareTag("Enemy"))
            {
                EnemyScript enemy = go.GetComponent<EnemyScript>();
                float weight = rigidbody.mass * -Physics.gravity.y;
                rigidbody.AddForce(force * weight * enemyForceMultiplier * Time.deltaTime);
                enemy.DamageEnemy(damageAmount);
            }
            else if (other.CompareTag("Player"))
            {
                PlayerStats stats = go.GetComponent<PlayerStats>();
                RoomGenerator.instance.playerController.controller.Move(force * playerForceMultiplier * Time.deltaTime);
                stats.DamagePlayer(damageAmount);
            }
            else if (other.CompareTag("Projectile") || other.CompareTag("EnemyProjectile"))
            {
                float weight = rigidbody.mass * -Physics.gravity.y;
                rigidbody.AddForce(force * weight * projectileForceMultiplier  * Time.deltaTime);
            }
            else if (other.CompareTag("Prop"))
            {
                float weight = rigidbody.mass * Physics.gravity.y;
                rigidbody.AddForce(force * weight * propForceMultiplier * Time.deltaTime);
            }
        }
    }

    IEnumerator DestroyAfterSound()
    {
        yield return new WaitForSeconds(source.clip.length);
        Destroy(gameObject);
    }
}
