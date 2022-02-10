using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public static RoomGenerator instance;

    public Transform roomParent;
    public RoomGrid grid;

    public Vector3Int start, end;

    public float rotationTest = 45;
    public GameObject test;

    public int maxRoomLimit = 10;

    private int failedAttempts = 0;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        start = GenerateRandomVector(0, 0, 0, 10, 1, 10);
        end = GenerateRandomVector(start.x, 0, start.z, 100, 1, 100);

        //var distance = end - start;
        Debug.Log("End: " + end.ToString());

        grid = new RoomGrid(new Vector3Int(end.x, 1, end.z));
        ScoreGrid();

        for(int i = 0; i < maxRoomLimit; i++)
        {
            GenerateRandomRoom();
        }
    }

    bool GenerateRandomRoom()
    {
        var posX = Mathf.RoundToInt(Random.Range(0, end.x));
        var posZ = Mathf.RoundToInt(Random.Range(0, end.z));
        var position = new Vector3Int(posX, 1, posZ);

        var sizeX = Mathf.RoundToInt(Random.Range(3, 6));
        var sizeZ = Mathf.RoundToInt(Random.Range(3, 6));
        var dimensions = new Vector3Int(sizeX, 1, sizeZ);

        return GenerateRoom(position, dimensions);
    }

    bool GenerateRoom(Vector3Int pos, Vector3Int dimensions)
    {
        var position = GetPositionAsGridSpace(pos);

        if(position.x < 0 || position.z < 0 ||
           position.x + dimensions.x > end.x || position.z + dimensions.z > end.z)
        {
            Debug.LogError("Room size exceeds grid dimensions! @ " + position.ToString() + " | " + new Vector3Int(position.x + dimensions.x, position.y + dimensions.y, position.z + dimensions.z).ToString());
            return false;
        }

        bool wasAlreadyOccupied = false;
        for (int x = position.x; x < position.x + dimensions.x; x++)
        {
            for (int z = position.z; z < dimensions.z; z++)
            {
                if(grid.cells[x, z].occupied)
                {
                    wasAlreadyOccupied = true;
                    break;
                }

                Debug.Log("Occupying space @ " + x + ", " + z);
                grid.cells[x, z].occupied = true;
            }
            if (wasAlreadyOccupied)
                break;
        }
        if(wasAlreadyOccupied)
        {
            Debug.LogError("Room tried overlappign existing room!");
            return false;
        }
        return true;
    }

    Vector3Int GenerateRandomVector(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
    {
        var x = Mathf.RoundToInt(Random.Range(minX, maxX));
        var y = Mathf.RoundToInt(Random.Range(minY, maxY));
        var z = Mathf.RoundToInt(Random.Range(minZ, maxZ));
        return new Vector3Int(x, y, z);
    }

    Vector3Int GetPositionAsGridSpace(Vector3Int position)
    {
        var x = Mathf.RoundToInt(position.x / RoomGrid.cellSize.x);
        var y = Mathf.RoundToInt(position.y / RoomGrid.cellSize.y);
        var z = Mathf.RoundToInt(position.z / RoomGrid.cellSize.z);
        return new Vector3Int(x, y, z);
    }

    GameObject SpawnPrefab(GameObject prefab, Vector3 position, Vector3 rotation)
    {
        GameObject spawned = Instantiate(prefab, position, Quaternion.Euler(rotation));
        spawned.transform.SetParent(roomParent);
        return spawned;
    }

    RoomGridCell GetCellAt(int x, int z)
    {
        if (x < 0 || z < 0 || x >= grid.dimensions.x || z >= grid.dimensions.z)
            return null;

        return grid.cells[x, z];
    }

    void ScoreGrid()
    {
        for (int x = 0; x < grid.dimensions.x; x++)
        {
            for (int z = 0; z < grid.dimensions.z; z++)
            {
                grid.cells[x, z].SetDistance(end);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(start, 0.25f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(end, 0.25f);

        if (grid != null)
            grid.OnDrawGizmos(transform);
    }
}
