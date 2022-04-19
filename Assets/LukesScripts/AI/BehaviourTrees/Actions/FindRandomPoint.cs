using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LukesScripts.AI.Actions
{
    public class FindRandomPoint : Action
    {
        public SharedFloat radius = 3f;
        public SharedFloat speed = 3f;
        public SharedFloat arriveDistance = 1f;
        public float playerThreashold;

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

            float distance = Vector3.Distance(transform.position, WeaponManager.instance.player.transform.position);
            if (distance <= playerThreashold)
            {
                Stop();
                return TaskStatus.Failure;
            }

            return TaskStatus.Running;
        }

        bool SetDestination()
        {
            Vector3 destination = Target(radius.Value);
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

        private Vector3 Target(float radius)
        {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
            randomDirection += transform.position;
            NavMeshHit hit;
            Vector3 finalPosition = Vector3.zero;
            if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
            {
                finalPosition = hit.position;
            }
            return finalPosition;
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