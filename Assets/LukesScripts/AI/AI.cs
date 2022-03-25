using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class AI : MonoBehaviour
{

    [SerializeField] protected NavMeshAgent agent;
    protected float runTimer = 0;
    [SerializeField] protected int runTimeout = 3;
    public Color minimapBlip = Color.red;
    protected GridCell currentCell, lastCell;
    [SerializeField] protected bool showPath = false;
    [SerializeField] protected float rotationDampening = 8f;

    public Action<AI, GridCell, GridCell> OnMinimapUpdated;

    protected bool IsOnNavmesh {
        get {
            if (agent == null)
                return false;

            return agent.isOnNavMesh;
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
        Minimap.instance.AddEntity(this);
        Init();
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

    public abstract void DrawGizmos();

    public bool MoveTo(Vector3 position)
    {
        agent.SetDestination(position);
        if(agent.path.status != NavMeshPathStatus.PathComplete)
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
            }
        }
        DrawGizmos();
    }
#endif
}
