using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public static RoomGenerator instance;

    public Transform roomParent;
    public Grid grid;
    public List<Room> rooms = new List<Room>();

    public Vector3 start, end;

    public float rotationTest = 45;
    public GameObject test;

    public int maxRoomLimit = 10;

    private int failedAttempts = 0;

    public GradedPath navAgent;

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
        start = GenerateRandomVector(10, 0, 10, 25, 1, 25);
        end = GenerateRandomVector((int) start.x, 0, (int) start.z, (int) start.x + 25, 1, (int)start.z + 25);

        //var distance = end - start;

        var dimensions = new Vector3(end.x, 1, end.z);
        grid.cells = dimensions;
        grid.Init();

        ScoreGrid();

        GenerateRandomRoom(start);
        GenerateRandomRoom(end);

        for (int i = 0; i < maxRoomLimit; i++)
        {
            GenerateRandomRoom();
        }

        if (rooms.Count == 0)
        {
            Debug.LogError("No rooms were generated!");
        }
        //ConnectRooms();
    }

    void ConnectRooms()
    {
        for(int i = 0; i < rooms.Count - 1; i++)
        {
            navAgent.src = rooms[i].centre;
            navAgent.dest = rooms[i + 1].centre;
            navAgent.CalculatePath();
            foreach(GridCell cell in navAgent.open)
            {
                if(cell.flag.Equals(GridCell.GridFlag.WALKABLE))
                {
                    cell.flag = GridCell.GridFlag.OCCUPIED;
                }
            }
        }
    }

    public GameObject SpawnPrefab(GameObject prefab, Vector3 position, Vector3 rotation)
    {
        GameObject spawned = Instantiate(prefab, position, Quaternion.Euler(rotation));
        spawned.transform.SetParent(roomParent);
        return spawned;
    }

    void ScoreGrid()
    {
        for (int x = 0; x < grid.cells.x; x++)
        {
            for (int z = 0; z < grid.cells.z; z++)
            {
                grid.grid[x, 0, z].SetDistance(end);
            }
        }
    }

    bool GenerateRoom(Vector3 pos, Vector3 roomDimensions)
    {
        var width = grid.cells.x;
        var length = grid.cells.z;

        if(pos.x < 0 || pos.z < 0 || pos.x + roomDimensions.x >= width || pos.z + roomDimensions.z >= length)
        {
            Debug.LogWarning("Room size exceeds grid size!");
            return false;
        }

        Room room = new Room();
        for(int x = (int) pos.x; x < pos.x + roomDimensions.x; x++)
        {
            for(int z = (int) pos.z; z < pos.z + roomDimensions.z; z++)
            {
                if(grid.grid[x, 0, z].flag.Equals(GridCell.GridFlag.OCCUPIED))
                {
                    Debug.LogWarning("Overlapping rooms!");
                    break;
                }

                grid.grid[x, 0, z].flag = GridCell.GridFlag.OCCUPIED;
            }
        }
        room.centre = new Vector3(pos.x + (roomDimensions.x / 2), 0, pos.z + (roomDimensions.z / 2));

        rooms.Add(room);
        if(rooms.Count > 1)
        {
            // Connect to previous room
            var src = rooms[rooms.Count - 2].centre;
            var dest = rooms[rooms.Count - 1].centre;
            navAgent.src = src;
            navAgent.dest = dest;

            navAgent.CalculatePath();

            foreach (GridCell cell in navAgent.open)
            {
                if (cell.flag.Equals(GridCell.GridFlag.WALKABLE))
                {
                    cell.flag = GridCell.GridFlag.OCCUPIED;
                }
            }

            foreach (GridCell cell in navAgent.closed)
            {
                if (cell.flag.Equals(GridCell.GridFlag.WALKABLE))
                {
                    cell.flag = GridCell.GridFlag.OCCUPIED;
                }
            }
        }
        return true;
    }

    bool GenerateRandomRoom()
    {
        var posX = Mathf.RoundToInt(Random.Range(0, end.x));
        var posZ = Mathf.RoundToInt(Random.Range(0, end.z));
        var position = new Vector3(posX, 1, posZ);

        var sizeX = Mathf.RoundToInt(Random.Range(3, 6));
        var sizeZ = Mathf.RoundToInt(Random.Range(3, 6));
        var dimensions = new Vector3(sizeX, 1, sizeZ);

        return GenerateRoom(position, dimensions);
    }

    bool GenerateRandomRoom(Vector3 pos)
    {
        var sizeX = Mathf.RoundToInt(Random.Range(3, 6));
        var sizeZ = Mathf.RoundToInt(Random.Range(3, 6));
        var dimensions = new Vector3(sizeX, 1, sizeZ);
        return GenerateRoom(pos, dimensions);
    }

    Vector3 GenerateRandomVector(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
    {
        var x = Mathf.RoundToInt(Random.Range(minX, maxX));
        var y = Mathf.RoundToInt(Random.Range(minY, maxY));
        var z = Mathf.RoundToInt(Random.Range(minZ, maxZ));
        return new Vector3(x, y, z);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(start, 0.25f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(end, 0.25f);
    }
}
