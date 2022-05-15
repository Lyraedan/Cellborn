using Newtonsoft.Json;
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
    [Header("Prop rates")]
    public Vector2 wallPropRate = new Vector2(30, 50);
    public Vector2 centrePropRate = new Vector2(30, 50);

    public List<Room> rooms = new List<Room>();
    public GameObject floorPrefab, wallPrefab, ceilingPrefab, environmentRootPrefab, environment;

    [Space(10)]
    public GameObject wizard;
    public GameObject teleporter;
    public GameObject prisonCell;
    private GameObject spawnCell;
    private GameObject levelTeleporter;

    private Room start, end;

    [Space(5)]
    public List<RoomPrefab> prefabs = new List<RoomPrefab>();

    private int failedAttempts = 0;

    public GradedPath navAgent;
    public TargetMovement targetAim;
    public bool enableCulling = true;

    private GameObject player;
    private PlayerMovementTest controller;

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
        for (int i = 0; i < numberOfLevels; i++)
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
        //generatedDungeonSize = GenerateRandomVector((int)minDungeonSize.x, 0, (int)minDungeonSize.y, (int)maxDungeonSize.x, 1, (int)maxDungeonSize.y);
        generatedDungeonSize = new Vector3(76, 0, 76);
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

        CleanupRooms();
        CarveHallways();
        CleanupHallways();

        rooms = rooms.OrderBy(room => room.centres[0].magnitude).ToList();
        start = rooms[0];
        end = rooms[rooms.Count - 1];

        FlagProps();
        FlagEntities();

        Destroy(floorMesh.gameObject);
        Destroy(wallMesh.gameObject);
        Destroy(roofMesh.gameObject);
        Destroy(environment);

        floorMesh = Instantiate(floorPrefab, transform).GetComponent<RoomMeshGenerator>();
        wallMesh = Instantiate(wallPrefab, transform).GetComponent<RoomMeshGenerator>();
        roofMesh = Instantiate(ceilingPrefab, transform).GetComponent<RoomMeshGenerator>();
        environment = Instantiate(environmentRootPrefab, transform);

        floorMesh.GenerateFloor(grid);
        wallMesh.GenerateWalls(floorMesh);
        roofMesh.GenerateCeiling(floorMesh);
        PlaceLighting(floorMesh.edgeVertices);
        PlaceProps(floorMesh.edgeVertices);

        navmesh = new NavMeshSurface[1];
        navmesh[0] = floorMesh.gameObject.GetComponent<NavMeshSurface>();

        Vector3 startCoords = PositionAsGridCoordinates(start.centres[0]);
        GridCell startPoint = navAgent.GetGridCellAt((int)startCoords.x, (int)startCoords.y, (int)startCoords.z);

        //PlaceProps();
        Debug.Log("Loading level: " + levelIndex);
        if (levelIndex == 0)
        {
            player = SpawnPlayer(startPoint);
            controller = player.GetComponent<PlayerMovementTest>();
            CameraManager.instance.main.gameObject.GetComponent<CameraFollow>().player = player;
            player.name = "Player";
            player.transform.SetParent(null);
        }
        else
        {
            // Spawn teleporter back?
            var position = startPoint.position;
            position.y += 0.5f;
            controller.TeleportPlayer(position);
        }

        BakeNavmesh();

        Vector3 endCords = PositionAsGridCoordinates(end.centres[0]);
        GridCell endPoint = navAgent.GetGridCellAt((int)endCords.x, (int)endCords.y, (int)endCords.z);
        if (levelIndex == numberOfLevels - 1)
        {
            var boss = SpawnWizard(endPoint);
            var bossAI = boss.GetComponent<AIWizard>();
        }
        else
        {
            // Generate teleporter
            levelTeleporter = SpawnTeleporter(endPoint);
        }

        StartCoroutine(AwaitAssignables());
    }

    private void PlaceLighting(List<RoomMeshGenerator.Edge> edgeVertices)
    {
        // Wall lights
        for (int i = 0; i < edgeVertices.Count; i++)
        {
            var range = UnityEngine.Random.Range(10, 25);
            var spawn = i % range == 0;
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
                var l = SpawnPrefab(light, position, direction);
                l.transform.SetParent(environment.transform);
            }
        }

        // Room center lights here
        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = 0; j < rooms[i].centres.Count; j++)
            {
                var light = GetRandomCeilingLight();
                if (light == null)
                {
                    Debug.LogError("No light prefab found!");
                    break;
                }
                var position = rooms[i].centres[j];
                position.y = wallMesh.wallHeight - 0.5f;
                var l = SpawnPrefab(light, position, Vector3.zero);
                l.transform.SetParent(environment.transform);
            }
        }
    }

    void ClearDungeon()
    {
        DeleteAllObjectsWithTag("Weapon");
        DeleteAllObjectsWithTag("Prop");
        DeleteAllObjectsWithTag("Enemy");
        // Reset grid
        for (int z = 0; z < grid.cells.z; z++)
        {
            for (int x = 0; x < grid.cells.x; x++)
            {
                var current = grid.grid[x, 0, z];
                current.rotation = Vector3.zero;
                current.flag = GridCell.GridFlag.WALKABLE;
            }
        }

        if (spawnCell != null)
        {
            Destroy(spawnCell);
        }
    }

    void OnTeleport()
    {
        int next = levelIndex + 1;
        Debug.Log("Do teleport! from " + levelIndex + " to " + next);
        levelIndex = next;
        ClearDungeon();
        DeleteAllObjectsWithTag("Environment");
        if (levelTeleporter != null)
            Destroy(levelTeleporter);

        // Is holding grapple hook
        if(WeaponManager.instance.currentWeapon.weaponId == 4)
        {
            if(WeaponManager.instance.currentWeapon.functionality != null)
            {
                // This is fuckin dumb
                WeaponGrapple weaponGrapple = (WeaponGrapple) WeaponManager.instance.currentWeapon.functionality;
                var grapple = weaponGrapple.grapple;

                if (grapple != null)
                {
                    if(grapple.isPulling)
                    {
                        grapple.RetrieveHook();
                    }
                }
                
            }
        }

        rooms.Clear();
        Generate(levels[levelIndex]);
    }

    public void Regenerate()
    {
        // Reloads scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Regenerate();
        } else if(Input.GetKeyDown(KeyCode.L))
        {
            controller.TeleportPlayerToRandomPoint();
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
        if (levelIndex == 0)
        {
            WeaponManager.instance.GetWeaponsInLevel();
        }
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
    public GameObject GetRandomLight()
    {
        var prop = prefabs.Where(e => e.type.Equals(RoomPrefab.RoomPropType.WALL_LIGHT)).ToList();
        if (prop.Count <= 0)
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

    public GameObject GetRandomProp()
    {
        var prop = prefabs.Where(e => e.type.Equals(RoomPrefab.RoomPropType.WALL_PROP)).ToList();
        if (prop.Count <= 0)
            return null;

        int index = Random.Range(0, prop.Count);
        return prop[index].prefab;
    }

    public GameObject GetRandomCentreProp()
    {
        var prop = prefabs.Where(e => e.type.Equals(RoomPrefab.RoomPropType.CENTER_PROP)).ToList();
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
        spawnCell = Instantiate(prisonCell, dungeonCellPosition, Quaternion.identity);
        return player.Spawn(pos, Vector3.zero);
    }

    public GameObject SpawnWizard(GridCell cell)
    {
        var pos = cell.position;
        pos.y += 0.5f;
        return Instantiate(wizard, pos, Quaternion.identity);
    }

    public GameObject SpawnTeleporter(GridCell cell)
    {
        var pos = cell.position;
        pos.y += -0.5f;
        var teleporterObject = Instantiate(teleporter, pos, Quaternion.identity);
        var teleport = teleporterObject.transform.Find("Collider").gameObject.GetComponent<Teleporter>();
        teleport.OnTriggered += OnTeleport;
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

    void PlaceProps(List<RoomMeshGenerator.Edge> edgeVertices)
    {
        // Wall lights
        for (int i = 0; i < edgeVertices.Count; i++)
        {
            var range = UnityEngine.Random.Range(wallPropRate.x, wallPropRate.y);
            var spawn = i % range == 0;
            if (spawn)
            {
                var prop = GetRandomProp();
                if (prop == null)
                {
                    Debug.LogError("No prop prefab found!");
                    break;
                }
                var position = floorMesh.transform.position + edgeVertices[i].origin;
                position.y += 0.5f;
                var direction = edgeVertices[i].DirectionAsVector3();
                var l = SpawnPrefab(prop, position, direction);
                l.transform.SetParent(environment.transform);
            }
        }

        // Room center lights here
        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = 0; j < rooms[i].centres.Count; j++)
            {
                var range = UnityEngine.Random.Range(centrePropRate.x, centrePropRate.y);
                var spawn = i % range == 0;

                if (spawn)
                {
                    var prop = GetRandomCentreProp();
                    if (prop == null)
                    {
                        Debug.LogError("No prop prefab found!");
                        break;
                    }
                    var position = rooms[i].centres[j];
                    position.y += 0.5f;
                    var gridCellCoords = navAgent.PositionAsGridCoordinates(position);
                    GridCell cell = navAgent.GetGridCellAt((int)gridCellCoords.x, (int)gridCellCoords.y, (int)gridCellCoords.z);

                    if (!CellisInPrisonCell(cell) || !CellIsInRoom(cell, 0))
                    {
                        var l = SpawnPrefab(prop, position, Vector3.zero);
                        l.transform.SetParent(environment.transform);
                    }
                }
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
                for (int i = 0; i < rooms.Count; i++)
                {
                    bool hallwayCheck = CellIsInHallway(cell, rooms[i]);
                    if (hallwayCheck)
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

    void CleanupRooms()
    {
        // Cleanup overlaps
        for (int z = 0; z < grid.cells.z; z++)
        {
            for (int x = 0; x < grid.cells.x; x++)
            {
                GridCell cell = grid.grid[x, 0, z];
                // Clean up walls that overlap
                if (cell.flag.Equals(GridCell.GridFlag.WALL))
                {
                    bool isAgainstVoid = TileIsAdjacent(cell, GridCell.GridFlag.WALKABLE);
                    if (!isAgainstVoid)
                    {
                        cell.flag = GridCell.GridFlag.OCCUPIED;
                    }
                }
            }
        }

        Debug.Log("Had " + rooms.Count + " before combining");
        StartCoroutine(Combine(0));
    }

    void CleanupHallways()
    {
        for (int z = 0; z < grid.cells.z; z++)
        {
            for (int x = 0; x < grid.cells.x; x++)
            {
                GridCell cell = grid.grid[x, 0, z];
                // Clean up walls that overlap
                if (cell.flag.Equals(GridCell.GridFlag.WALL))
                {
                    bool isAgainstVoid = TileIsAdjacent(cell, GridCell.GridFlag.WALKABLE);
                    if (!isAgainstVoid)
                    {
                        cell.flag = GridCell.GridFlag.OCCUPIED;
                    }
                }
            }
        }
    }

    void CarveHallways()
    {

        for (int i = 0; i < rooms.Count - 1; i++)
        {
            var current = rooms[i];
            var next = rooms[i + 1];

            var gridCurrent = navAgent.PositionAsGridCoordinates(current.centres[0]);
            var gridNext = navAgent.PositionAsGridCoordinates(next.centres[0]);

            var pointsX = new int[] { (int)gridCurrent.x, (int)gridNext.x };
            var pointsZ = new int[] { (int)gridCurrent.z, (int)gridNext.z };

            int difX = pointsX[1] - pointsX[0];
            int x = pointsX[0];
            if (difX > 0)
            {
                while (x < pointsX[1])
                {
                    var cell = grid.grid[x, 0, (int)gridCurrent.z];
                    cell.flag = GridCell.GridFlag.OCCUPIED;
                    current.hallways.Add(cell);
                    x++;
                }
            }
            else if (difX < 0)
            {
                while (x > pointsX[1])
                {
                    var cell = grid.grid[x, 0, (int)gridCurrent.z];
                    cell.flag = GridCell.GridFlag.OCCUPIED;
                    current.hallways.Add(cell);
                    x--;
                }
            }

            int difZ = pointsZ[1] - pointsZ[0];
            int z = pointsZ[0];
            if (difZ > 0)
            {
                while (z < pointsZ[1])
                {
                    var cell = grid.grid[x, 0, z];
                    cell.flag = GridCell.GridFlag.OCCUPIED;
                    current.hallways.Add(cell);
                    z++;
                }
            }
            else if (difZ < 0)
            {
                while (z > pointsZ[1])
                {
                    var cell = grid.grid[x, 0, z];
                    cell.flag = GridCell.GridFlag.OCCUPIED;
                    current.hallways.Add(cell);
                    z--;
                }
            }
        }

        // Widen hallways
        for (int i = 0; i < rooms.Count; i++)
        {
            var current = rooms[i];
            for (int j = 0; j < current.hallways.Count; j++)
            {
                var hallway = current.hallways[j];
                SetAdjacentCells(hallway, GridCell.GridFlag.OCCUPIED);
            }
        }

        for (int z = 0; z < grid.cells.z; z++)
        {
            for (int x = 0; x < grid.cells.x; x++)
            {
                GridCell cell = grid.grid[x, 0, z];
                if (cell.flag.Equals(GridCell.GridFlag.OCCUPIED))
                {
                    bool isAgainstVoid = TileIsAdjacent(cell, GridCell.GridFlag.WALKABLE);
                    if (isAgainstVoid)
                    {
                        cell.flag = GridCell.GridFlag.WALL;
                    }
                }
            }
        }

    }

    void CombineRooms(Room root, List<Room> rooms)
    {
        List<Room> toCombine = new List<Room>();
        for (int i = 0; i < rooms.Count; i++)
        {
            var current = rooms[i];
            if (current != root)
            {
                bool overlap = RoomOverlaps(root, current);
                if (overlap)
                {
                    toCombine.Add(current);
                }
            }
        }

        if (toCombine.Count > 0)
        {
            for (int i = 0; i < toCombine.Count; i++)
            {
                var room = toCombine[i];
                rooms.Remove(room);
                Debug.Log("Removed overlapping room!");
                root.occupied.AddRange(room.occupied);
                root.walls.AddRange(room.walls);
                root.hallways.AddRange(room.hallways);
                root.centres.AddRange(room.centres);
            }
            root.indicatorColor = Random.ColorHSV();
            Debug.Log("Combined " + toCombine.Count + " Rooms");
        }
    }

    IEnumerator Combine(int index)
    {
        if (index >= rooms.Count)
        {
            Debug.Log("Finished Combining rooms!");
            Debug.Log("Have " + rooms.Count + " after combining");
            yield break;
        }

        var current = rooms[index];
        CombineRooms(current, rooms);
        StartCoroutine(Combine(index + 1));
        yield return null;
    }

    bool RoomOverlaps(Room a, Room b)
    {
        return a.Intersects(b);
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
    /// Set the cells around the current sell in a 3x3 area
    /// </summary>
    /// <param name="current"></param>
    /// <param name="flag"></param>
    public void SetAdjacentCells(GridCell current, GridCell.GridFlag flag)
    {
        var adjacent = GetAdjacentCells(current);
        for (int i = 0; i < adjacent.Length; i++)
        {
            adjacent[i].flag = flag;
        }
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
        switch (mode)
        {
            case 0:
                for (int i = 0; i < adjacent.Length; i++)
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
        room.start = new Vector3(pos.x, 0f, pos.z);
        room.end = new Vector3(pos.x + roomDimensions.x, 0f, pos.z + roomDimensions.z);
        room.centres.Add(new Vector3(pos.x + (roomDimensions.x / 2), 0, pos.z + (roomDimensions.z / 2)));

        var gridCentreX = Mathf.RoundToInt((pos.x + roomDimensions.x) / 2);
        var gridCentreZ = Mathf.RoundToInt((pos.z + roomDimensions.z) / 2);
        room.gridCentre = new Vector3Int(gridCentreX, 0, gridCentreZ);

        rooms.Add(room);
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
        Vector3 playerCoords = PositionAsGridCoordinates(start.centres[0]);
        for (int z = -3; z < 4; z++)
        {
            for (int x = -3; x < 4; x++)
            {
                GridCell cell = navAgent.GetGridCellAt((int)playerCoords.x + x, (int)playerCoords.y, (int)playerCoords.z + z);
                if (current.position.Equals(cell.position))
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
        Gizmos.DrawSphere(start.centres[0], 0.25f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(end.centres[0], 0.25f);

        for (int i = 0; i < rooms.Count; i++)
        {
            var room = rooms[i];
            room.DrawGizmos();
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