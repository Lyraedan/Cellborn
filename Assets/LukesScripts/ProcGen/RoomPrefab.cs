using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomPrefab
{

    public enum RoomPropType
    {
        FLOOR, WALL, PROP, DOOR
    }

    public GameObject prefab;
    public float angle = 0;
    public RoomPropType type = RoomPropType.PROP;

    public GameObject Spawn(Vector3 position)
    {
        return RoomGenerator.instance.SpawnPrefab(prefab, position, new Vector3(angle, 0, 0));
    }
}
