using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Temperature : MonoBehaviour
{
    public float temperature;
    public GameObject fireFX;
    public GameObject iceFX;
    public GameObject shockFX;
    public bool onFire;
    public bool isPlayer;
    public float fireTimer = 0;
    public float shockTimer = 0;
    public EnemyScript enemyScript;
    public NavMeshAgent navMeshAgent;
    public float shockDuration = 0;

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

        if (temperature < 0)
        {
            Frozen();
        }
        else
        {
            iceFX.SetActive(false);
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
            temperature = temperature - 0.4f;
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
        }

        if (shockDuration > 0)
        {
            OnShocked();
        }
        else
        {
            shockFX.SetActive(false);
        }
    }

    void OnShocked()
    {

        if (shockTimer > 0.25f)
        {
            shockFX.SetActive(true);
            if (isPlayer)
            {
                PlayerStats.instance.DamagePlayer(3);
            }
            else
            {
                enemyScript.DamageEnemy(3);
            }
            shockTimer = 0;
            shockDuration--;
        }
        shockTimer = shockTimer + 1 * Time.fixedDeltaTime;

        if (isPlayer)
        {
            PlayerMovementTest.instance.potionSpeedMultiplier = 0.25f;
            PlayerMovementTest.instance.speedUpTime = 0.25f;
            PlayerMovementTest.instance.isSpedUp = true;
        }
        else
        {
            navMeshAgent.speed = 0.875f;
        }
    }

    void OnFire()
    {
        if (fireTimer > 0.25f)
        {
            if (isPlayer)
            {
                PlayerStats.instance.DamagePlayer(1);
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
        iceFX.SetActive(true);

        if (temperature <= -80)
        {
            if (isPlayer)
            {
                PlayerMovementTest.instance.potionSpeedMultiplier = 0;
                PlayerMovementTest.instance.speedUpTime = 2;
                PlayerMovementTest.instance.isSpedUp = true;
            }
            else
            {
                navMeshAgent.speed = 0f;
                Debug.Log("temp <= -80");
            }
        }
        else if (temperature < -50)
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
                Debug.Log("temp <= -50");
            }
        }
        else if (temperature < -40)
        {
            if (isPlayer)
            {
                PlayerMovementTest.instance.potionSpeedMultiplier = 0.45f;
                PlayerMovementTest.instance.speedUpTime = 2;
                PlayerMovementTest.instance.isSpedUp = true;
            }
            else
            {
                navMeshAgent.speed = 1.575f;
                Debug.Log("temp <= -40");
            }
        }
        else if (temperature < -30)
        {
            if (isPlayer)
            {
                PlayerMovementTest.instance.potionSpeedMultiplier = 0.65f;
                PlayerMovementTest.instance.speedUpTime = 2;
                PlayerMovementTest.instance.isSpedUp = true;
            }
            else
            {
                navMeshAgent.speed = 2.275f;
            }
        }
        else if (temperature < -20)
        {
            if (isPlayer)
            {
                PlayerMovementTest.instance.potionSpeedMultiplier = 0.85f;
                PlayerMovementTest.instance.speedUpTime = 2;
                PlayerMovementTest.instance.isSpedUp = true;
            }
            else
            {
                navMeshAgent.speed = 2.975f;
            }
        }
        else if (temperature < -10)
        {
            if (isPlayer)
            {
                PlayerMovementTest.instance.potionSpeedMultiplier = 0.95f;
                PlayerMovementTest.instance.speedUpTime = 2;
                PlayerMovementTest.instance.isSpedUp = true;
            }
            else
            {
                navMeshAgent.speed = 3.325f;
            }
        }
        else
        {
            if (!isPlayer)
            {
                navMeshAgent.speed = 3.5f;
            }
        }

    }
}
