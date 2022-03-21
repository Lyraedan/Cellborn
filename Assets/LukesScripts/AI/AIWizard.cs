using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIWizard : AI
{

    Vector3 next;
    [SerializeField] private Vector3 bindingPoint;
    [SerializeField] private GameObject projectile;
    [SerializeField] private float roamRadius = 3;
    [SerializeField] private float bulletForce = 10.0f;

    [SerializeField] private float secondsBetweenAttacks = 2.5f;
    private float attackDelay = 0;

    public override void Init()
    {

    }

    public override void Tick()
    {
        // To far away from spawn point
        if (agent.remainingDistance <= 1 && DistanceFromPlayer > 5)
        {
            // Reached destination
            FindNewLocation();
        }
        else if (Vector3.Distance(transform.position, bindingPoint) > 20)
        {
            MoveTo(bindingPoint);
        }

        attackDelay += 1f * Time.deltaTime;
        if (DistanceFromPlayer < 10)
        {
            if (attackDelay >= secondsBetweenAttacks)
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
                var bullet = Instantiate(projectile, transform.position + dir, Quaternion.identity);
                Rigidbody body = bullet.GetComponent<Rigidbody>();
                body.AddForce(dir * bulletForce);
            }
        }
    }

    public override void DrawGizmos()
    {
    }
}
