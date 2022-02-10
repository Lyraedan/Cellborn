using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGridCell
{
    public enum CellFlag
    {
        FLOOR, OBSTACLE, DOOR
    }

    public CellFlag flag = CellFlag.FLOOR;
    public Vector3 position;
    public bool occupied = false;
    public float distance;

    public void SetDistance(Vector3 target)
    {
        this.distance = Mathf.Abs(target.x - position.x) + Mathf.Abs(target.y - position.y) + Mathf.Abs(target.z - position.z);
    }
}
