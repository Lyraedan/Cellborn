using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour
{

    public GridCell cell;

    public void BakePropCell()
    {
        var gridCellCoords = RoomGenerator.instance.navAgent.PositionAsGridCoordinates(transform.position);
        cell = RoomGenerator.instance.navAgent.GetGridCellAt((int)transform.position.x, 0, (int)transform.position.z);

        if (cell.flag.Equals(GridCell.GridFlag.OCCUPIED))
            cell.flag = GridCell.GridFlag.PROP;
        else if (cell.flag.Equals(GridCell.GridFlag.WALL))
            cell.flag = GridCell.GridFlag.WALL_PROP;
        else if (cell.flag.Equals(GridCell.GridFlag.WALKABLE))
        {
            cell = RoomGenerator.instance.floorMesh.GetClosestEdgeAt(transform.position).cell;
            cell.flag = GridCell.GridFlag.WALL_PROP;
        }

        // After all the checks fuckin remove
        if (cell.flag.Equals(GridCell.GridFlag.OCCUPIED))
            Remove();
    }

    public void Remove()
    {
        Debug.LogError("Removing prop");
        Destroy(gameObject);
    }

}
