using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITest : AI
{
    Vector3 next;

    [SerializeField] private float roamRadius = 3;
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
        } else if(DistanceFromPlayer < 3 && DistanceFromPlayer > 1.5f)
        {
            // Player is here!
            MoveTo(WeaponManager.instance.player.transform.position);
            attackDelay = 0;
        } else if(runTimer >= runTimeout)
        {
            // Couldn't reach destination in time
            FindNewLocation();
            runTimer = 0;
        }

        // We are within attacking distance
        if(DistanceFromPlayer <= 1.5f)
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
        next = RandomNavmeshLocation(roamRadius);
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
