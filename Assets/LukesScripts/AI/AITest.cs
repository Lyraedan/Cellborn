using System.Collections;
using System.Collections.Generic;
using LukesScripts.AI;
using UnityEngine;

public class AITest : AI
{

    private float secondsBetweenAttacks = 1f;
    private float attackDelay = 0;
    public PlayerStats playerStats;
    private int damage = 1;

    public GameObject acid;
    public GameObject acidPoint;

    [SerializeField] private float remainingDistance = 0f;

    public override void Init()
    {

    }

    public override void Tick()
    {
        attackDelay += 1f * Time.deltaTime;
    }

    public override void Attack()
    {
        if (attackDelay >= secondsBetweenAttacks)
        {
            Debug.Log("Do attack!");
            Instantiate(acid, acidPoint.transform.position, Quaternion.identity);
            attackDelay = 0;
        }
    }

    public override void OnHit()
    {
        Debug.Log("Notice player");
    }

    public override void OnDeath()
    {

    }

    public override void DrawGizmos()
    {

    }
}
