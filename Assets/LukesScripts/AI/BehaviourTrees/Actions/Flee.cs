using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LukesScripts.AI.Actions
{
    public class Flee : Action
    {
        public float fleeDistance = 3f;
        public SharedFloat speed = 3f;
        public SharedFloat arriveDistance = 1f;

        protected NavMeshAgent agent;

        public override void OnStart()
        {
            agent = GetComponent<NavMeshAgent>();
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
                agent.isStopped = true;
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }

        bool SetDestination()
        {
            Vector3 away = -transform.forward * fleeDistance;
            Vector3 destination = GetNearestNavmeshLocation(away);

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

            return remainingDistance <= arriveDistance.Value;
        }

        bool IsOnNavmesh()
        {
            return agent.isOnNavMesh;
        }

        public Vector3 GetNearestNavmeshLocation(Vector3 point)
        {
            NavMeshHit myNavHit;
            Vector3 pos = transform.position;
            float radius = 10; //Mathf.Infinity;
            if (NavMesh.SamplePosition(point, out myNavHit, radius, -1))
            {
                pos = myNavHit.position;
            }
            return pos;
        }

        void Stop()
        {
            if (agent.hasPath)
            {
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5
                navMeshAgent.Stop();
#else
                agent.isStopped = true;
#endif
            }
        }
    }
}
