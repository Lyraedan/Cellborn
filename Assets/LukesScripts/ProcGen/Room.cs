using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Room
{
    public List<RoomTile> tiles = new List<RoomTile>();
    public RoomTile center { get {
            var tile = tiles[tiles.Count / 2];
            return tile;
        }
    }

    public void SpawnTiles()
    {
        for(int i = 0; i < tiles.Count; i++)
        {
            var cell = tiles[i].cell;
            RoomGenerator.instance.SpawnPrefab(RoomGenerator.instance.test, cell.position, Vector3.zero);
        }
    }
}
