using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace LukesScripts.AI.Actions
{
    public class GoToMuffin : Action
    {
        public SharedFloat speed = 3f;

        protected NavMeshAgent agent;
        private AI ai;
        private GameObject nearest;

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

            HeadToMuffin();
        }

        public override void OnReset()
        {
            Stop();
            HeadToMuffin();
        }

        public override void OnEnd()
        {
            Stop();
            HeadToMuffin();
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

            HeadToMuffin();
            return TaskStatus.Running;
        }

        void HeadToMuffin()
        {
            nearest = GetNearestMuffin();
            if (nearest != null)
            {
                ai.LookAt(nearest.transform.position);
                SetDestination();
            }
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
            if (nearest == null)
                return Vector3.zero;

            return nearest.transform.position;
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

        public GameObject GetNearestMuffin()
        {
            var muffins = GameObject.FindObjectsOfType<Muffin>().ToList();
            if (muffins.Count > 0)
            {
                muffins.Sort((a, b) =>
                {
                    return (int)Vector3.Distance(a.transform.position, transform.position);
                });
                return muffins[0].gameObject;
            }
            else
                return null;
        }
    }
}