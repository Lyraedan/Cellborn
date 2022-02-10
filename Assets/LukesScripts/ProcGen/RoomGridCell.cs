using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomGridCell
{
    public enum CellFlag
    {
        FLOOR, OBSTACLE, DOOR, FREE, EDGE, CORNER, TEST
    }

    public CellFlag flag = CellFlag.FREE;
    public Vector3 position;
    public bool occupied = false;
    public float distance;

    public void SetDistance(Vector3 target)
    {
        this.distance = Mathf.Abs(target.x - position.x) + Mathf.Abs(target.y - position.y) + Mathf.Abs(target.z - position.z);
    }

    public override string ToString()
    {
        return $"Flag: {flag.ToString()}\n" +
               $"Position: {position.ToString()}\n" +
               $"Occupied: {occupied}\n" +
               $"Distance: {distance}";
    }

}
