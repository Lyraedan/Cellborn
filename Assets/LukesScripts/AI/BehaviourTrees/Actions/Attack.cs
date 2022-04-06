using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Attack : Action
{
    private AI ai;
    private NavMeshAgent agent;

    public override void OnStart()
    {
        ai = GetComponent<AI>();
        agent = GetComponent<NavMeshAgent>();
    }

    public override TaskStatus OnUpdate()
    {
        ai.Attack();
        return TaskStatus.Success;
    }

}
