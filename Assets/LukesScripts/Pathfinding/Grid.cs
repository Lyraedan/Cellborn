using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public static Grid instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public Vector3 cellSize = Vector3.one;
    /// <summary>
    /// The size of the grid
    /// </summary>
    public Vector3 cells = new Vector3(5, 5, 5);
    public GridCell[,,] grid;

    public bool drawWalkable = true;
    public bool drawWalls = true;
    public bool drawOccupied = true;
    public bool drawBlocks = true;
    public bool drawHallways = true;
    public bool drawDistances = true;
    public bool drawAllPaths = false;
    public bool drawEntitySpawns = false;

    [HideInInspector] public bool ready = false;

    public void Init()
    {
        grid = new GridCell[(int)cells.x, (int)cells.y, (int)cells.z];
        for (int x = 0; x < cells.x; x++)
        {
            for (int y = 0; y < cells.y; y++)
            {
                for (int z = 0; z < cells.z; z++)
                {
                    grid[x, y, z] = new GridCell();
                    grid[x, y, z].position = new Vector3(transform.position.x + (x * cellSize.x), transform.position.y + (y * cellSize.y), transform.position.z + (z * cellSize.z));
                }
            }
        }
        Bake();
        ready = true;
    }

    void Bake()
    {
        for(int x = 0; x < cells.x; x++)
        {
            for(int y = 0; y < cells.y; y++)
            {
                for(int z = 0; z < cells.z; z++)
                {
                    grid[x, y, z].flag = Physics.CheckSphere(grid[x, y, z].position, 0.5f) ? GridCell.GridFlag.NONWALKABLE : GridCell.GridFlag.WALKABLE;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if(grid != null)
        {
            for(int x = 0; x < cells.x; x++)
            {
                for(int y = 0; y < cells.y; y++)
                {
                    for(int z = 0; z < cells.z; z++)
                    {
                        if (grid[x, y, z].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            if (drawWalkable)
                            {
                                Gizmos.color = Color.green;
                                Gizmos.DrawWireCube(grid[x, y, z].position, cellSize);
                            }
                        } else if(grid[x, y, z].flag.Equals(GridCell.GridFlag.NONWALKABLE))
                        {
                            if (drawBlocks)
                            {
                                Gizmos.color = Color.red;
                                Gizmos.DrawCube(grid[x, y, z].position, cellSize);
                            }
                        } else if(grid[x, y, z].flag.Equals(GridCell.GridFlag.OCCUPIED))
                        {
                            if (drawOccupied)
                            {
                                Gizmos.color = Color.red;
                                Gizmos.DrawWireCube(grid[x, y, z].position, cellSize);
                            }
                        } else if(grid[x, y, z].flag.Equals(GridCell.GridFlag.HALLWAY))
                        {
                            if (drawHallways)
                            {
                                Gizmos.color = Color.yellow;
                                Gizmos.DrawCube(grid[x, y, z].position, cellSize);
                            }
                        } else if (grid[x, y, z].flag.Equals(GridCell.GridFlag.WALL))
                        {
                            if (drawWalls)
                            {
                                Gizmos.color = Color.magenta;
                                Gizmos.DrawCube(grid[x, y, z].position, cellSize);
                            }
                        } else if (grid[x, y, z].flag.Equals(GridCell.GridFlag.CORNER))
                        {
                            if (drawWalls)
                            {
                                Gizmos.color = Color.blue;
                                Gizmos.DrawCube(grid[x, y, z].position, cellSize);
                            }
                        }
                        if (drawDistances)
                        {
                            Gizmos.color = Color.white;
                            Handles.Label(grid[x, y, z].position, $"{grid[x, y, z].distance}");
                        }
                        if(drawEntitySpawns)
                        {
                            Gizmos.color = Color.cyan;
                            Gizmos.DrawSphere(grid[x, y, z].position, 0.25f);
                        }
                    }
                }
            }
        }
    }
}

[System.Serializable]
public class GridCell
{
    public enum GridFlag {
        WALKABLE, NONWALKABLE, OCCUPIED, HALLWAY, WALL, CORNER
    }

    public GridFlag flag = GridFlag.WALKABLE;
    public Vector3 position = Vector3.zero;
    public Vector3 rotation = Vector3.zero;
    public bool hasEntity = false;
    [HideInInspector] public float score = 0;
    [HideInInspector] public float distance = 0;
    public float scoredDistance => score + distance;

    public void SetDistance(Vector3 target)
    {
        this.distance = Mathf.Abs(target.x - position.x) + Mathf.Abs(target.y - position.y) + Mathf.Abs(target.z - position.z);
    }
}
