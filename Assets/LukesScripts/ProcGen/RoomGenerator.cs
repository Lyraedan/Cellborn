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

    public List<RoomPrefab> prefabs = new List<RoomPrefab>();

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
        Smooth();

        PlaceFloors();
        PlaceWalls();
        PlaceCorners();
        PlaceEntities();

        var sorted = rooms.OrderBy(room => room.centre.magnitude).ToList();
        start = sorted[0];
        end = sorted[sorted.Count - 1];

        BakeNavmesh();

        Vector3 spawnCoords = PositionAsGridCoordinates(start.centre);
        GridCell spawnPoint = navAgent.GetGridCellAt((int) spawnCoords.x, (int) spawnCoords.y, (int) spawnCoords.z);
        var player = SpawnPlayer(spawnPoint);
        Camera.main.gameObject.GetComponent<CameraFollow>().player = player;
    }

    public GameObject SpawnPrefab(GameObject prefab, Vector3 position, Vector3 rotation)
    {
        position.y += -0.5f;
        GameObject spawned = Instantiate(prefab, position, Quaternion.Euler(rotation));
        spawned.name = $"{prefab.name}_{position.ToString()}_{rotation.ToString()}";
        spawned.transform.SetParent(roomParent);
        return spawned;
    }

    #region Prefab Grabbers
    public GameObject SpawnRandomFloor(GridCell cell)
    {
        var floors = prefabs.Where(e => e.type.Equals(RoomPrefab.RoomPropType.FLOOR)).ToList();
        int index = Random.Range(0, floors.Count);
        return floors[index].Spawn(cell.position, cell.rotation);
    }

    public GameObject SpawnRandomWall(GridCell cell)
    {
        var walls = prefabs.Where(e => e.type.Equals(RoomPrefab.RoomPropType.WALL)).ToList();
        int index = Random.Range(0, walls.Count);
        return walls[index].Spawn(cell.position, cell.rotation);
    }

    public GameObject SpawnRandomProp(GridCell cell)
    {
        var prop = prefabs.Where(e => e.type.Equals(RoomPrefab.RoomPropType.PROP)).ToList();
        int index = Random.Range(0, prop.Count);
        return prop[index].Spawn(cell.position, cell.rotation);
    }
    
    public GameObject SpawnRandomCorner(GridCell cell)
    {
        var corners = prefabs.Where(e => e.type.Equals(RoomPrefab.RoomPropType.CORNER)).ToList();
        int index = Random.Range(0, corners.Count);
        return corners[index].Spawn(cell.position, cell.rotation);
    }

    public GameObject SpawnRandomEntity(GridCell cell)
    {
        var entities = prefabs.Where(e => e.type.Equals(RoomPrefab.RoomPropType.ENTITY)).ToList();
        int index = Random.Range(0, entities.Count);
        var pos = cell.position;
        pos.y += 0.5f;
        return entities[index].Spawn(pos, Vector3.zero);
    }
    
    public GameObject SpawnPlayer(GridCell cell)
    {
        var player = prefabs.Where(e => e.type.Equals(RoomPrefab.RoomPropType.PLAYER)).ToList()[0];
        var pos = cell.position;
        pos.y += 0.5f;
        return player.Spawn(pos, Vector3.zero);
    }
    #endregion

    #region Population
    void PlaceFloors()
    {
        for(int x = 0; x < grid.cells.x; x++)
        {
            for(int z = 0; z < grid.cells.z; z++)
            {
                var cell = grid.grid[x, 0, z];
                if(cell.flag.Equals(GridCell.GridFlag.OCCUPIED) || cell.flag.Equals(GridCell.GridFlag.HALLWAY))
                    SpawnRandomFloor(cell);
            }
        }
    }

    void PlaceWalls()
    {
        for(int x = 0; x < grid.cells.x; x++)
        {
            for(int z = 0; z < grid.cells.z; z++)
            {
                var cell = grid.grid[x, 0, z];
                if (cell.flag.Equals(GridCell.GridFlag.WALL))
                    SpawnRandomWall(cell);
            }
        }
    }
    
    void PlaceCorners()
    {
        for (int x = 0; x < grid.cells.x; x++)
        {
            for (int z = 0; z < grid.cells.z; z++)
            {
                var cell = grid.grid[x, 0, z];
                if (cell.flag.Equals(GridCell.GridFlag.CORNER))
                    SpawnRandomCorner(cell);
            }
        }
    }

    void PlaceEntities()
    {
        for(int x = 0; x < grid.cells.x; x++)
        {
            for(int z = 0; z < grid.cells.z; z++)
            {
                var cell = grid.grid[x, 0, z];
                if (cell.hasEntity)
                    SpawnRandomEntity(cell);
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
                if(cell.flag.Equals(GridCell.GridFlag.WALKABLE) || cell.flag.Equals(GridCell.GridFlag.WALL))
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
                    
                    // Diagonals
                    if(nx > cx)
                    {
                        if(adjacent[3].flag.Equals(GridCell.GridFlag.WALKABLE) || adjacent[3].flag.Equals(GridCell.GridFlag.WALL))
                            adjacent[3].flag = GridCell.GridFlag.HALLWAY;
                    }
                    if(nx < cx)
                    {
                        if (adjacent[2].flag.Equals(GridCell.GridFlag.WALKABLE) || adjacent[2].flag.Equals(GridCell.GridFlag.WALL))
                            adjacent[2].flag = GridCell.GridFlag.HALLWAY;
                    }
                    if(nz < cz)
                    {
                        if (adjacent[1].flag.Equals(GridCell.GridFlag.WALKABLE) || adjacent[1].flag.Equals(GridCell.GridFlag.WALL))
                            adjacent[1].flag = GridCell.GridFlag.HALLWAY;
                    }
                    if(nz > cz)
                    {
                        if (adjacent[0].flag.Equals(GridCell.GridFlag.WALKABLE) || adjacent[0].flag.Equals(GridCell.GridFlag.WALL))
                            adjacent[0].flag = GridCell.GridFlag.HALLWAY;
                    }
                    // Straight up, down, left, right
                    if(adjacent[0].flag.Equals(GridCell.GridFlag.WALKABLE) || adjacent[0].flag.Equals(GridCell.GridFlag.WALL))
                    {
                        adjacent[0].flag = GridCell.GridFlag.HALLWAY;
                        adjacent[0].rotation = Vector3.zero;
                    }

                    if (adjacent[1].flag.Equals(GridCell.GridFlag.WALKABLE) || adjacent[1].flag.Equals(GridCell.GridFlag.WALL))
                    {
                        adjacent[1].flag = GridCell.GridFlag.HALLWAY;
                        adjacent[1].rotation = new Vector3(0, 180, 0);
                    }

                    if (adjacent[2].flag.Equals(GridCell.GridFlag.WALKABLE) || adjacent[2].flag.Equals(GridCell.GridFlag.WALL))
                    {
                        adjacent[2].flag = GridCell.GridFlag.HALLWAY;
                        adjacent[2].rotation = new Vector3(0, 270, 0);
                    }

                    if (adjacent[3].flag.Equals(GridCell.GridFlag.WALKABLE) || adjacent[3].flag.Equals(GridCell.GridFlag.WALL))
                    {
                        adjacent[3].flag = GridCell.GridFlag.HALLWAY;
                        adjacent[2].rotation = new Vector3(0, 90, 0);
                    }
                }

                //Check last square since we miss it off
                GridCell last = room.hallways[room.hallways.Count - 1];
                var finalAdjacent = GetAdjacentCells(last);
                if (finalAdjacent[0].flag.Equals(GridCell.GridFlag.WALKABLE) || finalAdjacent[0].flag.Equals(GridCell.GridFlag.WALL))
                {
                    finalAdjacent[0].flag = GridCell.GridFlag.HALLWAY;
                    finalAdjacent[0].rotation = Vector3.zero;
                }

                if (finalAdjacent[1].flag.Equals(GridCell.GridFlag.WALKABLE) || finalAdjacent[1].flag.Equals(GridCell.GridFlag.WALL))
                {
                    finalAdjacent[1].flag = GridCell.GridFlag.HALLWAY;
                    finalAdjacent[1].rotation = new Vector3(0, 180, 0);
                }

                if (finalAdjacent[2].flag.Equals(GridCell.GridFlag.WALKABLE) || finalAdjacent[2].flag.Equals(GridCell.GridFlag.WALL))
                {
                    finalAdjacent[2].flag = GridCell.GridFlag.HALLWAY;
                    finalAdjacent[2].rotation = new Vector3(0, 270, 0);
                }

                if (finalAdjacent[3].flag.Equals(GridCell.GridFlag.WALKABLE) || finalAdjacent[3].flag.Equals(GridCell.GridFlag.WALL))
                {
                    finalAdjacent[3].flag = GridCell.GridFlag.HALLWAY;
                    finalAdjacent[2].rotation = new Vector3(0, 90, 0);
                }
            }
        }
    }

    /// <summary>
    /// A disgusting function that checks each tile individually
    /// </summary>
    void Smooth()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            var room = rooms[i];
            // Carve center hallways
            for (int j = 0; j < room.hallways.Count; j++)
            {
                var hallway = room.hallways[j];
                hallway.flag = GridCell.GridFlag.OCCUPIED;
            }
        }

        for (int x = 0; x < grid.cells.x; x++)
        {
            for (int z = 0; z < grid.cells.z; z++)
            {
                var current = grid.grid[x, 0, z];
                if (!current.flag.Equals(GridCell.GridFlag.WALKABLE))
                {
                    var adjacent = GetAdjacentCells(current);
                    var upValid = adjacent[0] != null;
                    var downValid = adjacent[1] != null;
                    var leftValid = adjacent[2] != null;
                    var rightValid = adjacent[3] != null;

                    if (current.flag.Equals(GridCell.GridFlag.WALL))
                    {
                        // Clear walls
                        if (upValid)
                        {
                            if (adjacent[0].flag.Equals(GridCell.GridFlag.OCCUPIED))
                            {
                                // Bottom walls
                                //current.flag = GridCell.GridFlag.NONWALKABLE;
                                current.rotation = new Vector3(0, 90, 0);
                            }
                        }
                        if (downValid)
                        {
                            if (adjacent[1].flag.Equals(GridCell.GridFlag.OCCUPIED))
                            {
                                // Top walls
                                //current.flag = GridCell.GridFlag.NONWALKABLE;
                                current.rotation = new Vector3(0, 270, 0);
                            }
                        }
                        if (leftValid)
                        {
                            if (adjacent[2].flag.Equals(GridCell.GridFlag.OCCUPIED))
                            {
                                // left walls
                                //current.flag = GridCell.GridFlag.NONWALKABLE;
                                current.rotation = new Vector3(0, 0, 0);
                            }
                        }
                        if (rightValid)
                        {
                            if (adjacent[3].flag.Equals(GridCell.GridFlag.OCCUPIED))
                            {
                                // right walls
                                //current.flag = GridCell.GridFlag.NONWALKABLE;
                                current.rotation = new Vector3(0, 180, 0);
                            }
                        }

                        if (upValid && downValid)
                        {
                            if (adjacent[0].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[1].flag.Equals(GridCell.GridFlag.OCCUPIED))
                            {
                                current.flag = GridCell.GridFlag.OCCUPIED;
                            }
                        }
                        if (leftValid && rightValid)
                        {
                            if (adjacent[2].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[3].flag.Equals(GridCell.GridFlag.OCCUPIED))
                            {
                                current.flag = GridCell.GridFlag.OCCUPIED;
                            }
                        }
                        // Flag corners
                        if (downValid && rightValid)
                        {
                            // Top left
                            if (adjacent[1].flag.Equals(GridCell.GridFlag.WALL) && adjacent[3].flag.Equals(GridCell.GridFlag.WALL))
                            {
                                current.flag = GridCell.GridFlag.CORNER;
                                current.rotation = new Vector3(0, 180, 0);
                            }
                        }
                        if (downValid && leftValid)
                        {
                            // Top right
                            if (adjacent[1].flag.Equals(GridCell.GridFlag.WALL) && adjacent[2].flag.Equals(GridCell.GridFlag.WALL))
                            {
                                current.flag = GridCell.GridFlag.CORNER;
                                current.rotation = new Vector3(0, 270, 0);
                            }
                        }
                        if (upValid && rightValid)
                        {
                            // Bottom left
                            if (adjacent[0].flag.Equals(GridCell.GridFlag.WALL) && adjacent[3].flag.Equals(GridCell.GridFlag.WALL))
                            {
                                current.flag = GridCell.GridFlag.CORNER;
                                current.rotation = new Vector3(0, 90, 0);
                            }
                        }
                        if (upValid && leftValid)
                        {
                            // Bottom right
                            if (adjacent[0].flag.Equals(GridCell.GridFlag.WALL) && adjacent[2].flag.Equals(GridCell.GridFlag.WALL))
                            {
                                current.flag = GridCell.GridFlag.CORNER;
                                current.rotation = Vector3.zero;
                            }
                        }
                    }
                    else if (current.flag.Equals(GridCell.GridFlag.HALLWAY))
                    {
                        //current.flag = GridCell.GridFlag.OCCUPIED;
                        if (upValid && downValid && leftValid && rightValid)
                        {
                            if (adjacent[0].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[1].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[2].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[3].flag.Equals(GridCell.GridFlag.OCCUPIED))
                            {
                                //current.flag = GridCell.GridFlag.NONWALKABLE;
                            }
                        }

                        if (upValid && downValid)
                        {
                            if (adjacent[0].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[1].flag.Equals(GridCell.GridFlag.OCCUPIED))
                            {
                                current.flag = GridCell.GridFlag.OCCUPIED;
                            }
                        }
                        if (leftValid && rightValid)
                        {
                            if (adjacent[2].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[3].flag.Equals(GridCell.GridFlag.OCCUPIED))
                            {
                                current.flag = GridCell.GridFlag.OCCUPIED;
                            }
                        }
                    }
                    else if (current.flag.Equals(GridCell.GridFlag.CORNER))
                    {
                        if (leftValid && rightValid)
                        {
                            if (adjacent[2].flag.Equals(GridCell.GridFlag.WALL) && adjacent[3].flag.Equals(GridCell.GridFlag.OCCUPIED))
                            {
                                current.flag = GridCell.GridFlag.WALL;
                            }
                            else if (adjacent[3].flag.Equals(GridCell.GridFlag.WALL) && adjacent[2].flag.Equals(GridCell.GridFlag.OCCUPIED))
                            {
                                current.flag = GridCell.GridFlag.WALL;
                            }
                        }
                    }
                }
            }
        }

        // Fix corridor corner rotations
        for (int z = 0; z < grid.cells.z; z++)
        {
            for (int x = 0; x < grid.cells.x; x++)
            {
                var current = grid.grid[x, 0, z];
                if (current.flag.Equals(GridCell.GridFlag.HALLWAY))
                {
                    var adjacent = GetAdjacentCells(current);
                    var upValid = adjacent[0] != null;
                    var downValid = adjacent[1] != null;
                    var leftValid = adjacent[2] != null;
                    var rightValid = adjacent[3] != null;

                    if (downValid && rightValid)
                    {
                        if (adjacent[1].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[3].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.flag = GridCell.GridFlag.CORNER;
                            current.rotation = new Vector3(0, 270, 0);
                        }
                        if (adjacent[1].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[3].flag.Equals(GridCell.GridFlag.OCCUPIED))
                        {
                            current.flag = GridCell.GridFlag.CORNER;
                            current.rotation = new Vector3(0, 90, 0);
                        }
                    }

                    if (leftValid && downValid)
                    {
                        if (adjacent[2].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[1].flag.Equals(GridCell.GridFlag.OCCUPIED))
                        {
                            current.flag = GridCell.GridFlag.CORNER;
                            current.rotation = new Vector3(0, 180, 0);
                        }
                        if (adjacent[2].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[1].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.flag = GridCell.GridFlag.CORNER;
                            current.rotation = Vector3.zero;
                        }
                    }

                    if (leftValid && rightValid) {
                        if((adjacent[2].flag.Equals(GridCell.GridFlag.HALLWAY) || adjacent[3].flag.Equals(GridCell.GridFlag.HALLWAY)) && adjacent[0].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.flag = GridCell.GridFlag.WALL;
                            current.rotation = new Vector3(0, 270, 0);
                        }
                        if ((adjacent[2].flag.Equals(GridCell.GridFlag.HALLWAY) || adjacent[3].flag.Equals(GridCell.GridFlag.HALLWAY)) && adjacent[1].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.flag = GridCell.GridFlag.WALL;
                            current.rotation = new Vector3(0, 90, 0);
                        }
                    }

                    if (upValid && downValid)
                    {
                        if((adjacent[0].flag.Equals(GridCell.GridFlag.HALLWAY) && adjacent[1].flag.Equals(GridCell.GridFlag.HALLWAY)) && adjacent[2].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.flag = GridCell.GridFlag.WALL;
                            current.rotation = new Vector3(0, 180, 0);
                        }

                        if ((adjacent[0].flag.Equals(GridCell.GridFlag.HALLWAY) && adjacent[1].flag.Equals(GridCell.GridFlag.HALLWAY)) && adjacent[3].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.flag = GridCell.GridFlag.WALL;
                            current.rotation = Vector3.zero;
                        }
                    }
                }
            }
        }
    }

    void BakeNavmesh()
    {

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

                //Flag walls
                if(x <= pos.x || x >= pos.x + (roomDimensions.x - 1) ||
                   z <= pos.z || z >= pos.z + (roomDimensions.z - 1))
                {
                    // These are walls
                    grid.grid[x, 0, z].flag = GridCell.GridFlag.WALL;
                    room.walls.Add(grid.grid[x, 0, z]);
                } else
                {
                    grid.grid[x, 0, z].flag = GridCell.GridFlag.OCCUPIED;
                    bool spawnEntity = (Random.Range(0, 20) == 0);
                    Debug.Log("Spawn entity: " + spawnEntity);
                    grid.grid[x, 0, z].hasEntity = spawnEntity;
                    room.occupied.Add(grid.grid[x, 0, z]);
                }
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
                if (cell.flag.Equals(GridCell.GridFlag.WALKABLE) || cell.flag.Equals(GridCell.GridFlag.WALL))
                {
                    cell.flag = GridCell.GridFlag.HALLWAY;
                    rooms[rooms.Count - 2].hallways.Add(cell);
                }
            }

            foreach (GridCell cell in navAgent.closed)
            {
                if (cell.flag.Equals(GridCell.GridFlag.WALKABLE) || cell.flag.Equals(GridCell.GridFlag.WALL))
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

    public Vector3 PositionAsGridCoordinates(Vector3 position)
    {
        var x = Mathf.RoundToInt(position.x / grid.cellSize.x);
        var y = Mathf.RoundToInt(position.y / grid.cellSize.y);
        var z = Mathf.RoundToInt(position.z / grid.cellSize.z);
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
