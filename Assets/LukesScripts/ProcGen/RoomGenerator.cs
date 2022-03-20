using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

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
    public TargetMovement targetAim;
    public bool enableCulling = true;

    //Adjacent directions
    private const int UP = 0;
    private const int DOWN = 1;
    private const int LEFT = 2;
    private const int RIGHT = 3;
    private const int UP_LEFT = 4;
    private const int UP_RIGHT = 5;
    private const int DOWN_LEFT = 6;
    private const int DOWN_RIGHT = 7;

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
        Debug.Log("Generating dungeon with seed: " + seed);
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
        FlagProps();
        FlagEntities();

        PlaceFloors();
        PlaceWalls();
        PlaceCorners();
        //grid.Bake();
        BakeNavmesh();

        PlaceProps();

        var sorted = rooms.OrderBy(room => room.centre.magnitude).ToList();
        start = sorted[0];
        end = sorted[sorted.Count - 1];

        Vector3 spawnCoords = PositionAsGridCoordinates(start.centre);
        GridCell spawnPoint = navAgent.GetGridCellAt((int) spawnCoords.x, (int) spawnCoords.y, (int) spawnCoords.z);
        var player = SpawnPlayer(spawnPoint);
        Camera.main.gameObject.GetComponent<CameraFollow>().player = player;
        targetAim.mainCam = Camera.main;

        StartCoroutine(AwaitAssignables());
    }

    IEnumerator AwaitAssignables()
    {
        yield return new WaitUntil(() => WeaponManager.instance != null);
        //Grab weapons
        WeaponManager.instance.GetWeaponsInLevel();
        Minimap.instance.GenerateMinimap(grid);
        PlaceEntities();

        Debug.Log("Got assignables");
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
        var spawned = floors[index].Spawn(cell.position, cell.rotation);
        CellInfo info = spawned.AddComponent<CellInfo>();
        info.cellRotation = cell.rotation;
        return spawned;
    }

    public GameObject SpawnRandomWall(GridCell cell)
    {
        var walls = prefabs.Where(e => e.type.Equals(RoomPrefab.RoomPropType.WALL)).ToList();
        int index = Random.Range(0, walls.Count);
        var spawned = walls[index].Spawn(cell.position, cell.rotation);
        CellInfo info = spawned.AddComponent<CellInfo>();
        info.cellRotation = cell.rotation;
        return spawned;
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
        var spawned = corners[index].Spawn(cell.position, cell.rotation);
        CellInfo info = spawned.AddComponent<CellInfo>();
        info.cellRotation = cell.rotation;
        return spawned;
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
        for (int z = 0; z < grid.cells.z; z++)
        {
            for (int x = 0; x < grid.cells.x; x++)
            {
                var cell = grid.grid[x, 0, z];
                if(cell.flag.Equals(GridCell.GridFlag.OCCUPIED) || cell.flag.Equals(GridCell.GridFlag.HALLWAY))
                    SpawnRandomFloor(cell);
            }
        }
    }

    void PlaceWalls()
    {
        for (int z = 0; z < grid.cells.z; z++)
        {
            for (int x = 0; x < grid.cells.x; x++)
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
    
    void PlaceProps()
    {
        for (int x = 0; x < grid.cells.x; x++)
        {
            for (int z = 0; z < grid.cells.z; z++)
            {
                var cell = grid.grid[x, 0, z];
                if (cell.hasProp)
                    SpawnRandomProp(cell);
            }
        }
    }
    
    void FlagProps()
    {
        for(int z = 0; z < grid.cells.z; z++)
        {
            for(int x = 0; x < grid.cells.x; x++)
            {
                var cell = grid.grid[x, 0, z];
                if (cell.flag.Equals(GridCell.GridFlag.OCCUPIED))
                {
                    bool spawnProp = (Random.Range(0, 10) == 0);
                    cell.hasProp = spawnProp;
                }
            }
        }
    }

    void FlagEntities()
    {
        for(int z = 0; z < grid.cells.z; z++)
        {
            for(int x = 0; x < grid.cells.x; x++)
            {
                var cell = grid.grid[x, 0, z];
                if (cell.flag.Equals(GridCell.GridFlag.OCCUPIED))
                {
                    if (!cell.hasProp)
                    {
                        bool spawnEntity = (Random.Range(0, 20) == 0);
                        grid.grid[x, 0, z].hasEntity = spawnEntity;
                    }
                }
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
                        if(adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALKABLE) || adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALL))
                            adjacent[RIGHT].flag = GridCell.GridFlag.HALLWAY;
                    }
                    if(nx < cx)
                    {
                        if (adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALKABLE) || adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL))
                            adjacent[LEFT].flag = GridCell.GridFlag.HALLWAY;
                    }
                    if(nz < cz)
                    {
                        if (adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALKABLE) || adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALL))
                            adjacent[DOWN].flag = GridCell.GridFlag.HALLWAY;
                    }
                    if(nz > cz)
                    {
                        if (adjacent[UP].flag.Equals(GridCell.GridFlag.WALKABLE) || adjacent[UP].flag.Equals(GridCell.GridFlag.WALL))
                            adjacent[UP].flag = GridCell.GridFlag.HALLWAY;
                    }
                    // Straight up, down, left, right
                    if(adjacent[UP].flag.Equals(GridCell.GridFlag.WALKABLE) || adjacent[UP].flag.Equals(GridCell.GridFlag.WALL))
                    {
                        adjacent[UP].flag = GridCell.GridFlag.HALLWAY;
                        adjacent[UP].rotation = Vector3.zero;
                    }

                    if (adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALKABLE) || adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALL))
                    {
                        adjacent[DOWN].flag = GridCell.GridFlag.HALLWAY;
                        adjacent[DOWN].rotation = new Vector3(0, 180, 0);
                    }

                    if (adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALKABLE) || adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL))
                    {
                        adjacent[LEFT].flag = GridCell.GridFlag.HALLWAY;
                        adjacent[LEFT].rotation = new Vector3(0, 270, 0);
                    }

                    if (adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALKABLE) || adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALL))
                    {
                        adjacent[RIGHT].flag = GridCell.GridFlag.HALLWAY;
                        adjacent[RIGHT].rotation = new Vector3(0, 90, 0);
                    }
                }

                //Check last square since we miss it off
                GridCell last = room.hallways[room.hallways.Count - 1];
                var finalAdjacent = GetAdjacentCells(last);
                if (finalAdjacent[UP].flag.Equals(GridCell.GridFlag.WALKABLE) || finalAdjacent[UP].flag.Equals(GridCell.GridFlag.WALL))
                {
                    finalAdjacent[UP].flag = GridCell.GridFlag.HALLWAY;
                    finalAdjacent[UP].rotation = Vector3.zero;
                }

                if (finalAdjacent[DOWN].flag.Equals(GridCell.GridFlag.WALKABLE) || finalAdjacent[DOWN].flag.Equals(GridCell.GridFlag.WALL))
                {
                    finalAdjacent[DOWN].flag = GridCell.GridFlag.HALLWAY;
                    finalAdjacent[DOWN].rotation = new Vector3(0, 180, 0);
                }

                if (finalAdjacent[LEFT].flag.Equals(GridCell.GridFlag.WALKABLE) || finalAdjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL))
                {
                    finalAdjacent[LEFT].flag = GridCell.GridFlag.HALLWAY;
                    finalAdjacent[LEFT].rotation = new Vector3(0, 270, 0);
                }

                if (finalAdjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALKABLE) || finalAdjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALL))
                {
                    finalAdjacent[RIGHT].flag = GridCell.GridFlag.HALLWAY;
                    finalAdjacent[RIGHT].rotation = new Vector3(0, 90, 0);
                }
            }
        }
    }

    /// <summary>
    /// A disgusting function that checks each tile individually
    /// </summary>
    /// Each individual for loop is necessary because there are checks that need to be made after initial changes are made
    void Smooth()
    {
        #region Base Smoothing
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

        // Base smoothing
        for (int x = 0; x < grid.cells.x; x++)
        {
            for (int z = 0; z < grid.cells.z; z++)
            {
                var current = grid.grid[x, 0, z];
                if (!current.flag.Equals(GridCell.GridFlag.WALKABLE))
                {
                    var adjacent = GetAdjacentCells(current);
                    var upValid = adjacent[UP] != null;
                    var downValid = adjacent[DOWN] != null;
                    var leftValid = adjacent[LEFT] != null;
                    var rightValid = adjacent[RIGHT] != null;

                    if (current.flag.Equals(GridCell.GridFlag.WALL))
                    {
                        // Clear walls
                        if (upValid)
                        {
                            if (adjacent[UP].flag.Equals(GridCell.GridFlag.OCCUPIED))
                            {
                                // Bottom walls
                                //current.flag = GridCell.GridFlag.NONWALKABLE;
                                current.rotation = new Vector3(0, 90, 0);
                            }
                        }
                        if (downValid)
                        {
                            if (adjacent[DOWN].flag.Equals(GridCell.GridFlag.OCCUPIED))
                            {
                                // Top walls
                                //current.flag = GridCell.GridFlag.NONWALKABLE;
                                current.rotation = new Vector3(0, 270, 0);
                            }
                        }
                        if (leftValid)
                        {
                            if (adjacent[LEFT].flag.Equals(GridCell.GridFlag.OCCUPIED))
                            {
                                // left walls
                                //current.flag = GridCell.GridFlag.NONWALKABLE;
                                current.rotation = new Vector3(0, 0, 0);
                            }
                        }
                        if (rightValid)
                        {
                            if (adjacent[RIGHT].flag.Equals(GridCell.GridFlag.OCCUPIED))
                            {
                                // right walls
                                //current.flag = GridCell.GridFlag.NONWALKABLE;
                                current.rotation = new Vector3(0, 180, 0);
                            }
                        }

                        if (upValid && downValid)
                        {
                            if (adjacent[UP].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.OCCUPIED))
                            {
                                current.flag = GridCell.GridFlag.OCCUPIED;
                            }
                        }
                        if (leftValid && rightValid)
                        {
                            if (adjacent[LEFT].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.OCCUPIED))
                            {
                                current.flag = GridCell.GridFlag.OCCUPIED;
                            }
                        }
                        // Flag corners
                        if (downValid && rightValid)
                        {
                            // Top left
                            if (adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALL) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALL))
                            {
                                current.flag = GridCell.GridFlag.CORNER;
                                current.rotation = new Vector3(0, 180, 0);
                            }
                        }
                        if (downValid && leftValid)
                        {
                            // Top right
                            if (adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALL) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL))
                            {
                                current.flag = GridCell.GridFlag.CORNER;
                                current.rotation = new Vector3(0, 270, 0);
                            }
                        }
                        if (upValid && rightValid)
                        {
                            // Bottom left
                            if (adjacent[UP].flag.Equals(GridCell.GridFlag.WALL) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALL))
                            {
                                current.flag = GridCell.GridFlag.CORNER;
                                current.rotation = new Vector3(0, 90, 0);
                            }
                        }
                        if (upValid && leftValid)
                        {
                            // Bottom right
                            if (adjacent[UP].flag.Equals(GridCell.GridFlag.WALL) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL))
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
                            if (adjacent[UP].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.OCCUPIED))
                            {
                                //current.flag = GridCell.GridFlag.NONWALKABLE;
                            }
                        }

                        if (upValid && downValid)
                        {
                            if (adjacent[UP].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.OCCUPIED))
                            {
                                current.flag = GridCell.GridFlag.OCCUPIED;
                            }
                        }
                        if (leftValid && rightValid)
                        {
                            if (adjacent[LEFT].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.OCCUPIED))
                            {
                                current.flag = GridCell.GridFlag.OCCUPIED;
                            }
                        }
                    }
                    else if (current.flag.Equals(GridCell.GridFlag.CORNER))
                    {
                        if (leftValid && rightValid)
                        {
                            if (adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.OCCUPIED))
                            {
                                current.flag = GridCell.GridFlag.WALL;
                            }
                            else if (adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.OCCUPIED))
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
                    var upValid = adjacent[UP] != null;
                    var downValid = adjacent[DOWN] != null;
                    var leftValid = adjacent[LEFT] != null;
                    var rightValid = adjacent[RIGHT] != null;

                    if (downValid && rightValid)
                    {
                        if (adjacent[DOWN].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.flag = GridCell.GridFlag.CORNER;
                            current.rotation = new Vector3(0, 270, 0);
                        }
                        if (adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.OCCUPIED))
                        {
                            current.flag = GridCell.GridFlag.CORNER;
                            current.rotation = new Vector3(0, 90, 0);
                        }
                    }

                    if (leftValid && downValid)
                    {
                        if (adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.OCCUPIED))
                        {
                            current.flag = GridCell.GridFlag.CORNER;
                            current.rotation = new Vector3(0, 180, 0);
                        }
                        if (adjacent[LEFT].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.flag = GridCell.GridFlag.CORNER;
                            current.rotation = Vector3.zero;
                        }
                    }

                    if (leftValid && rightValid)
                    {
                        if ((adjacent[LEFT].flag.Equals(GridCell.GridFlag.HALLWAY) || adjacent[RIGHT].flag.Equals(GridCell.GridFlag.HALLWAY) || adjacent[2].flag.Equals(GridCell.GridFlag.WALL) || adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALL)) && adjacent[UP].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.flag = GridCell.GridFlag.WALL;
                            current.rotation = new Vector3(0, 270, 0);
                        }
                        if ((adjacent[LEFT].flag.Equals(GridCell.GridFlag.HALLWAY) || adjacent[RIGHT].flag.Equals(GridCell.GridFlag.HALLWAY) || adjacent[2].flag.Equals(GridCell.GridFlag.WALL) || adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALL)) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.flag = GridCell.GridFlag.WALL;
                            current.rotation = new Vector3(0, 90, 0);
                        }
                    }

                    if (upValid && downValid)
                    {
                        if ((adjacent[UP].flag.Equals(GridCell.GridFlag.HALLWAY) || adjacent[DOWN].flag.Equals(GridCell.GridFlag.HALLWAY) || adjacent[UP].flag.Equals(GridCell.GridFlag.WALL) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALL)) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.flag = GridCell.GridFlag.WALL;
                            current.rotation = new Vector3(0, 180, 0);
                        }

                        if ((adjacent[UP].flag.Equals(GridCell.GridFlag.HALLWAY) || adjacent[DOWN].flag.Equals(GridCell.GridFlag.HALLWAY) || adjacent[UP].flag.Equals(GridCell.GridFlag.WALL) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALL)) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.flag = GridCell.GridFlag.WALL;
                            current.rotation = Vector3.zero;
                        }
                    }
                }
            }
        }
        #endregion


        #region Extended Smoothing
        // Sort out remaining hallway tiles
        for (int z = 0; z < grid.cells.z; z++)
        {
            for (int x = 0; x < grid.cells.x; x++)
            {
                var current = grid.grid[x, 0, z];
                var adjacent = GetAdjacentCells(current);
                var upValid = adjacent[UP] != null;
                var downValid = adjacent[DOWN] != null;
                var leftValid = adjacent[LEFT] != null;
                var rightValid = adjacent[RIGHT] != null;
                var upRightValid = adjacent[UP_RIGHT] != null;
                var upLeftValid = adjacent[UP_LEFT] != null;

                if (current.flag.Equals(GridCell.GridFlag.HALLWAY))
                {
                    if (leftValid && rightValid)
                    {
                        if (adjacent[LEFT].flag.Equals(GridCell.GridFlag.OCCUPIED) || adjacent[RIGHT].flag.Equals(GridCell.GridFlag.HALLWAY) || adjacent[LEFT].flag.Equals(GridCell.GridFlag.HALLWAY) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.OCCUPIED))
                        {
                            current.flag = GridCell.GridFlag.OCCUPIED;
                        }
                    }

                    if (leftValid && rightValid && upValid && downValid)
                    {
                        if (adjacent[UP].flag.Equals(GridCell.GridFlag.OCCUPIED) || adjacent[DOWN].flag.Equals(GridCell.GridFlag.OCCUPIED) || adjacent[LEFT].flag.Equals(GridCell.GridFlag.OCCUPIED) || adjacent[RIGHT].flag.Equals(GridCell.GridFlag.OCCUPIED))
                        {
                            current.flag = GridCell.GridFlag.OCCUPIED;
                        }
                    }
                }

            }
        }

        // Clean up
        for (int z = 0; z < grid.cells.z; z++)
        {
            for (int x = 0; x < grid.cells.x; x++)
            {
                var current = grid.grid[x, 0, z];
                var adjacent = GetAdjacentCells(current);
                var upValid = adjacent[UP] != null;
                var downValid = adjacent[DOWN] != null;
                var leftValid = adjacent[LEFT] != null;
                var rightValid = adjacent[RIGHT] != null;
                var upRightValid = adjacent[UP_RIGHT] != null;
                var upLeftValid = adjacent[UP_LEFT] != null;
                var downLeftValid = adjacent[DOWN_LEFT] != null;
                var downRightValid = adjacent[DOWN_RIGHT] != null;

                if (current.flag.Equals(GridCell.GridFlag.WALL))
                {
                    if (upValid && downLeftValid)
                    {
                        if (adjacent[UP].flag.Equals(GridCell.GridFlag.WALL) && adjacent[DOWN_LEFT].flag.Equals(GridCell.GridFlag.WALL))
                        {
                            current.rotation = new Vector3(0, 180, 0);
                        }
                    }
                    if (upValid && downRightValid && leftValid)
                    {
                        if (adjacent[UP].flag.Equals(GridCell.GridFlag.WALL) && adjacent[DOWN_RIGHT].flag.Equals(GridCell.GridFlag.WALL) && !adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.rotation = Vector3.zero;
                        }
                    }
                    if (upRightValid && leftValid && downValid)
                    {
                        if (adjacent[UP_RIGHT].flag.Equals(GridCell.GridFlag.CORNER) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.flag = GridCell.GridFlag.CORNER;
                            current.rotation = Vector3.zero;
                        }
                        if (adjacent[UP_RIGHT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.flag = GridCell.GridFlag.CORNER;
                            current.rotation = Vector3.zero;
                        }
                    }
                    if(upValid && leftValid && rightValid)
                    {
                        if(adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[UP].flag.Equals(GridCell.GridFlag.OCCUPIED))
                        {
                            current.flag = GridCell.GridFlag.CORNER;
                        }
                    }
                    if(upValid && upLeftValid)
                    {
                        if(adjacent[UP].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[UP_LEFT].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            adjacent[UP].flag = GridCell.GridFlag.WALL;
                            adjacent[UP].rotation = new Vector3(0, 180, 0);
                        }
                    }
                }


                if (current.flag.Equals(GridCell.GridFlag.CORNER))
                {
                    if (upValid && leftValid && rightValid && downLeftValid && downRightValid)
                    {
                        if (adjacent[LEFT].flag.Equals(GridCell.GridFlag.CORNER) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.CORNER) && adjacent[UP].flag.Equals(GridCell.GridFlag.WALL))
                        {
                            current.flag = GridCell.GridFlag.OCCUPIED;
                        }
                        if (adjacent[UP].flag.Equals(GridCell.GridFlag.CORNER) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALL))
                        {
                            current.flag = GridCell.GridFlag.WALL;
                            current.rotation = new Vector3(0, 270, 0);
                        }
                        if (adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[DOWN_LEFT].flag.Equals(GridCell.GridFlag.CORNER) && adjacent[DOWN_RIGHT].flag.Equals(GridCell.GridFlag.CORNER))
                        {
                            current.flag = GridCell.GridFlag.WALKABLE;
                            adjacent[DOWN].flag = GridCell.GridFlag.WALL;
                            adjacent[DOWN].rotation = new Vector3(0, 270, 0);
                        }
                    }
                    if (upValid && downValid && leftValid && rightValid)
                    {
                        if (adjacent[UP].flag.Equals(GridCell.GridFlag.WALL) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALL) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.OCCUPIED))
                        {
                            current.flag = GridCell.GridFlag.WALL;
                            current.rotation = new Vector3(0, 180, 0);
                        }
                    }
                    if (upValid && downValid && leftValid && upRightValid)
                    {
                        if (adjacent[UP].flag.Equals(GridCell.GridFlag.WALL) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.OCCUPIED) && !adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.flag = GridCell.GridFlag.OCCUPIED;
                            current.rotation = Vector3.zero;
                        }
                        if (adjacent[UP].flag.Equals(GridCell.GridFlag.WALL) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            adjacent[UP].flag = GridCell.GridFlag.WALKABLE;
                            adjacent[UP_RIGHT].flag = GridCell.GridFlag.WALL;
                            adjacent[UP_RIGHT].rotation = new Vector3(0, 180, 0);
                        }
                    }
                    if (rightValid && leftValid && upValid)
                    {
                        if (adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.CORNER) && adjacent[UP].flag.Equals(GridCell.GridFlag.OCCUPIED))
                        {
                            current.flag = GridCell.GridFlag.WALL;
                            current.rotation = new Vector3(0, 90, 0);
                        }
                    }
                    if (upValid && downValid && leftValid && rightValid)
                    {
                        if (adjacent[UP].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.OCCUPIED))
                        {
                            current.flag = GridCell.GridFlag.OCCUPIED;
                            current.rotation = new Vector3(0, 90, 0);
                        }
                    }
                    if (upValid && leftValid && downRightValid)
                    {
                        if (adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[UP].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[DOWN_RIGHT].flag.Equals(GridCell.GridFlag.CORNER))
                        {
                            current.flag = GridCell.GridFlag.WALL;
                            current.rotation = new Vector3(0, 90, 0);
                        }
                    }
                }
            }
        }

        for (int z = 0; z < grid.cells.z; z++)
        {
            for (int x = 0; x < grid.cells.x; x++)
            {
                var current = grid.grid[x, 0, z];
                var adjacent = GetAdjacentCells(current);
                var upValid = adjacent[UP] != null;
                var downValid = adjacent[DOWN] != null;
                var leftValid = adjacent[LEFT] != null;
                var rightValid = adjacent[RIGHT] != null;
                var upRightValid = adjacent[UP_RIGHT] != null;
                var upLeftValid = adjacent[UP_LEFT] != null;
                var downLeftValid = adjacent[DOWN_LEFT] != null;
                var downRightValid = adjacent[DOWN_RIGHT] != null;

                if(current.flag.Equals(GridCell.GridFlag.CORNER))
                {
                    if (rightValid && leftValid && upValid)
                    {
                        if (adjacent[UP].flag.Equals(GridCell.GridFlag.WALL) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            adjacent[UP].flag = GridCell.GridFlag.WALL;
                            adjacent[UP].rotation = Vector3.zero;
                        }
                    }
                    if(upValid && upRightValid && rightValid)
                    {
                        if(adjacent[UP].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[UP_RIGHT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.OCCUPIED))
                        {
                            adjacent[UP].flag = GridCell.GridFlag.CORNER;
                            adjacent[UP].rotation = new Vector3(0, 90, 0);
                            adjacent[UP_RIGHT].flag = GridCell.GridFlag.OCCUPIED;
                            adjacent[UP_RIGHT].rotation = Vector3.zero;
                        }
                    }
                    if(leftValid && rightValid && downValid)
                    {
                        if (adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.flag = GridCell.GridFlag.WALL;
                            current.rotation = new Vector3(0, 90, 0);
                        }
                    }
                }

                if(current.flag.Equals(GridCell.GridFlag.WALKABLE))
                {
                    if(downValid && leftValid && rightValid)
                    {
                        if(adjacent[DOWN].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.CORNER) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.CORNER))
                        {
                            adjacent[DOWN].flag = GridCell.GridFlag.WALL;
                            adjacent[DOWN].rotation = new Vector3(0, 270, 0);
                        }
                    }
                    if(downValid && leftValid && upLeftValid)
                    {
                        if(adjacent[LEFT].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[UP_LEFT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.CORNER))
                        {
                            adjacent[LEFT].flag = GridCell.GridFlag.WALL;
                        }
                    }
                    if(leftValid && downValid && downRightValid)
                    {
                        if(adjacent[LEFT].flag.Equals(GridCell.GridFlag.CORNER) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[DOWN_RIGHT].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            adjacent[DOWN].flag = GridCell.GridFlag.CORNER;
                            adjacent[DOWN].rotation = new Vector3(0, 270, 0);
                        }
                    }
                }

                if(current.flag.Equals(GridCell.GridFlag.WALL))
                {
                    if(rightValid && leftValid && downRightValid)
                    {
                        if(adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[DOWN_RIGHT].flag.Equals(GridCell.GridFlag.CORNER))
                        {
                            current.flag = GridCell.GridFlag.CORNER;
                        }
                    }

                    if(leftValid && rightValid && downValid)
                    {
                        if(adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.rotation = new Vector3(0, 90, 0);
                        }
                    }
                }
            }
        }
        #endregion
}

    void BakeNavmesh()
    {
        Debug.Log("Baking navmesh");
        var navmesh = FindObjectsOfType<NavMeshSurface>();
        foreach(NavMeshSurface surface in navmesh)
        {
            surface.BuildNavMesh();
        }
        Debug.Log("Baked navmesh");
    }

    /// <summary>
    /// Returns the cells "up, down, left, right, upLeft, upRight, downLeft, downRight" to the current cell
    /// </summary>
    /// <param name="current"></param>
    /// <returns></returns>
    GridCell[] GetAdjacentCells(GridCell current)
    {
        GridCell up = navAgent.GetGridCellAt((int)current.position.x, (int)current.position.y, (int)current.position.z + 1);
        GridCell down = navAgent.GetGridCellAt((int)current.position.x, (int)current.position.y, (int)current.position.z - 1);
        GridCell left = navAgent.GetGridCellAt((int)current.position.x - 1, (int)current.position.y, (int)current.position.z);
        GridCell right = navAgent.GetGridCellAt((int)current.position.x + 1, (int)current.position.y, (int)current.position.z);
        GridCell upLeft = navAgent.GetGridCellAt((int)current.position.x - 1, (int)current.position.y, (int)current.position.z + 1);
        GridCell upRight = navAgent.GetGridCellAt((int)current.position.x + 1, (int)current.position.y, (int)current.position.z + 1);
        GridCell downLeft = navAgent.GetGridCellAt((int)current.position.x - 1, (int)current.position.y, (int)current.position.z - 1);
        GridCell downRight = navAgent.GetGridCellAt((int)current.position.x + 1, (int)current.position.y, (int)current.position.z - 1);
        return new GridCell[] { up, down, left, right, upLeft, upRight, downLeft, downRight };
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

#if UNITY_EDITOR
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
                if (room.hallways.Count > 0)
                {
                    for (int j = 1; j < room.hallways.Count - 1; j++)
                    {
                        Gizmos.DrawSphere(room.hallways[j].position, 0.25f);
                    }
                }
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(room.hallways[0].position, 0.25f);
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(room.hallways[room.hallways.Count - 1].position, 0.25f);
            }
        }
    }
#endif
}

public class CellInfo : MonoBehaviour
{
    public Vector3 cellRotation = Vector3.zero;
}