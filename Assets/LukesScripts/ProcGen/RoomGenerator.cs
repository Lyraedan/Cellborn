using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public static RoomGenerator instance;

    public Transform roomParent;
    public Grid grid;
    [Header("Dungeon settings")]
    public int seed = 0;
    public Vector2 minDungeonSize = new Vector2(10, 10);
    public Vector2 maxDungeonSize = new Vector2(25, 25);
    public Vector2 minRoomSize = new Vector2(3, 6);
    [Tooltip("This gets updated at runtime")]public Vector3 generatedDungeonSize = Vector3.zero;
    public int maxRoomLimit = 10;
    public List<Room> rooms = new List<Room>();

    private Room start, end;

    public GameObject test;

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
        if (seed == 0)
            seed = Random.Range(0, 1000000);

        Random.InitState(seed);
        generatedDungeonSize = GenerateRandomVector((int) minDungeonSize.x, 0, (int) minDungeonSize.y, (int) maxDungeonSize.x, 1, (int) maxDungeonSize.y);
        Debug.Log("Generated dungeon of size: " + generatedDungeonSize.ToString());

        var dimensions = new Vector3(generatedDungeonSize.x, 1, generatedDungeonSize.z);
        grid.cells = dimensions;
        grid.Init();

        for (int i = 0; i < maxRoomLimit; i++)
        {
            GenerateRandomRoom();
        }

        if (rooms.Count == 0)
        {
            Debug.LogError("No rooms were generated!");
        }
        ConnectRooms();
        ShapeHallways();

        PlaceFloors();

        var sorted = rooms.OrderBy(room => room.centre.magnitude).ToList();
        start = sorted[0];
        end = sorted[sorted.Count - 1];
    }

    public GameObject SpawnPrefab(GameObject prefab, Vector3 position, Vector3 rotation)
    {
        GameObject spawned = Instantiate(prefab, position, Quaternion.Euler(rotation));
        spawned.name = $"{prefab.name}_{position.ToString()}_{rotation.ToString()}";
        spawned.transform.SetParent(roomParent);
        return spawned;
    }

    #region Population
    void PlaceFloors()
    {
        for(int x = 0; x < grid.cells.x; x++)
        {
            for(int z = 0; z < grid.cells.z; z++)
            {
                var cell = grid.grid[x, 0, z];
                if(cell.flag.Equals(GridCell.GridFlag.OCCUPIED) || cell.flag.Equals(GridCell.GridFlag.HALLWAY))
                    SpawnPrefab(test, cell.position, Vector3.zero);
            }
        }
    }
    #endregion

    #region Proc gen
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
                    cell.flag = GridCell.GridFlag.HALLWAY;
                    rooms[i].hallways.Add(cell);
                }
            }
        }
    }

    void ShapeHallways()
    {
        // Look for 1x1 hallway tiles and extend them to either 2x2
        for(int i = 0; i < rooms.Count; i++)
        {
            var room = rooms[i];
            if (room.hallways.Count >= 2)
            {
                for (int j = 0; j < room.hallways.Count - 1; j++)
                {
                    GridCell current = room.hallways[j];
                    GridCell next = room.hallways[j + 1];

                    var adjacent = GetAdjacentCells(current);

                    int cx = (int) current.position.x;
                    int cz = (int) current.position.z;
                    int nx = (int) next.position.x;
                    int nz = (int) next.position.z;
                    Debug.Log($"Current ({cx}, {cz}) -> Next({nx}, {nz})");
                    
                    // Diagonals
                    if(nx > cx)
                    {
                        if(adjacent[3].flag.Equals(GridCell.GridFlag.WALKABLE))
                            adjacent[3].flag = GridCell.GridFlag.HALLWAY;
                    }
                    if(nx < cx)
                    {
                        if (adjacent[2].flag.Equals(GridCell.GridFlag.WALKABLE))
                            adjacent[2].flag = GridCell.GridFlag.HALLWAY;
                    }
                    if(nz < cz)
                    {
                        if (adjacent[1].flag.Equals(GridCell.GridFlag.WALKABLE))
                            adjacent[1].flag = GridCell.GridFlag.HALLWAY;
                    }
                    if(nz > cz)
                    {
                        if (adjacent[0].flag.Equals(GridCell.GridFlag.WALKABLE))
                            adjacent[0].flag = GridCell.GridFlag.HALLWAY;
                    }
                    // Straight up, down, left, right
                    if(adjacent[0].flag.Equals(GridCell.GridFlag.WALKABLE))
                    {
                        adjacent[0].flag = GridCell.GridFlag.HALLWAY;
                    }

                    if (adjacent[1].flag.Equals(GridCell.GridFlag.WALKABLE))
                    {
                        adjacent[1].flag = GridCell.GridFlag.HALLWAY;
                    }

                    if (adjacent[2].flag.Equals(GridCell.GridFlag.WALKABLE))
                    {
                        adjacent[2].flag = GridCell.GridFlag.HALLWAY;
                    }

                    if (adjacent[3].flag.Equals(GridCell.GridFlag.WALKABLE))
                    {
                        adjacent[3].flag = GridCell.GridFlag.HALLWAY;
                    }
                }

                //Check last square since we miss it off
                GridCell last = room.hallways[room.hallways.Count - 1];
                var finalAdjacent = GetAdjacentCells(last);
                if (finalAdjacent[0].flag.Equals(GridCell.GridFlag.WALKABLE))
                {
                    finalAdjacent[0].flag = GridCell.GridFlag.HALLWAY;
                }

                if (finalAdjacent[1].flag.Equals(GridCell.GridFlag.WALKABLE))
                {
                    finalAdjacent[1].flag = GridCell.GridFlag.HALLWAY;
                }

                if (finalAdjacent[2].flag.Equals(GridCell.GridFlag.WALKABLE))
                {
                    finalAdjacent[2].flag = GridCell.GridFlag.HALLWAY;
                }

                if (finalAdjacent[3].flag.Equals(GridCell.GridFlag.WALKABLE))
                {
                    finalAdjacent[3].flag = GridCell.GridFlag.HALLWAY;
                }
            }
        }
    }

    /// <summary>
    /// Returns the cells "up, down, left, right" to the current cell
    /// </summary>
    /// <param name="current"></param>
    /// <returns></returns>
    GridCell[] GetAdjacentCells(GridCell current)
    {
        GridCell up = navAgent.GetGridCellAt((int)current.position.x, (int)current.position.y, (int)current.position.z + 1);
        GridCell down = navAgent.GetGridCellAt((int)current.position.x, (int)current.position.y, (int)current.position.z - 1);
        GridCell left = navAgent.GetGridCellAt((int)current.position.x - 1, (int)current.position.y, (int)current.position.z);
        GridCell right = navAgent.GetGridCellAt((int)current.position.x + 1, (int)current.position.y, (int)current.position.z);
        return new GridCell[] { up, down, left, right };
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
                    //break;
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
                    cell.flag = GridCell.GridFlag.HALLWAY;
                    rooms[rooms.Count - 2].hallways.Add(cell);
                }
            }

            foreach (GridCell cell in navAgent.closed)
            {
                if (cell.flag.Equals(GridCell.GridFlag.WALKABLE))
                {
                    cell.flag = GridCell.GridFlag.HALLWAY;
                    rooms[rooms.Count - 2].hallways.Add(cell);
                }
            }
        }
        return true;
    }

    bool GenerateRandomRoom()
    {
        var posX = Mathf.RoundToInt(Random.Range(0, generatedDungeonSize.x));
        var posZ = Mathf.RoundToInt(Random.Range(0, generatedDungeonSize.z));
        var position = new Vector3(posX, 1, posZ);

        var sizeX = Mathf.RoundToInt(Random.Range(minRoomSize.x, minRoomSize.y));
        var sizeZ = Mathf.RoundToInt(Random.Range(minRoomSize.x, minRoomSize.y));
        var dimensions = new Vector3(sizeX, 1, sizeZ);

        return GenerateRoom(position, dimensions);
    }
    #endregion

    Vector3 GenerateRandomVector(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
    {
        var x = Mathf.RoundToInt(Random.Range(minX, maxX));
        var y = Mathf.RoundToInt(Random.Range(minY, maxY));
        var z = Mathf.RoundToInt(Random.Range(minZ, maxZ));
        return new Vector3(x, y, z);
    }

    private void OnDrawGizmos()
    {
        if (start == null || end == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(start.centre, 0.25f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(end.centre, 0.25f);

        for(int i = 0; i < rooms.Count; i++)
        {
            var room = rooms[i];
            if (room.drawHallwayPath)
            {
                Gizmos.color = Color.black;
                for(int j = 1; j < room.hallways.Count - 1; j++)
                {
                    Gizmos.DrawSphere(room.hallways[j].position, 0.25f);
                }
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(room.hallways[0].position, 0.25f);
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(room.hallways[room.hallways.Count - 1].position, 0.25f);
            }
        }
    }
}
