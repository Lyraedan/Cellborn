using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class RoomGenerator : MonoBehaviour
{
    public static RoomGenerator instance;

    public Transform roomParent;
    public Grid grid;
    [Header("Dungeon settings")]
    public int seed = 0;
    public int levelIndex = 0;
    public int numberOfLevels = 3;
    [SerializeField] private int[] levels;
    public Vector2 minDungeonSize = new Vector2(10, 10);
    public Vector2 maxDungeonSize = new Vector2(25, 25);
    public Vector2 minRoomSize = new Vector2(3, 6);
    [Tooltip("This gets updated at runtime")] public Vector3 generatedDungeonSize = Vector3.zero;
    public int maxRoomLimit = 10;
    public GameObject wizard;
    public GameObject teleporter;
    public GameObject prisonCell;
    public List<Room> rooms = new List<Room>();

    private Room start, end;

    public List<RoomPrefab> prefabs = new List<RoomPrefab>();

    private int failedAttempts = 0;

    public GradedPath navAgent;
    public TargetMovement targetAim;
    public bool enableCulling = true;

    //Adjacent directions
    [HideInInspector] public const int UP = 0;
    [HideInInspector] public const int DOWN = 1;
    [HideInInspector] public const int LEFT = 2;
    [HideInInspector] public const int RIGHT = 3;
    [HideInInspector] public const int UP_LEFT = 4;
    [HideInInspector] public const int UP_RIGHT = 5;
    [HideInInspector] public const int DOWN_LEFT = 6;
    [HideInInspector] public const int DOWN_RIGHT = 7;

    public int entitySpawnRate = 20;

    public NavMeshSurface[] navmesh;
    public RoomMeshGenerator floorMesh, wallMesh, roofMesh;

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
        levels = new int[numberOfLevels];
        for(int i = 0; i < numberOfLevels; i++)
        {
            if (seed == 0)
                levels[i] = Random.Range(0, 1000000);
            else
                levels[i] = seed + i;
        }

        Generate(levels[levelIndex]);
    }

    void Generate(int seed)
    {
        Random.InitState(seed);
        Debug.Log("Generating dungeon with seed: " + seed);
        generatedDungeonSize = GenerateRandomVector((int)minDungeonSize.x, 0, (int)minDungeonSize.y, (int)maxDungeonSize.x, 1, (int)maxDungeonSize.y);
        Debug.Log("Generated dungeon of size: " + generatedDungeonSize.ToString());

        var dimensions = new Vector3(generatedDungeonSize.x, 1, generatedDungeonSize.z);
        grid.cells = dimensions;
        grid.Init();

        int limit = Mathf.RoundToInt(maxDungeonSize.x / minRoomSize.x + maxDungeonSize.y / minRoomSize.y);
        Debug.Log("Calculated the ability to fit " + limit + " rooms");

        for (int i = 0; i < (maxRoomLimit == 0 ? limit : maxRoomLimit); i++)
        {
            GenerateRandomRoom();
        }

        if (rooms.Count == 0)
        {
            Debug.LogError("No rooms were generated!");
        }
        ConnectRooms();
        try
        {
            ShapeHallways();
            Smooth();
        }
        catch (System.Exception e)
        {
            Regenerate();
            return;
        }

        rooms = rooms.OrderBy(room => room.centre.magnitude).ToList();
        start = rooms[0];
        end = rooms[rooms.Count - 1];

        FlagProps();
        FlagEntities();

        floorMesh.GenerateFloor(grid);
        wallMesh.GenerateWalls(floorMesh);
        roofMesh.GenerateCeiling(floorMesh);
        PlaceLighting(floorMesh.edgeVertices);

        BakeNavmesh();

        //PlaceProps();
        if (levelIndex == 0)
        {
            Vector3 startCoords = PositionAsGridCoordinates(start.centre);
            GridCell startPoint = navAgent.GetGridCellAt((int)startCoords.x, (int)startCoords.y, (int)startCoords.z);
            var player = SpawnPlayer(startPoint);
            CameraManager.instance.main.gameObject.GetComponent<CameraFollow>().player = player;
        } else
        {
            // Spawn teleporter back?
        }

        Vector3 endCords = PositionAsGridCoordinates(end.centre);
        GridCell endPoint = navAgent.GetGridCellAt((int)endCords.x, (int)endCords.y, (int)endCords.z);
        if (levelIndex == numberOfLevels - 1)
        {
            var boss = SpawnWizard(endPoint);
            var bossAI = boss.GetComponent<AIWizard>();
            bossAI.bindingPoint = endCords;
        } else
        {
            // Generate teleporter
            var teleporter = SpawnTeleporter(levelIndex + 1, endPoint);
        }

        StartCoroutine(AwaitAssignables());
    }

    private void PlaceLighting(List<RoomMeshGenerator.Edge> edgeVertices)
    {
        // Wall lights
        for(int i = 0; i < edgeVertices.Count; i++)
        {
            var spawn = i % 10 == 0;
            if (spawn)
            {
                var light = GetRandomLight();
                if (light == null)
                {
                    Debug.LogError("No light prefab found!");
                    break;
                }
                var position = floorMesh.transform.position + edgeVertices[i].origin;
                position.y += (wallMesh.wallHeight / 2) + 0.5f;
                var direction = edgeVertices[i].DirectionAsVector3();
                SpawnPrefab(light, position, direction);
            }
        }

        // Room center lights here
        for(int i = 0; i < rooms.Count; i++)
        {
            var light = GetRandomCeilingLight();
            if (light == null)
            {
                Debug.LogError("No light prefab found!");
                break;
            }
            var position = rooms[i].centre;
            position.y = wallMesh.wallHeight - 0.5f;
            SpawnPrefab(light, position, Vector3.zero);
        }
    }

    void ClearDungeon()
    {
        DeleteAllObjectsWithTag("Weapon");
        DeleteAllObjectsWithTag("Prop");
        DeleteAllObjectsWithTag("Enemy");
    }

    public void Regenerate()
    {
        //ClearDungeon();
        //DeleteAllObjectsWithTag("Environment");
        //DeleteAllObjectsWithTag("Player");
        //Generate(seed);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Regenerate();
        }
    }

    void DeleteAllObjectsWithTag(string tag)
    {
        var list = GameObject.FindGameObjectsWithTag(tag);
        for (int i = 0; i < list.Length; i++)
        {
            Destroy(list[i]);
        }
    }

    IEnumerator AwaitAssignables()
    {
        yield return new WaitUntil(() => WeaponManager.instance != null);
        //Grab weapons
        WeaponManager.instance.GetWeaponsInLevel();
        Minimap.instance.GenerateMinimap(grid);
        PlaceEntities();

        // Matilde if you need to. Start behaviour tree's beyond this point.

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
    public GameObject SpawnRandomProp(GridCell cell)
    {
        var prop = prefabs.Where(e => e.type.Equals(RoomPrefab.RoomPropType.PROP)).ToList();
        int index = Random.Range(0, prop.Count);
        return prop[index].Spawn(cell.position, cell.rotation);
    }

    public GameObject GetRandomLight()
    {
        var prop = prefabs.Where(e => e.type.Equals(RoomPrefab.RoomPropType.WALL_LIGHT)).ToList();
        if(prop.Count <= 0)
            return null;

        int index = Random.Range(0, prop.Count);
        return prop[index].prefab;
    }

    public GameObject GetRandomCeilingLight()
    {
        var prop = prefabs.Where(e => e.type.Equals(RoomPrefab.RoomPropType.CEILING_LIGHT)).ToList();
        if (prop.Count <= 0)
            return null;

        int index = Random.Range(0, prop.Count);
        return prop[index].prefab;
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
        var dungeonCellPosition = pos;
        dungeonCellPosition.z -= 19.5f;
        dungeonCellPosition.y += 1f;
        dungeonCellPosition.x += 1f;
        Instantiate(prisonCell, dungeonCellPosition, Quaternion.identity);
        return player.Spawn(pos, Vector3.zero);
    }

    public GameObject SpawnWizard(GridCell cell)
    {
        var pos = cell.position;
        pos.y += 0.5f;
        return Instantiate(wizard, pos, Quaternion.identity);
    }

    public GameObject SpawnTeleporter(int nextLevelIndex, GridCell cell)
    {
        var pos = cell.position;
        pos.y += -0.5f;
        var teleporterObject = Instantiate(teleporter, pos, Quaternion.identity);
        var teleport = teleporterObject.GetComponent<Teleporter>();
        teleport.OnTriggered += () =>
        {
            ClearDungeon();
            DeleteAllObjectsWithTag("Environment");
            Generate(levels[nextLevelIndex]);
        };
        return teleporterObject;
    }
    #endregion

    #region Population
    void PlaceEntities()
    {
        for (int x = 0; x < grid.cells.x; x++)
        {
            for (int z = 0; z < grid.cells.z; z++)
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
        for (int z = 0; z < grid.cells.z; z++)
        {
            for (int x = 0; x < grid.cells.x; x++)
            {
                var cell = grid.grid[x, 0, z];
                bool isInPrisonCell = CellisInPrisonCell(cell);

                if (cell.flag.Equals(GridCell.GridFlag.OCCUPIED) && !isInPrisonCell)
                {
                    bool spawnProp = (Random.Range(0, 10) == 0);
                    cell.hasProp = spawnProp;
                }
            }
        }
    }

    void FlagEntities()
    {
        for (int z = 0; z < grid.cells.z; z++)
        {
            for (int x = 0; x < grid.cells.x; x++)
            {
                var cell = grid.grid[x, 0, z];
                bool isInStart = CellIsInRoom(cell, 0);
                bool isInHallway = false;
                for(int i = 0; i < rooms.Count; i++)
                {
                    bool hallwayCheck = CellIsInHallway(cell, rooms[i]);
                    if(hallwayCheck)
                    {
                        isInHallway = true;
                        break;
                    }
                }
                if (cell.flag.Equals(GridCell.GridFlag.OCCUPIED) && !isInStart && !isInHallway)
                {
                    if (!cell.hasProp)
                    {
                        bool spawnEntity = (Random.Range(0, entitySpawnRate) == 0);
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
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            navAgent.src = rooms[i].centre;
            navAgent.dest = rooms[i + 1].centre;
            navAgent.CalculatePath();
            foreach (GridCell cell in navAgent.open)
            {
                if (cell.flag.Equals(GridCell.GridFlag.WALKABLE) || cell.flag.Equals(GridCell.GridFlag.WALL))
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
        for (int i = 0; i < rooms.Count; i++)
        {
            var room = rooms[i];
            if (room.hallways.Count >= 2)
            {
                for (int j = 0; j < room.hallways.Count - 1; j++)
                {
                    GridCell current = room.hallways[j];
                    GridCell next = room.hallways[j + 1];

                    // TODO DO FUCKING ADJACENT CHECKS YOU MUPPPET - Past and Lazy Luke <3
                    var adjacent = GetAdjacentCells(current);

                    int girth = 1;
                    int cx = (int)current.position.x - girth;
                    int cz = (int)current.position.z - girth;
                    int nx = (int)next.position.x + girth;
                    int nz = (int)next.position.z + girth;

                    // Diagonals
                    if (nx > cx)
                    {
                        if (adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALKABLE) || adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALL))
                            adjacent[RIGHT].flag = GridCell.GridFlag.HALLWAY;
                    }
                    if (nx < cx)
                    {
                        if (adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALKABLE) || adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL))
                            adjacent[LEFT].flag = GridCell.GridFlag.HALLWAY;
                    }
                    if (nz < cz)
                    {
                        if (adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALKABLE) || adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALL))
                            adjacent[DOWN].flag = GridCell.GridFlag.HALLWAY;
                    }
                    if (nz > cz)
                    {
                        if (adjacent[UP].flag.Equals(GridCell.GridFlag.WALKABLE) || adjacent[UP].flag.Equals(GridCell.GridFlag.WALL))
                            adjacent[UP].flag = GridCell.GridFlag.HALLWAY;
                    }
                    // Straight up, down, left, right
                    if (adjacent[UP].flag.Equals(GridCell.GridFlag.WALKABLE) || adjacent[UP].flag.Equals(GridCell.GridFlag.WALL))
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
        List<GridCell> flagged = new List<GridCell>();
        int[] possibleRotations = new int[] { 270, 90, 180, 0 };

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
                    if (upValid && leftValid && rightValid)
                    {
                        if (adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[UP].flag.Equals(GridCell.GridFlag.OCCUPIED))
                        {
                            current.flag = GridCell.GridFlag.CORNER;
                        }
                    }
                    if (upValid && upLeftValid)
                    {
                        if (adjacent[UP].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[UP_LEFT].flag.Equals(GridCell.GridFlag.WALKABLE))
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

                if (current.flag.Equals(GridCell.GridFlag.CORNER))
                {
                    if (rightValid && leftValid && upValid)
                    {
                        if (adjacent[UP].flag.Equals(GridCell.GridFlag.WALL) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            adjacent[UP].flag = GridCell.GridFlag.WALL;
                            adjacent[UP].rotation = Vector3.zero;
                        }
                    }
                    if (upValid && upRightValid && rightValid)
                    {
                        if (adjacent[UP].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[UP_RIGHT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.OCCUPIED))
                        {
                            adjacent[UP].flag = GridCell.GridFlag.CORNER;
                            adjacent[UP].rotation = new Vector3(0, 90, 0);
                            adjacent[UP_RIGHT].flag = GridCell.GridFlag.OCCUPIED;
                            adjacent[UP_RIGHT].rotation = Vector3.zero;
                        }
                    }
                    if (leftValid && rightValid && downValid)
                    {
                        if (adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.flag = GridCell.GridFlag.WALL;
                            current.rotation = new Vector3(0, 90, 0);
                        }
                    }
                }

                if (current.flag.Equals(GridCell.GridFlag.WALKABLE))
                {
                    if (downValid && leftValid && rightValid)
                    {
                        if (adjacent[DOWN].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.CORNER) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.CORNER))
                        {
                            adjacent[DOWN].flag = GridCell.GridFlag.WALL;
                            adjacent[DOWN].rotation = new Vector3(0, 270, 0);
                        }
                    }
                    if (downValid && leftValid && upLeftValid)
                    {
                        if (adjacent[LEFT].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[UP_LEFT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.CORNER))
                        {
                            adjacent[LEFT].flag = GridCell.GridFlag.WALL;
                        }
                    }
                    if (leftValid && downValid && downRightValid)
                    {
                        if (adjacent[LEFT].flag.Equals(GridCell.GridFlag.CORNER) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[DOWN_RIGHT].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            adjacent[DOWN].flag = GridCell.GridFlag.CORNER;
                            adjacent[DOWN].rotation = new Vector3(0, 270, 0);
                        }
                    }
                }

                if (current.flag.Equals(GridCell.GridFlag.WALL))
                {
                    if (rightValid && leftValid && downRightValid)
                    {
                        if (adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[DOWN_RIGHT].flag.Equals(GridCell.GridFlag.CORNER))
                        {
                            current.flag = GridCell.GridFlag.CORNER;
                        }
                    }

                    if (leftValid && rightValid && downValid)
                    {
                        if (adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.rotation = new Vector3(0, 90, 0);
                        }
                    }
                }
            }
        }
        #endregion

        #region Corner fixing
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
                var downRightValid = adjacent[DOWN_RIGHT] != null;
                var downLeftValid = adjacent[DOWN_LEFT] != null;

                if (upValid && downValid)
                {
                    if (current.flag.Equals(GridCell.GridFlag.CORNER))
                    {
                        if (adjacent[UP].flag.Equals(GridCell.GridFlag.CORNER) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.CORNER))
                        {
                            if (leftValid && rightValid)
                            {
                                if ((adjacent[LEFT].flag.Equals(GridCell.GridFlag.OCCUPIED) || adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL)) && (adjacent[RIGHT].flag.Equals(GridCell.GridFlag.OCCUPIED) || adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALL)))
                                {
                                    current.flag = GridCell.GridFlag.OCCUPIED;
                                }
                                else
                                {
                                    if (!current.flag.Equals(GridCell.GridFlag.OCCUPIED))
                                        flagged.Add(current);
                                }
                            }
                            else
                            {
                                if (!current.flag.Equals(GridCell.GridFlag.OCCUPIED))
                                    flagged.Add(current);
                            }
                        }
                    }
                    else if (current.flag.Equals(GridCell.GridFlag.OCCUPIED))
                    {
                        for(int i = 0; i < 4; i++)
                        {
                            if (adjacent[i] != null)
                            {
                                if (adjacent[i].flag.Equals(GridCell.GridFlag.WALKABLE))
                                {
                                    current.flag = GridCell.GridFlag.WALL;
                                    current.rotation = new Vector3(0, possibleRotations[i], 0);
                                }
                            }
                        }
                    }
                }

                if(current.flag.Equals(GridCell.GridFlag.WALL))
                {
                    if(rightValid && downValid && downLeftValid && upValid)
                    {
                        if(adjacent[UP].flag.Equals(GridCell.GridFlag.WALL) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[DOWN_LEFT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.flag = GridCell.GridFlag.CORNER;
                            current.rotation = Vector3.zero;
                        }
                    }
                    if(leftValid && downValid && downRightValid)
                    {
                        if(adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[DOWN_RIGHT].flag.Equals(GridCell.GridFlag.WALL))
                        {
                            current.rotation = new Vector3(0, 90, 0);
                        }
                    }
                    if(upValid && rightValid)
                    {
                        if(adjacent[UP].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.flag = GridCell.GridFlag.CORNER;
                            current.rotation = new Vector3(0, 270, 0);
                        }
                    }
                    if (upValid && leftValid)
                    {
                        if (adjacent[UP].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALKABLE))
                        {
                            current.flag = GridCell.GridFlag.CORNER;
                            current.rotation = new Vector3(0, 180, 0);
                        }
                    }
                }
            }
        }

        foreach(GridCell cell in flagged)
        {
            cell.flag = GridCell.GridFlag.WALL;
            var adjacent = GetAdjacentCells(cell);

            var upValid = adjacent[UP] != null;
            var downValid = adjacent[DOWN] != null;
            var leftValid = adjacent[LEFT] != null;
            var rightValid = adjacent[RIGHT] != null;

            if(leftValid)
            {
                if(adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALKABLE))
                {
                    cell.rotation = new Vector3(0, 180, 0);
                }
            }

            if(rightValid)
            {
                if (adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALKABLE))
                {
                    cell.rotation = Vector3.zero;
                }
            }
        }
        flagged.Clear();

        // TODO needs looking at not correct - You get better results without this code

        for (int z = 0; z < grid.cells.z; z++)
        {
            for (int x = 0; x < grid.cells.x; x++)
            {
                var current = grid.grid[x, 0, z];
                var adjacent = GetAdjacentCells(current);

                var upValid = adjacent[UP] != null;
                var leftValid = adjacent[LEFT] != null;
                var downValid = adjacent[DOWN] != null;
                var rightValid = adjacent[RIGHT] != null;
                var upLeftValid = adjacent[UP_LEFT] != null;
                var upRightValid = adjacent[UP_RIGHT] != null;
                var downLeftValid = adjacent[DOWN_LEFT] != null;
                var downRightValid = adjacent[DOWN_RIGHT] != null;

                if (current.flag.Equals(GridCell.GridFlag.CORNER))
                {
                    if(upLeftValid && rightValid)
                    {
                        if(adjacent[UP_LEFT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALL))
                        {
                            current.rotation = new Vector3(0, 90, 0);
                        }
                    }
                    if (upValid && downValid && leftValid && rightValid)
                    {
                        if (adjacent[UP].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALL) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALL))
                        {
                            current.flag = GridCell.GridFlag.OCCUPIED;
                            current.rotation = Vector3.zero;
                        }
                    }
                    if(upValid && downValid && leftValid && rightValid && downLeftValid)
                    {
                        if(adjacent[UP].flag.Equals(GridCell.GridFlag.WALL) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[DOWN_LEFT].flag.Equals(GridCell.GridFlag.CORNER))
                        {
                            current.flag = GridCell.GridFlag.WALL;
                            current.rotation = new Vector3(0, 180, 0);
                        }
                        if(adjacent[UP].flag.Equals(GridCell.GridFlag.WALL) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.OCCUPIED))
                        {
                            current.flag = GridCell.GridFlag.WALL;
                            current.rotation = new Vector3(0, 180, 0);
                        }
                    }
                    if(upValid && downValid && leftValid && rightValid && upRightValid && downLeftValid)
                    {
                        if (adjacent[UP].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.OCCUPIED) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[UP_RIGHT].flag.Equals(GridCell.GridFlag.WALL))
                        {
                            // Better do rotation checks ^^^
                            // WTF was I doing here
                            //adjacent[UP_RIGHT].flag = GridCell.GridFlag.WALL;
                            //adjacent[UP_RIGHT].rotation = new Vector3(0, 90, 0);
                        }
                    }
                    if (downValid && leftValid && rightValid)
                    {
                        if(adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.CORNER))
                        {
                            current.flag = GridCell.GridFlag.WALKABLE;
                            adjacent[RIGHT].flag = GridCell.GridFlag.CORNER;
                            adjacent[RIGHT].rotation = new Vector3(0, 180, 0);
                        }
                    }
                }
                else if (current.flag.Equals(GridCell.GridFlag.WALL))
                {
                    if (downValid && upRightValid && leftValid && upValid)
                    {
                        if (adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALL) && adjacent[UP_RIGHT].flag.Equals(GridCell.GridFlag.WALL) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALKABLE) && !adjacent[UP].flag.Equals(GridCell.GridFlag.CORNER))
                        {
                            current.flag = GridCell.GridFlag.OCCUPIED;
                            current.rotation = new Vector3(0, 180, 0);
                        }
                    }
                    if(upValid && downValid && leftValid && rightValid)
                    {
                        if(adjacent[UP].flag.Equals(GridCell.GridFlag.CORNER) && adjacent[LEFT].flag.Equals(GridCell.GridFlag.WALKABLE) && adjacent[DOWN].flag.Equals(GridCell.GridFlag.WALL) && adjacent[RIGHT].flag.Equals(GridCell.GridFlag.OCCUPIED))
                        {
                            // Better do rotation checks ^^^
                            //current.flag = GridCell.GridFlag.CORNER;
                            //current.rotation = new Vector3(0, 180, 0);
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
        foreach (NavMeshSurface surface in navmesh)
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
    public GridCell[] GetAdjacentCells(GridCell current)
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

    /// <summary>
    /// Check if a tile is adjacent
    /// </summary>
    /// <param name="current">The grid cell</param>
    /// <param name="flag">What are we looking for</param>
    /// <param name="mode">0 - All, 1 - Plus shape, 2 - Cross shape</param>
    /// <returns></returns>
    public bool TileIsAdjacent(GridCell current, GridCell.GridFlag flag, int mode = 0)
    {
        var adjacent = GetAdjacentCells(current);
        switch (mode) {
            case 0:
                for(int i = 0; i < adjacent.Length; i++)
                {
                    if (adjacent[i] != null)
                    {
                        if (adjacent[i].flag.Equals(flag))
                            return true;
                    }
                }
                break;
            case 1:
                for (int i = 0; i < 4; i++)
                {
                    if (adjacent[i] != null)
                    {
                        if (adjacent[i].flag.Equals(flag))
                            return true;
                    }
                }
                break;
            case 2:
                for (int i = 4; i < adjacent.Length; i++)
                {
                    if (adjacent[i] != null)
                    {
                        if (adjacent[i].flag.Equals(flag))
                            return true;
                    }
                }
                break;
            default:
                for (int i = 0; i < adjacent.Length; i++)
                {
                    if (adjacent[i] != null)
                    {
                        if (adjacent[i].flag.Equals(flag))
                            return true;
                    }
                }
                break;
        }
        return false;
    }

    bool GenerateRoom(Vector3 pos, Vector3 roomDimensions)
    {
        var width = grid.cells.x;
        var length = grid.cells.z;

        if (pos.x < 0 || pos.z < 0 || pos.x + roomDimensions.x >= width || pos.z + roomDimensions.z >= length)
        {
            Debug.LogWarning("Room size exceeds grid size!");
            return false;
        }

        Room room = new Room();
        for (int x = (int)pos.x; x < pos.x + roomDimensions.x; x++)
        {
            for (int z = (int)pos.z; z < pos.z + roomDimensions.z; z++)
            {
                if (grid.grid[x, 0, z].flag.Equals(GridCell.GridFlag.OCCUPIED))
                {
                    Debug.LogWarning("Overlapping rooms!");
                    //break;
                }

                //Flag walls
                if (x <= pos.x || x >= pos.x + (roomDimensions.x - 1) ||
                   z <= pos.z || z >= pos.z + (roomDimensions.z - 1))
                {
                    // These are walls
                    grid.grid[x, 0, z].flag = GridCell.GridFlag.WALL;
                    room.walls.Add(grid.grid[x, 0, z]);
                }
                else
                {
                    grid.grid[x, 0, z].flag = GridCell.GridFlag.OCCUPIED;

                    room.occupied.Add(grid.grid[x, 0, z]);
                }
            }
        }
        room.centre = new Vector3(pos.x + (roomDimensions.x / 2), 0, pos.z + (roomDimensions.z / 2));

        rooms.Add(room);
        if (rooms.Count > 1)
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

    private bool CellIsInRoom(GridCell current, int roomIndex)
    {
        if (roomIndex < 0)
            return false;
        else if (roomIndex >= rooms.Count)
            return false;

        return rooms[roomIndex].occupied.Contains(current) || rooms[roomIndex].hallways.Contains(current) || rooms[roomIndex].walls.Contains(current);
    }

    private bool CellIsInHallway(GridCell current, Room room)
    {
        return room.hallways.Contains(current);
    }

    private bool CellisInPrisonCell(GridCell current)
    {
        Vector3 playerCoords = PositionAsGridCoordinates(start.centre);
        for(int z = -3; z < 4; z++)
        {
            for(int x = -3; x < 4; x++)
            {
                GridCell cell = navAgent.GetGridCellAt((int)playerCoords.x + x, (int)playerCoords.y, (int)playerCoords.z + z);
                if(current.position.Equals(cell.position))
                {
                    return true;
                }
            }
        }
        return false;
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

        for (int i = 0; i < rooms.Count; i++)
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