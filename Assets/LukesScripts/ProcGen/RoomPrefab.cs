using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class RoomPrefab
{

    public enum RoomPropType
    {
        FLOOR, WALL, PROP, DOOR, CORNER, ENTITY, BOSS, PLAYER
    }

    public GameObject prefab;
    public RoomPropType type = RoomPropType.PROP;

    public GameObject Spawn(Vector3 position, Vector3 rotation, bool placeOnNavmesh = false)
    {
        /*
        if(placeOnNavmesh)
        {
            NavMeshHit hit;
            var valid = NavMesh.SamplePosition(position, out hit, Mathf.Infinity, NavMesh.AllAreas);
            position = hit.position;
        }*/
        return RoomGenerator.instance.SpawnPrefab(prefab, position, rotation);
    }
}
