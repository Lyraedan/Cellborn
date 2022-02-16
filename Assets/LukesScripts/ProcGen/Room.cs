using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Room
{
    public Vector3 centre;
    public List<GridCell> hallways = new List<GridCell>();
    public bool drawHallwayPath = false;
}