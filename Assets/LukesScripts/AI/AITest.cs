using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITest : AI
{
    Vector3 next;

    private int secondsBetweenAttacks = 3;
    private float attackDelay = 0;

    public override void Init()
    {
        next = RandomNavmeshLocation(3);
    }

    public override void Tick()
    {
        if(agent.remainingDistance <= 1 && DistanceFromPlayer > 5)
        {
            // Reached destination
            FindNewLocation();
        } else if(DistanceFromPlayer < 3)
        {
            // Player is here!
            MoveTo(WeaponManager.instance.player.transform.position);
        } else if(runTimer >= runTimeout)
        {
            // Couldn't reach destination in time
            FindNewLocation();
        }

        // We are within attacking distance
        if(DistanceFromPlayer < 1.5f)
        {
            attackDelay += 1f * Time.deltaTime;
            if(attackDelay >= secondsBetweenAttacks)
            {
                Attack();
                attackDelay = 0;
            }
        }
    }

    void FindNewLocation()
    {
        next = RandomNavmeshLocation(3);
        MoveTo(next);
    }

    public override void Attack()
    {
        Debug.Log("Do attack!");
    }

    public override void DrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(next, 0.25f);
    }
}