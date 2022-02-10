using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomGrid
{
    public static Vector3Int cellSize = new Vector3Int(2, 2, 2);
    public RoomGridCell[,] cells;

    public Vector3Int dimensions;

    [Header("Debugging")]
    public bool drawGrid = true;
    public bool hideFreeSlots = false;
    public bool fillOccupiedSlots = false;

    public RoomGrid(Vector3Int dimensions)
    {
        this.dimensions = dimensions;
        cells = new RoomGridCell[dimensions.x, dimensions.z];
        for (int x = 0; x < dimensions.x; x++)
        {
            for (int z = 0; z < dimensions.z; z++)
            {
                cells[x, z] = new RoomGridCell();
                cells[x, z].position = new Vector3(x * cellSize.x, 0.5f, z * cellSize.z);
            }
        }
    }

    public void OnDrawGizmos(Transform transform)
    {
        if (!Application.isEditor)
            return;

        if (drawGrid)
        {
            for (int x = 0; x < dimensions.x; x++)
            {
                for (int z = 0; z < dimensions.z; z++)
                {
                    var cell = cells[x, z];

                    if (!hideFreeSlots)
                    {
                        Gizmos.color = cell.occupied ? Color.red : Color.green;
                        if (fillOccupiedSlots)
                            Gizmos.DrawCube(cell.position, cellSize);
                        else
                            Gizmos.DrawWireCube(cell.position, cellSize);
                    } else
                    {
                        if (cell.occupied)
                        {
                            Gizmos.color = Color.red;
                            if (fillOccupiedSlots)
                                Gizmos.DrawCube(cell.position, cellSize);
                            else
                                Gizmos.DrawWireCube(cell.position, cellSize);
                        }
                    }

                    if(cell.flag.Equals(RoomGridCell.CellFlag.TEST))
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawSphere(cell.position, 0.1f);
                    }

                    if (Vector3.Distance(UnityEditor.SceneView.currentDrawingSceneView.camera.transform.position, transform.position + cell.position) < 5f)
                    {
                        Gizmos.color = Color.white;
                        UnityEditor.Handles.Label(cell.position, $"{cell.distance}\n{Mathf.RoundToInt(cell.position.x / cellSize.x)}, {Mathf.RoundToInt(cell.position.y / cellSize.y)}, {Mathf.RoundToInt(cell.position.z / cellSize.z)}");
                    }
                }
            }
        }
    }
}
