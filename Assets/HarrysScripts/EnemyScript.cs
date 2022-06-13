using LukesScripts.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public float maxHP, currentHP, projectileDamage;
    public GameObject deathSmoke, hitEffect;
    private GameObject lastCollision;
    public AI functionality;
    public Color colour;
    public Temperature temperature;
    public bool red;
    public bool blue;
    public bool yellow;

    [SerializeField] private bool isFightingBoss = false;

    private void Start()
    {
        if (red)
        {
            colour = Color.red;
        }
        else if (blue)
        {
            colour = Color.blue;
        }
        else if (yellow)
        {
            colour = Color.yellow;
        }

        isFightingBoss = functionality.GetType().Equals(typeof(AIWizard));
        if (isFightingBoss && !UIController.instance.bossHealthGroup.activeSelf)
        {
            UIController.instance.bossHealthBar.maxValue = maxHP;
            UIController.instance.bossHealthBar.value = currentHP;
            UIController.instance.bossHealthText.text = currentHP + "/" + maxHP;
            UIController.instance.bossHealthGroup.SetActive(true);
        } else
        {
            if(RoomGenerator.instance.levelIndex < RoomGenerator.instance.numberOfLevels - 1)
            {
                UIController.instance.bossHealthGroup.SetActive(false);
            }
        }
    }

    void Update()
    {
        if(currentHP > maxHP)
        {
            currentHP = maxHP;
        }
        if(currentHP <= 0)
        {
            Instantiate(deathSmoke, gameObject.transform.position, Quaternion.identity);
            if (lastCollision != null)
            {
                lastCollision.transform.SetParent(null);
                lastCollision.GetComponent<Rigidbody>().useGravity = true;
                lastCollision.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            }
            if (functionality != null)
                functionality.Die();
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        var collObj = collision.gameObject;

        if (collObj.CompareTag("Projectile"))
        {
            lastCollision = collObj;
            if (functionality != null)
            {
                Debug.Log("Hit by: " + collObj.tag);
                functionality.Hit();
            }
            if (collObj.GetComponent<ProjectileBehaviour>().colour == colour)
            {
                DamageEnemy(collObj.GetComponent<ProjectileBehaviour>().enemyDamage * 2);
            }
            else if (collObj.GetComponent<ProjectileBehaviour>().colour == Color.grey)
            {
                DamageEnemy(collObj.GetComponent<ProjectileBehaviour>().enemyDamage);
            }
            else
            {
                DamageEnemy(collObj.GetComponent<ProjectileBehaviour>().enemyDamage / 2);
            }

            if (collObj.GetComponent<ProjectileBehaviour>().colour == Color.red)
            {
                temperature.temperature += 50;
            }
            else if (collObj.GetComponent<ProjectileBehaviour>().colour == Color.blue)
            {
                temperature.temperature -= 40;
            }
            else if (collObj.GetComponent<ProjectileBehaviour>().colour == Color.yellow)
            {
                temperature.shockDuration = 4; 
            }


            Instantiate(hitEffect, collObj.transform.position, collObj.transform.rotation);
            Destroy(collObj);
        }
        else if (collObj.CompareTag("Grapple"))
        {
            lastCollision = collObj;
            if (functionality != null)
            {
                Debug.Log("Hit by: " + collObj.tag);
                functionality.Hit();
            }
            Instantiate(hitEffect, collObj.transform.position, collObj.transform.rotation);
            DamageEnemy(collObj.GetComponent<ProjectileBehaviour>().enemyDamage);
        }
    }

    public void DamageEnemy(float damage)
    {
        currentHP -= damage;

        if (isFightingBoss)
        {
            UIController.instance.bossHealthBar.value = currentHP;
            UIController.instance.bossHealthText.text = currentHP + "/" + maxHP;
        }        
    }
}
