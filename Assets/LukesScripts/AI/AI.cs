using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class AI : MonoBehaviour
{

    [SerializeField] protected NavMeshAgent agent;
    protected float runTimer = 0;
    protected int runTimeout = 3;

    protected bool IsOnNavmesh {
        get {
            Vector3 position = transform.position;
            NavMeshHit hit;
            float meshThreshold = 3;
            if(NavMesh.SamplePosition(position, out hit, meshThreshold, NavMesh.AllAreas))
            {
                if(Mathf.Approximately(position.x, hit.position.x) && Mathf.Approximately(position.z, hit.position.z))
                {
                    return position.y > hit.position.y;
                }
            }
            return false;
        }
    }

    protected float DistanceFromPlayer
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
        Init();
    }

    private void Update()
    {
        if (!IsOnNavmesh)
            return;

        if (agent.remainingDistance > 1)
        {
            runTimer += 1f * Time.deltaTime;
        }
        Tick();
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

    public abstract void DrawGizmos();

    public void MoveTo(Vector3 position)
    {
        agent.SetDestination(position);
    }

    public Vector3 RandomNavmeshLocation(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }
        return finalPosition;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        DrawGizmos();
    }
#endif

}
