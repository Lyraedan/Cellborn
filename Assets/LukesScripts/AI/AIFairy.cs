using LukesScripts.AI;
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

    public GameObject projectile;
    public Transform firePoint;

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

    }

    public override void Tick()
    {
        attackDelay += 1f * Time.deltaTime;
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
        var direction = PlayerStats.instance.transform.position - transform.position;
        direction.y = 0;
        var rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationDampening);

        if (attackDelay >= secondsBetweenAttacks)
        {
            Debug.Log("Do attack!");
            //PlayerStats.instance.DamagePlayer(damage);

            GameObject projInstance = Instantiate(projectile, firePoint.position, firePoint.rotation);
            var projScript = projInstance.GetComponent<ProjectileBehaviour>();
            projInstance.GetComponent<Rigidbody>().AddForce((transform.forward * projScript.throwStrength) + (transform.up * projScript.arcSize), ForceMode.Impulse);

            attackDelay = 0;
        }
    }

    public override void OnHit()
    {
        Debug.Log("Notice player");
        /*
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
        */
    }

    public override void OnDeath()
    {
        audioSource.clip = deathSound;
        audioSource.Play();
    }

    public override void DrawGizmos()
    {

    }

}
