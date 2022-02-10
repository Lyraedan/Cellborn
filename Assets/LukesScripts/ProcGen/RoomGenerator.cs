using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public static RoomGenerator instance;

    public Transform roomParent;
    public RoomGrid grid;
    public List<Room> rooms = new List<Room>();

    public Vector3Int start, end;

    public float rotationTest = 45;
    public GameObject test;

    public int maxRoomLimit = 10;

    private int failedAttempts = 0;

    private PathFinding pathfinder;

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
        pathfinder = new PathFinding();

        start = GenerateRandomVector(3, 0, 3, 10, 1, 10);
        end = GenerateRandomVector(start.x, 0, start.z, 50, 1, 50);

        //var distance = end - start;
        Debug.Log("End: " + end.ToString());

        grid = new RoomGrid(new Vector3Int(end.x, 1, end.z));
        ScoreGrid();

        GenerateRandomRoom(start);
        GenerateRandomRoom(end);

        for (int i = 0; i < maxRoomLimit; i++)
        {
            GenerateRandomRoom();
        }

        if(rooms.Count == 0)
        {
            Debug.LogError("No rooms were generated!");
        }

        ConnectRooms();
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

    bool GenerateRandomRoom(Vector3Int pos)
    {
        var sizeX = Mathf.RoundToInt(Random.Range(3, 6));
        var sizeZ = Mathf.RoundToInt(Random.Range(3, 6));
        var dimensions = new Vector3Int(sizeX, 1, sizeZ);
        return GenerateRoom(pos, dimensions);
    }

    bool GenerateRoom(Vector3Int pos, Vector3Int dimensions)
    {
        var position = GetPositionAsGridSpace(pos);

        var width = grid.dimensions.x;
        var length = grid.dimensions.z;

        if (pos.x < 0 || pos.z < 0 || pos.x + dimensions.x >= width || pos.z + dimensions.z >= length)
        {
            Debug.Log("Room size exceeds grid size!");
            return false;
        }

        Room room = new Room();
        for(int x = pos.x; x < pos.x + dimensions.x; x++)
        {
            for(int z = pos.z; z < pos.z + dimensions.z; z++)
            {
                if(grid.cells[x, z].occupied)
                {
                    Debug.LogError("Overlapping rooms!");
                    break;
                }

                grid.cells[x, z].occupied = true;
                grid.cells[x, z].flag = RoomGridCell.CellFlag.FLOOR;
                RoomTile tile = new RoomTile();
                tile.gridX = x;
                tile.gridZ = z;
                tile.cell = grid.cells[x, z];
                room.tiles.Add(tile);
            }
        }

        // No tiles were assigned
        if (room.tiles.Count <= 0)
            return false;

        room.SpawnTiles();
        rooms.Add(room);
        return true;
    }

    void ConnectRooms()
    {
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            pathfinder.source = rooms[i].center.cell.position;
            pathfinder.destination = rooms[i + 1].center.cell.position;

            pathfinder.CalculatePath(path => {
                Debug.Log("Path node count: " + path.Count);
                foreach(RoomGridCell cell in path)
                {
                    SpawnPrefab(test, cell.position, Vector3.zero);
                    cell.flag = RoomGridCell.CellFlag.TEST;
                    cell.occupied = true;
                }
            });
        }
    }

    Vector3Int GenerateRandomVector(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
    {
        var x = Mathf.RoundToInt(Random.Range(minX, maxX));
        var y = Mathf.RoundToInt(Random.Range(minY, maxY));
        var z = Mathf.RoundToInt(Random.Range(minZ, maxZ));
        return new Vector3Int(x, y, z);
    }

    public Vector3Int GetPositionAsGridSpace(Vector3Int position)
    {
        var x = Mathf.RoundToInt(position.x / RoomGrid.cellSize.x);
        var y = Mathf.RoundToInt(position.y / RoomGrid.cellSize.y);
        var z = Mathf.RoundToInt(position.z / RoomGrid.cellSize.z);
        return new Vector3Int(x, y, z);
    }

    public GameObject SpawnPrefab(GameObject prefab, Vector3 position, Vector3 rotation)
    {
        GameObject spawned = Instantiate(prefab, position, Quaternion.Euler(rotation));
        spawned.transform.SetParent(roomParent);
        return spawned;
    }

    public RoomGridCell GetCellAt(int x, int z)
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

        if(pathfinder != null)
            pathfinder.DrawGizmos();

        if (rooms.Count > 0)
        {
            for (int i = 0; i < rooms.Count - 1; i++)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawCube(rooms[i].center.cell.position, Vector3.one);
                Gizmos.color = Color.black;
                Gizmos.DrawLine(rooms[i].center.cell.position, rooms[i + 1].center.cell.position);
            }
        }
    }
}
