using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Arena : MonoBehaviour
{
    public NavMeshSurface navmesh;
    public Transform playerSpawn;
    public Transform wizardSpawn;

    public GameObject floor, wall, ceiling;
    public MeshRenderer ceilingMesh;

    private void Start()
    {
        if(SuperSecret.secretEnabled)
        {
            floor.layer = LayerMask.NameToLayer("Default");
            wall.layer = LayerMask.NameToLayer("Default");
            ceiling.layer = LayerMask.NameToLayer("Default");
            ceilingMesh.enabled = true;
        }
    }
}
