using System.Collections;
using System.Collections.Generic;
using Bolt;
using LukesScripts.AI;
using UnityEngine;
using EventHooks = LukesScripts.Blueprints.EventHooks;

public class AIWizard : AI
{

    private float secondsBetweenAttacks = 2.8f;
    private float attackDelay = 0;

    public override void Init()
    {

    }

    private void OnDestroy()
    {
        if(PlayerStats.instance != null)
            PlayerStats.instance.winGroup.SetActive(true);
    }

    public override void Tick()
    {
        attackDelay += 1f * Time.deltaTime;

    }

    public override void Attack()
    {
        // You need to do this in blueprints... how?
        if (attackDelay >= secondsBetweenAttacks)
        {
            Debug.Log("Do attack!");
            CustomEvent.Trigger(gameObject, EventHooks.DelayedAttack);
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