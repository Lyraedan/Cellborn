using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding
{
    public enum PathStatus
    {
        VALID, INVALID, FAILED
    }

    public Vector3 source, destination;
    public Vector3Int src, dest;

    public PathStatus status = PathStatus.INVALID;
    public List<RoomGridCell> open = new List<RoomGridCell>();
    public List<RoomGridCell> closed = new List<RoomGridCell>();
    private bool reachedDestination = false;

    [Header("Debugging")]
    public bool drawPath = false;
    public bool drawClosed = false;

    public void CalculatePath(Action<List<RoomGridCell>> OnValidPathCalculated)
    {
        src = RoomGenerator.instance.GetPositionAsGridSpace(new Vector3Int((int) source.x, (int) source.y, (int) source.z));
        dest = RoomGenerator.instance.GetPositionAsGridSpace(new Vector3Int((int) destination.x, (int) destination.y, (int) destination.z));

        if (src.x < 0 || src.z < 0 || dest.x < 0 || dest.z < 0 ||
           src.x >= RoomGenerator.instance.grid.dimensions.x || src.z >= RoomGenerator.instance.grid.dimensions.z ||
           dest.x >= RoomGenerator.instance.grid.dimensions.x || dest.z >= RoomGenerator.instance.grid.dimensions.z)
        {
            Debug.LogError("Path source or destination is out of bounds!\nSource: " + source.ToString() + "\nDestination: " + destination.ToString());
            return;
        }

        RoomGridCell start = RoomGenerator.instance.GetCellAt(src.x, src.z);
        RoomGridCell end = RoomGenerator.instance.GetCellAt(dest.x, dest.z);
        Debug.Log("Source: " + src.ToString() + "\nDestination:" + dest.ToString() + "\n\nMain Src: " + source + "\nMain Dest: " + destination);
        Score();
        Clear();
        reachedDestination = false;
        FindPath(OnValidPathCalculated);
        open.Add(end);
        Debug.Log("Found Path: " + status.ToString());
    }

    void FindPath(Action<List<RoomGridCell>> OnValidPathCalculated)
    {
        reachedDestination = CalculateValidPath();
        while(!reachedDestination)
        {
            reachedDestination = CalculateValidPath();
            if (status.Equals(PathStatus.FAILED))
                break;
        }

        if (status.Equals(PathStatus.FAILED))
            return;

        if(open.Count <= 0)
        {
            status = PathStatus.INVALID;
            return;
        }

        status = PathStatus.VALID;
        OnValidPathCalculated?.Invoke(open);
        foreach(RoomGridCell o in open)
        {
            Debug.Log("Open cell at " + o.ToString());
        }
    }

    bool CalculateValidPath()
    {
        RoomGridCell next = FindNeighbour(RoomGenerator.instance.GetCellAt((int)src.x, (int) source.z));
        if(next == null)
        {
            status = PathStatus.FAILED;
            Debug.LogError("Failed to find valid path!");
            return false;
        }

        Debug.Log("Next distance: " + next.distance);
        status = PathStatus.INVALID;

        while(next.distance > 2)
        {
            if(!closed.Contains(next))
            {
                open.Add(next);
                next = FindNeighbour(next);
            }

            if(open.Contains(next))
            {
                foreach(RoomGridCell cell in open)
                {
                    closed.Add(cell);
                }
                open.Clear();
                break;
            }
        }
        return next.distance >= 2;
    }

    void Score()
    {
        for(int x = 0; x < RoomGenerator.instance.grid.dimensions.x; x++)
        {
            for(int z = 0; z < RoomGenerator.instance.grid.dimensions.z; z++)
            {
                RoomGenerator.instance.grid.cells[x, z].SetDistance(dest);
            }
        }
    }

    void Clear()
    {
        open.Clear();
        closed.Clear();
    }

    public RoomGridCell FindNeighbour(RoomGridCell cell)
    {
        List<RoomGridCell> neighbours = new List<RoomGridCell>();
        for (int x = -1; x < 2; x++)
        {
            for (int z = -1; z < 2; z++)
            {
                RoomGridCell next = RoomGenerator.instance.GetCellAt(x, z);
                if (next != null)
                {
                    neighbours.Add(next);
                }
            }
        }

        neighbours.Sort((a, b) =>
        {
            return a.distance.CompareTo(b.distance);
        });

        if (neighbours.Count == 0)
            return null;

        return neighbours[0];
    }

    public void DrawGizmos()
    {
        if (!status.Equals(PathStatus.VALID))
            return;

        // Only draw a valid path
        if(drawPath)
        {
            Gizmos.color = Color.yellow;
            foreach(RoomGridCell walk in open)
            {
                Gizmos.DrawSphere(walk.position, 0.1f);
            }

            if(drawClosed)
            {
                Gizmos.color = Color.red;
                foreach(RoomGridCell block in closed)
                {
                    Gizmos.DrawSphere(block.position, 0.1f);
                }
            }

            Gizmos.color = Color.gray;
            Gizmos.DrawSphere(source, 0.1f);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(destination, 0.1f);
        }
    }
}
