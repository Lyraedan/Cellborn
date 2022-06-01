using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class RoomPrefab
{

    public enum RoomPropType
    {
        FLOOR, WALL, WALL_PROP, CENTER_PROP, DOOR, CORNER, ENTITY, BOSS, PLAYER, WALL_LIGHT, CEILING_LIGHT, WALL_DECOR, LITTER
    }

    public GameObject prefab;
    public RoomPropType type = RoomPropType.WALL_PROP;

    public GameObject Spawn(Vector3 position, Vector3 rotation)
    {
        return RoomGenerator.instance.SpawnPrefab(prefab, position, rotation);
    }
}
