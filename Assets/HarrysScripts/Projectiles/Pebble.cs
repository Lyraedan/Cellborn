using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pebble : ProjectileBehaviour
{
    public float stoppedCheckTime, currentY, lastY;
    public float checkTimer;

    //[HideInInspector]
    public bool canDamage = true;

    void Update()
    {
        currentY = gameObject.transform.position.y;

        if (currentY == lastY)
        {
            //Is on floor
            if (canDamage)
            {
                checkTimer += 1f * Time.deltaTime;
            }
        }
        else
        {
            lastY = currentY;

            checkTimer = 0;
        }

        if (checkTimer >= stoppedCheckTime)
        {
            canDamage = false;
        }

        if (canDamage)
        {
            enemyDamage = 1;
        }
        else
        {
            enemyDamage = 0;
        }

        t += 1f * Time.deltaTime;

        if (t >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
