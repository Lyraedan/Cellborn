using System.Collections;
using System.Collections.Generic;
using LukesScripts.AI;
using UnityEngine;

public class AIWizard : AI
{

    Vector3 next;
    public Vector3 bindingPoint;
    [SerializeField] private GameObject projectile;
    [SerializeField] private float roamRadius = 3;
    [SerializeField] private float bulletForce = 10.0f;
    public EnemyScript enemyScript;

    [SerializeField] private float secondsBetweenAttacks = 2.5f;
    private float attackDelay = 0;

    private bool persuePlayerAfterHit = false;
    private float persueTimer = 0f;
    private float persueTimeout = 10f;

    [SerializeField] private int chanceOfWaiting = 3;
    [Tooltip("How long will the AI wait if it chooses to.")]  [SerializeField] private float waitDuration = 4f;
    private float waitTimer = 0;
    private bool returningToRoom = false;
    private bool wait = false;

    [Tooltip("Threashold on how long the entity will try to reach the destination before giving up.")] [SerializeField] private int giveUpThreashold = 10;
    private float travelTime = 0f;

    private float minRadius = 3f;
    private float maxRadius = 20f;

    public override void Init()
    {

    }

    private void OnDestroy()
    {
        if(PlayerStats.instance != null)
            PlayerStats.instance.win.gameObject.SetActive(true);
    }

    public override void Tick()
    {
        if (!IsOnNavmesh)
        {
            var validated = GetNearestNavmeshLocation(OurPosition);
            OurPosition = validated;
            return;
        }

        if (enemyScript.currentHP <= (enemyScript.maxHP / 3) * 2)
        {
            bulletForce = 200;
            secondsBetweenAttacks = 0.5f;
        }
        else if (enemyScript.currentHP <= enemyScript.maxHP / 3)
        {
            bulletForce = 300;
            secondsBetweenAttacks = 0.25f;
        }
        else
        {
            bulletForce = 100;
            secondsBetweenAttacks = 1f;
        }

        if (!persuePlayerAfterHit)
        {
            if(agent.remainingDistance <= 1f)
            {
                travelTime += 1f * Time.deltaTime;
            }
            // To far away from spawn point
            if (agent.remainingDistance <= 1f && DistanceFromPlayer > 5 && !returningToRoom && !wait)
            {
                // Reached destination
                wait = Random.Range(0, chanceOfWaiting) == 0;
                FindNewLocation();
            }
            else if (Vector3.Distance(transform.position, bindingPoint) > maxRadius && !returningToRoom && !wait)
            {
                returningToRoom = true;
                MoveTo(bindingPoint);
            } else if(Vector3.Distance(transform.position, bindingPoint) <= minRadius && returningToRoom && !wait)
            {
                returningToRoom = false;
            }
        } else
        {
            persueTimer += 1f * Time.deltaTime;
            MoveTo(WeaponManager.instance.player.transform.position);
            if (persueTimer >= persueTimeout)
            {
                persuePlayerAfterHit = false;
                persueTimer = 0;
            }
        }

        if(wait)
        {
            if(agent.remainingDistance <= 1)
            {
                agent.isStopped = true;
                waitTimer += 1f * Time.deltaTime;
                if(waitTimer >= waitDuration)
                {
                    agent.isStopped = false;
                    wait = false;
                    waitTimer = 0;
                }
            }
        }

        attackDelay += 1f * Time.deltaTime;
        if (DistanceFromPlayer < 10)
        {
            wait = false;
            if (attackDelay >= secondsBetweenAttacks)
            {
                DoAttack();
                attackDelay = 0;
            }
        }
    }

    void FindNewLocation()
    {
        next = RandomNavmeshLocation(roamRadius);
        MoveTo(next);
        travelTime = 0f;
    }

    public override void Attack()
    {
        /*
       a   b
        a b
         *
        c d
       c   d
         */
        // Projectile directions
        Vector3 a = transform.forward + -transform.right;
        Vector3 b = transform.forward + transform.right;
        Vector3 c = -transform.forward + -transform.right;
        Vector3 d = -transform.forward + transform.right;

        Vector3[] directions = new Vector3[4] { a, b, c, d };
        int attackCount = 5;
        for (int i = 0; i < attackCount; i++)
        {
            for (int j = 0; j < directions.Length; j++)
            {
                Vector3 dir = directions[j];
                var bullet = Instantiate(projectile, (transform.position + dir)*(i+1), Quaternion.identity);
                Rigidbody body = bullet.GetComponent<Rigidbody>();
                body.AddForce(dir * bulletForce);
            }
        }
    }


    public override void OnHit()
    {
        Debug.Log("Notice player");
        persuePlayerAfterHit = true;
        persueTimer = 0;
        travelTime = 0;
    }

    public override void OnDeath()
    {
        
    }

    public override void DrawGizmos()
    {

    }

}
