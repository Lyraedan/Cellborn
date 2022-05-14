using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Room
{
    public Vector3 start, end;
    public List<Vector3> centres = new List<Vector3>();
    public Vector3Int gridCentre;
    public List<GridCell> occupied = new List<GridCell>();
    public List<GridCell> walls = new List<GridCell>();
    public List<GridCell> hallways = new List<GridCell>();
    public bool drawHallwayPath = false;
    public Color indicatorColor = Color.red;

    public bool Intersects(Room room)
    {
        return start.x < room.end.x &&
               end.x > room.start.x &&
               start.z < room.end.z &&
               end.z > room.start.z;
    }

    public void DrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(start, 0.25f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(end, 0.25f);
        Gizmos.color = indicatorColor;
        for (int i = 0; i < centres.Count; i++)
        {
            var top = centres[i] + (Vector3.up * 10f);
            Gizmos.DrawLine(centres[i], top);
            Gizmos.DrawSphere(top, 0.25f);
        }
    }
}