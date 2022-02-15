using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://dotnetcoretutorials.com/2020/07/25/a-search-pathfinding-algorithm-in-c/
public class GradedPath : MonoBehaviour
{
    public Grid grid;

    private enum PathStatus
    {
        VALID, INVALID, FAILED
    }

    public Vector3 src;
    public Vector3 dest;

    [SerializeField] private PathStatus pathStatus = PathStatus.INVALID;
    public List<GridCell> open = new List<GridCell>();
    public List<GridCell> closed = new List<GridCell>();
    [SerializeField] private bool reachedDestination = false;

    [Header("Debug tools")]
    [SerializeField] private bool drawPath = true;
    [SerializeField] private bool drawBlocked = true;
    [SerializeField] private bool drawOccupiedSpace = true;

    public void CalculatePath()
    {
        if(!grid.ready)
        {
            Debug.LogError($"[{gameObject.name}] Navigation grid is not ready!");
            return;
        }

        if (src.x < 0 || src.y < 0 || src.z < 0 || dest.x < 0 || dest.y < 0 || dest.z < 0 ||
            src.x >= grid.cells.x || src.y >= grid.cells.y || src.z >= grid.cells.z || dest.x >= grid.cells.x || dest.y >= grid.cells.y || dest.z >= grid.cells.z)
        {
            Debug.LogWarning($"[{gameObject.name}] Source or destination out of bounds!\nSource: " + src + "\nDestination: " + dest + "\nGrid: " + grid.cells);
            return;
        }

        GridCell source = GetGridCellAt((int) src.x, (int) src.y, (int) src.z);
        GridCell destination = GetGridCellAt((int) dest.x, (int) dest.y, (int) dest.z);
        ScoreGrid();
        Clear();
        reachedDestination = false;
        FindPath();
        // Add the destination to our path list
        open.Add(destination);
    }

    void Clear()
    {
        open.Clear();
        closed.Clear();
    }

    void ScoreGrid()
    {
        for (int x = 0; x < grid.cells.x; x++)
        {
            for (int y = 0; y < grid.cells.y; y++)
            {
                for (int z = 0; z < grid.cells.z; z++)
                {
                    grid.grid[x, y, z].SetDistance(dest);
                }
            }
        }
    }

    void FindPath()
    {
        reachedDestination = CalculateValidPath();
        while (!reachedDestination)
        {
            reachedDestination = CalculateValidPath();
            // If we fail to reach the destination
            if (pathStatus.Equals(PathStatus.FAILED))
                break;
        }

        // If it failed
        if (pathStatus.Equals(PathStatus.FAILED))
            return;

        pathStatus = PathStatus.VALID;
    }

    bool CalculateValidPath()
    {
        GridCell next = FindNeighbour(GetGridCellAt((int)src.x, (int)src.y, (int)src.z));
        if (next == null)
        {
            pathStatus = PathStatus.FAILED;
            return false;
        }

        pathStatus = PathStatus.INVALID;
        //Locking up unity
        while (next.distance != 0)
        {
            if (!closed.Contains(next))
            {
                open.Add(next);
                next = FindNeighbour(next);
            }

            if (open.Contains(next))
            {
                // We hit a spot where it can't progress
                foreach (GridCell cell in open)
                {
                    closed.Add(cell);
                }
                open.Clear();
                break;
            }
        }
        return next.distance <= 1;
    }

    GridCell FindNeighbour(GridCell start)
    {
        List<GridCell> neighbours = new List<GridCell>();
        // Get neighbours
        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                for (int z = -1; z < 2; z++)
                {
                    GridCell next = GetGridCellAt((int)start.position.x + x, (int)start.position.y + y, (int)start.position.z + z);
                    if (next != null)
                    {
                        var isFree = next.flag.Equals(GridCell.GridFlag.WALKABLE) || next.flag.Equals(GridCell.GridFlag.OCCUPIED);
                        if (isFree && !closed.Contains(next))
                            neighbours.Add(next);
                    }
                }
            }
        }

        neighbours.Sort((a, b) =>
        {
            return a.distance.CompareTo(b.distance);
        });

        if (neighbours.Count == 0)
            return null;

        // Return the closest neighbour to the destination
        return neighbours[0];
    }

    public GridCell GetGridCellAt(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0 || x >= grid.cells.x || y >= grid.cells.y || z >= grid.cells.z)
            return null;

        return grid.grid[x, y, z];
    }

    public bool IsValidAt(int x, int y, int z)
    {
        return grid.grid[x, y, z].flag.Equals(GridCell.GridFlag.WALKABLE) || grid.grid[x, y, z].flag.Equals(GridCell.GridFlag.OCCUPIED);
    }

    public bool PathWasSuccessful()
    {
        return !pathStatus.Equals(PathStatus.FAILED);
    }

    public Vector3 PositionAsGridCoordinates()
    {
        var x = Mathf.RoundToInt(transform.position.x / grid.cellSize.x);
        var y = Mathf.RoundToInt(transform.position.y / grid.cellSize.y);
        var z = Mathf.RoundToInt(transform.position.z / grid.cellSize.z);
        return new Vector3(x, y, z);
    }

    private void OnDrawGizmos()
    {
        if(grid.drawAllPaths)
            DrawPathGizmo();
    }

    private void OnDrawGizmosSelected()
    {
        if(!grid.drawAllPaths)
            DrawPathGizmo();
    }

    void DrawPathGizmo()
    {
        if (reachedDestination)
        {
            if (drawPath)
            {
                Gizmos.color = Color.yellow;
                foreach (GridCell walk in open)
                {
                    Gizmos.DrawSphere(walk.position, 0.1f);
                }
            }

            if (drawBlocked)
            {
                Gizmos.color = Color.red;
                foreach (GridCell block in closed)
                {
                    Gizmos.DrawSphere(block.position, 0.1f);
                }
            }
        }
        Gizmos.color = Color.grey;
        Gizmos.DrawSphere(src, 0.1f);


        Gizmos.color = Color.green;
        Gizmos.DrawSphere(dest, 0.1f);

        // Draw grid space agent is occupying
        if (drawOccupiedSpace)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(PositionAsGridCoordinates(), Vector3.one);
        }
    }
}
