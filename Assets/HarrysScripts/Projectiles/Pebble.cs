using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pebble : ProjectileBehaviour
{
    public float stoppedCheckTime, currentY, lastY;
    public float checkTimer;

    //[HideInInspector]
    public bool canDamage = true;

    public GameObject destroyEffect;

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

        if (!canDamage)
        {
            enemyDamage = 0;
        }

        t += 1f * Time.deltaTime;

        if (t >= lifetime)
        {
            GameObject destEff = Instantiate(destroyEffect, transform.position, transform.rotation);
            destEff.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            Destroy(gameObject);
        }
    }
}
