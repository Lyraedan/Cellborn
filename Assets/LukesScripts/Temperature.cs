using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Temperature : MonoBehaviour
{
    public float temperature;
    public GameObject fireFX;
    public bool onFire;
    public bool isPlayer;
    PlayerStats playerStats;
    public float fireTimer = 0;
    public EnemyScript enemyScript;
    public NavMeshAgent navMeshAgent;

    void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
    }

    private void FixedUpdate()
    {
        if (temperature >= 60)
        {
            fireFX.SetActive(true);
            onFire = true;
            OnFire();
        }
        else if (temperature >= 40)
        {
            if (onFire)
            {
                OnFire();
            }
        }
        else
        {
            fireFX.SetActive(false);
            onFire = false;
        }

        if (temperature < 1 && temperature > 0)
        {
            temperature = 0;
        }
        else if (temperature > -1 && temperature < 0)
        {
            temperature = 0;
        }

        if (temperature > 0)
        {
            temperature = temperature - 0.024f;
        }
        else if (temperature < 0)
        {
            temperature = temperature + 0.4f;
        }

        //Temperature Cap and Freeze Trigger
        if (temperature > 100)
        {
            temperature = 100;
        }
        else if (temperature <= -80)
        {
            temperature = -80;
            Frozen();
        }
        else
        {
            if (!isPlayer)
            {
                navMeshAgent.speed = 3.5f;
            }
        }
    }

    void OnFire()
    {
        if (fireTimer > 0.25f)
        {
            if (isPlayer)
            {
                playerStats.DamagePlayer(1);
            }
            else
            {
                enemyScript.DamageEnemy(1);
            }
            fireTimer = 0;
        }
        fireTimer = fireTimer + 1 * Time.fixedDeltaTime;
    }

    void Frozen()
    {
        if (isPlayer)
        {
            PlayerMovementTest.instance.potionSpeedMultiplier = 0.25f;
            PlayerMovementTest.instance.speedUpTime = 2;
            PlayerMovementTest.instance.isSpedUp = true;
        }
        else
        {
            navMeshAgent.speed = 0.875f;
        }
    }
}
