using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FollowPlayer : Action
{
    public SharedFloat speed = 3f;

    protected NavMeshAgent agent;
    private AI ai;

    public override void OnStart()
    {
        agent = GetComponent<NavMeshAgent>();
        ai = GetComponent<AI>();

        agent.speed = speed.Value;

        #if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5
            agent.Resume();
        #else
            agent.isStopped = false;
        #endif

        SetDestination();
    }

    public override void OnReset()
    {
        Stop();
        SetDestination();
    }

    public override void OnEnd()
    {
        Stop();
        SetDestination();
    }

    public override TaskStatus OnUpdate()
    {
        if (!IsOnNavmesh())
            return TaskStatus.Failure;

        if (HasArrived())
        {
            Stop();
            return TaskStatus.Success;
        }

        ai.LookAt(WeaponManager.instance.player.transform.position);
        SetDestination();
        return TaskStatus.Running;
    }

    bool SetDestination()
    {
        Vector3 destination = Target();
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5
            agent.Resume();
#else
        agent.isStopped = false;
#endif
        return agent.SetDestination(destination);
    }

    bool HasArrived()
    {
        // The path hasn't been computed yet if the path is pending.
        float remainingDistance;
        if (agent.pathPending)
        {
            remainingDistance = float.PositiveInfinity;
        }
        else
        {
            remainingDistance = agent.remainingDistance;
        }

        return remainingDistance <= 1f;
    }

    bool IsOnNavmesh()
    {
        return agent.isOnNavMesh;
    }

    private Vector3 Target()
    {
        return WeaponManager.instance.player.transform.position;
    }

    void Stop()
    {
        if (agent.hasPath)
        {
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5
                agent.Stop();
#else
            agent.isStopped = true;
#endif
        }
    }
}
