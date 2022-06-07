using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour
{
    void Start()
    {
        GridCell cell = RoomGenerator.instance.navAgent.GetGridCellAt((int) transform.position.x, 0, (int) transform.position.z);
        cell.flag = GridCell.GridFlag.PROP;
    }

}
