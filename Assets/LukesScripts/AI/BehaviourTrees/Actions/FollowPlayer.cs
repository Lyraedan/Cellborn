using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace LukesScripts.AI.Actions
{
    public class FollowPlayer : Action
    {
        public SharedFloat speed = 3f;
        public float muffinThreashold = 20f;

        protected NavMeshAgent agent;
        private AI ai;
        public float playerThreashold;

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

            var nearestMuffin = GetNearestMuffin();
            if(nearestMuffin != null)
            {
                float distance = Vector3.Distance(transform.position, nearestMuffin.transform.position);
                if(distance <= muffinThreashold)
                {
                    Stop();
                    return TaskStatus.Failure;
                }
            }

            float playerDistance = Vector3.Distance(transform.position, WeaponManager.instance.player.transform.position);
            if (playerDistance <= playerThreashold)
            {
                Stop();
                return TaskStatus.Failure;
            }

            ai.LookAt(WeaponManager.instance.player.transform.position);
            SetDestination();
            return TaskStatus.Running;
        }

        bool SetDestination()
        {
            try
            {
                Vector3 destination = Target();
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5
            agent.Resume();
#else
                agent.isStopped = false;
#endif
                return agent.SetDestination(destination);
            } catch(System.Exception e)
            {
                return false;
            }
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

        public GameObject GetNearestMuffin()
        {
            var muffins = GameObject.FindObjectsOfType<Muffin>().ToList();
            if (muffins.Count > 0)
            {
                muffins.Sort((a, b) =>
                {
                    return a.DistanceFrom(transform.position).CompareTo(b.DistanceFrom(transform.position));
                });
                return muffins[0].gameObject;
            }
            else
                return null;
        }
    }
}