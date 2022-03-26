using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIFairy : AI
{
    private Vector3 next;

    [SerializeField] private float roamRadius = 3f;
    private float secondsBetweenAttacks = 1f;
    private float attackDelay = 0;
    public PlayerStats playerStats;
    private int damage = 1;
    [SerializeField] private int chanceOfAgro = 4;

    private bool persuePlayerAfterHit = false;
    private float persueTimer = 0f;
    private float persueTimeout = 10f;

    private Vector3 patrolPoint;
    [SerializeField] private float patrolRadius = 100f;
    [SerializeField] private float patrolDurationMin = 10;
    [SerializeField] private float patrolDurationMax = 60;
    [SerializeField] private float maxRoamFromPatrolPoint = 50f;
    private float patrolDurationChosen = 0;
    private float patrolTimer = 0;
    [SerializeField] private bool returningToPatrolPoint = false;

    private bool flee = false;
    private float fleeTimer = 0;
    [SerializeField] private float maxFleeDistance = 30f;

    private float DistanceFromPatrolPoint
    {
        get
        {
            return Vector3.Distance(OurPosition, patrolPoint);
        }
    }

    [SerializeField] private float remainingDistance = 0f;

    public override void Init()
    {
        next = RandomNavmeshLocation(3);
    }

    public override void Tick()
    {
        if (!IsOnNavmesh)
        {
            var validated = GetNearestNavmeshLocation(OurPosition);
            OurPosition = validated;
            return;
        }

        remainingDistance = agent.remainingDistance;

        if (!persuePlayerAfterHit && !flee)
        {
            bool changePatrolArea = patrolTimer >= patrolDurationChosen;
            bool persuingPlayer = DistanceFromPlayer < 3 && DistanceFromPlayer > 0.5f;

            if (changePatrolArea && !persuingPlayer)
                PickNewPatrolPoint();

            patrolTimer += 1f * Time.deltaTime;

            if (returningToPatrolPoint)
            {
                if (remainingDistance <= 1f)
                {
                    returningToPatrolPoint = false;
                }
            }

            if (DistanceFromPatrolPoint > maxRoamFromPatrolPoint)
            {
                returningToPatrolPoint = true;
            }

            if (agent.remainingDistance <= 3f && DistanceFromPlayer > 5 && DistanceFromPatrolPoint < maxRoamFromPatrolPoint && !returningToPatrolPoint)
            {
                // Reached destination
                FindNewLocation();
            }
            else if (persuingPlayer && !returningToPatrolPoint)
            {
                // Player is here!
                MoveTo(PlayerPosition);
            }
            else if (runTimer >= runTimeout)
            {
                // Couldn't reach destination in time
                FindNewLocation();
                //returningToPatrolPoint = true;
                runTimer = 0;
            }
        }
        else if(persuePlayerAfterHit && !flee)
        {
            persueTimer += 1f * Time.deltaTime;
            MoveTo(PlayerPosition);
            if (persueTimer >= persueTimeout)
            {
                persuePlayerAfterHit = false;
                persueTimer = 0;
            }
        } 
        else if(flee)
        {
            // Fuckin run for it. Put as much distance between you and the player
            if(agent.remainingDistance < 1f || DistanceFromPlayer >= 10)
            {
                flee = false;
            }
        }

        attackDelay += 1f * Time.deltaTime;

        // We are within attacking distance
        if (DistanceFromPlayer <= 0.5f)
        {
            agent.isStopped = true;
            if (attackDelay >= secondsBetweenAttacks)
            {
                Attack();
                attackDelay = 0;
            }
        }
        else
        {
            agent.isStopped = false;
        }

    }

    void PickNewPatrolPoint()
    {
        var point = RandomNavmeshLocation(roamRadius);
        Vector3 validated = GetNearestNavmeshLocation(point);
        patrolPoint = validated;
        patrolTimer = 0;
        patrolDurationChosen = Random.Range(patrolDurationMin, patrolDurationMax);
    }

    void FindNewLocation()
    {
        var point = RandomNavmeshLocation(patrolRadius);
        next = GetNearestNavmeshLocation(point);
        MoveTo(next);
    }

    public override void Attack()
    {
        Debug.Log("Do attack!");
        PlayerStats.instance.DamagePlayer(damage);
    }

    public override void OnHit()
    {
        Debug.Log("Notice player");
        bool getAggressive = Random.Range(0, chanceOfAgro) == 0;

        if (getAggressive)
        {
            persuePlayerAfterHit = true;
            persueTimer = 0;
        } else
        {
            flee = true;
            Vector3 direction = -transform.forward;
            Vector3 point = direction * maxFleeDistance;
            Vector3 validate = GetNearestNavmeshLocation(point);
            agent.SetDestination(validate);
            fleeTimer = 0;
        }
    }

    public override void DrawGizmos()
    {

    }

}
