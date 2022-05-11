using Bolt;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using EventHooks = LukesScripts.Blueprints.EventHooks;

namespace LukesScripts.AI
{
    public abstract class AI : MonoBehaviour
    {

        [SerializeField] protected NavMeshAgent agent;
        protected float runTimer = 0;
        [SerializeField] protected int runTimeout = 3;
        public Color minimapBlip = Color.red;
        protected GridCell currentCell, lastCell;
        [SerializeField] protected bool showPath = false;
        [SerializeField] protected float rotationDampening = 8f;
        public SkinnedMeshRenderer skinnedMeshRenderer;
        public Material defaultMaterial;
        public Material damagedMaterial;
        public AudioSource audioSource;
        public AudioClip idleSound, hitSound, deathSound;

        protected Vector3 OurPosition
        {
            get
            {
                return transform.position;
            }
            set
            {
                transform.position = value;
            }
        }

        protected Vector3 PlayerPosition
        {
            get
            {
                if (WeaponManager.instance == null)
                    return Vector3.zero;
                if (WeaponManager.instance.player == null)
                    return Vector3.zero;

                return WeaponManager.instance.player.transform.position;
            }
        }

        public Action<AI, GridCell, GridCell> OnMinimapUpdated;
        public bool isHit = false;

        protected bool IsOnNavmesh
        {
            get
            {
                if (agent == null)
                    return false;

                return agent.isOnNavMesh;
            }
        }

        public float DistanceFromPlayer
        {
            get
            {
                if (WeaponManager.instance == null)
                    return Mathf.Infinity;
                if (WeaponManager.instance.player == null)
                    return Mathf.Infinity;

                return Vector3.Distance(transform.position, WeaponManager.instance.player.transform.position);
            }
        }

        private void Start()
        {
            Minimap.instance.AddEntity(this);
            Init();
            CustomEvent.Trigger(gameObject, EventHooks.Init);
        }

        private void OnDestroy()
        {
            Minimap.instance.RemoveEntity(this);
        }

        private void Update()
        {
            if (!IsOnNavmesh)
                return;

            if (agent.remainingDistance > 1)
            {
                runTimer += 1f * Time.deltaTime;
            }
            currentCell = RoomGenerator.instance.navAgent.GetGridCellAt((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
            if (lastCell == null)
                lastCell = currentCell;

            Tick();
            CustomEvent.Trigger(gameObject, EventHooks.Tick);
            OnMinimapUpdated?.Invoke(this, currentCell, lastCell);
            lastCell = currentCell;
        }

        /// <summary>
        /// Called in Start
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// Called in Update
        /// </summary>
        public abstract void Tick();

        /// <summary>
        /// What happens when this AI attacks?
        /// </summary>
        public abstract void Attack();

        public void DoAttack()
        {
            Attack();
            CustomEvent.Trigger(gameObject, EventHooks.Attack);
        }

        public abstract void OnHit();

        public abstract void OnDeath();

        public abstract void DrawGizmos();

        public void Die()
        {
            int selected = UnityEngine.Random.Range(0, WeaponBag.instance.weaponBag.Count);
            var position = transform.position;
            GameObject drop = Instantiate(WeaponBag.instance.weaponBag[selected], position, Quaternion.identity);
            WeaponBag.instance.weaponBag.RemoveAt(selected);
            if (WeaponBag.instance.weaponBag.Count == 0)
            {
                WeaponBag.instance.RefillBag();
            }
            audioSource.clip = deathSound;
            audioSource.Play();
            OnDeath();
            CustomEvent.Trigger(gameObject, EventHooks.OnDeath);
        }

        public void Hit()
        {
            isHit = true;
            audioSource.clip = hitSound;
            audioSource.Play();
            OnHit();
            CustomEvent.Trigger(gameObject, EventHooks.OnHit);
            isHit = false;
            StartCoroutine(DamageIndicator());
            if (idleSound != null)
            {
                audioSource.clip = idleSound;
                audioSource.Play();
            }
        }

        IEnumerator DamageIndicator()
        {
            skinnedMeshRenderer.material = damagedMaterial;
            yield return new WaitForSeconds(0.1f);
            skinnedMeshRenderer.material = defaultMaterial;
        }

        public bool MoveTo(Vector3 position)
        {
            agent.SetDestination(position);
            if (agent.path.status != NavMeshPathStatus.PathComplete)
            {
                Debug.LogError("Invalid path");
                return false;
            }
            LookAt(position);
            return true;
        }

        public Vector3 RandomNavmeshLocation(float radius)
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

        public Vector3 GetNearestNavmeshLocation(Vector3 point)
        {
            NavMeshHit myNavHit;
            Vector3 pos = OurPosition;
            float radius = 10; //Mathf.Infinity;
            if (NavMesh.SamplePosition(point, out myNavHit, radius, -1))
            {
                pos = myNavHit.position;
            }
            return pos;
        }

        public void LookAt(Vector3 point)
        {
            var direction = point - transform.position;
            direction.y = 0;
            var rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationDampening);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (showPath)
            {
                if (agent.pathStatus.Equals(NavMeshPathStatus.PathComplete))
                {
                    if (agent.path.corners.Length >= 2)
                    {
                        Gizmos.color = Color.yellow;
                        for (int i = 0; i < agent.path.corners.Length - 1; i++)
                        {
                            Gizmos.DrawLine(agent.path.corners[i], agent.path.corners[i + 1]);
                            Gizmos.color = Color.red;
                            Gizmos.DrawSphere(agent.path.corners[i], 0.1f);
                        }
                    }
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(agent.destination, 0.1f);
                }
            }
            DrawGizmos();
            CustomEvent.Trigger(gameObject, EventHooks.DrawGizmos);
        }
#endif
    }
}