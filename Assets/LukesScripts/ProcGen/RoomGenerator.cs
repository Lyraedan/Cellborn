using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
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
    public int maxEntities = 10;
    public int maxLitter = 100;

    [Header("Environmental rates")]
    [Tooltip("What are the chances of spawning a light if a light is chosen to be spawned? 1 in x")]
    public float wallLightChance = 10f;

    [Tooltip("What are the rates of choosing to spawn a light over a wall prop? 1 in x")]
    public Vector2 lightOrPropChance = new Vector2(0, 2);

    [Tooltip("What are the rate ranges for spawning the wall props? 1 in x")]
    public Vector2 wallPropRate = new Vector2(30, 50);

    [Tooltip("What are the rate ranges for spawning wall decoration as opposed to wall prop groups? 1 in x")]
    public Vector2 wallDecorRate = new Vector2(0, 2);

    public float startSafeZoneThreashold = 40f;
    public float endSafeZoneThreashold = 20f;
    public float centreSafeZoneThreashold = 5f;

    public List<Room> rooms = new List<Room>();
    public GameObject floorPrefab, wallPrefab, ceilingPrefab, environmentRootPrefab, environment;

    [Space(10)]
    public GameObject wizard;
    public GameObject teleporter;
    public GameObject prisonCell;
    private GameObject spawnCell;
    private GameObject levelTeleporter;
    [SerializeField] private GameObject bossRoom;
    public VideoPlayer bossCutsceneEnter;

    private Room start, end;

    [Space(5)]
    public List<RoomPrefab> prefabs = new List<RoomPrefab>();

    private int failedAttempts = 0;

    public GradedPath navAgent;
    public TargetMovement targetAim;
    public bool enableCulling = true;

    private GameObject player;
    public PlayerMovementTest playerController;

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

    public List<NavMeshSurface> navmesh = new List<NavMeshSurface>();

    public RoomMeshGenerator floorMesh, wallMesh, roofMesh;

    private List<GameObject> wallProps = new List<GameObject>();
    private List<RoomMeshGenerator.Edge> floorCorners = new List<RoomMeshGenerator.Edge>();

    [HideInInspector] public bool cutscenePlaying = false;

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

    /// <summary>
    /// Remove all the navmesh surface information and clear the navmesh list
    /// </summary>
    void ClearNavmesh()
    {
        if (navmesh.Count == 0)
            return;

        foreach (NavMeshSurface surface in navmesh)
        {
            surface.RemoveData();
        }
    }

    void Generate(int seed)
    {
        Debug.Log("Loading level: " + levelIndex);
        if (levelIndex == numberOfLevels - 1)
        {
            // Spawn boss room
            var arenaPos = new Vector3(generatedDungeonSize.x / 2, -0.5f, generatedDungeonSize.z / 2);
            var arena = Instantiate(bossRoom, arenaPos, Quaternion.identity).GetComponent<Arena>();

            // Add arena navmesh
            navmesh.Add(arena.navmesh);
            BakeNavmesh();

            SetupArena(arena);
            Debug.Log("Generated arena");
            return;
        }
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

        if (floorMesh != null)
            Destroy(floorMesh.gameObject);

        if (wallMesh != null)
            Destroy(wallMesh.gameObject);

        if (roofMesh != null)
            Destroy(roofMesh.gameObject);

        if (environment != null)
            Destroy(environment);

        floorMesh = Instantiate(floorPrefab, transform).GetComponent<RoomMeshGenerator>();
        wallMesh = Instantiate(wallPrefab, transform).GetComponent<RoomMeshGenerator>();
        roofMesh = Instantiate(ceilingPrefab, transform).GetComponent<RoomMeshGenerator>();
        environment = Instantiate(environmentRootPrefab, transform);

        floorMesh.GenerateFloor(grid);
        wallMesh.GenerateWalls(floorMesh);
        roofMesh.GenerateCeiling(floorMesh);

        navmesh.Add(floorMesh.gameObject.GetComponent<NavMeshSurface>());

        Vector3 startCoords = PositionAsGridCoordinates(start.centres[0]);
        GridCell startPoint = navAgent.GetGridCellAt((int)startCoords.x, (int)startCoords.y, (int)startCoords.z);

        //PlaceProps();
        if (levelIndex == 0)
        {
            player = SpawnPlayer(startPoint);
            playerController = player.GetComponent<PlayerMovementTest>();
            CameraManager.instance.main.gameObject.GetComponent<CameraFollow>().player = player;
            player.name = "Player";
            player.transform.SetParent(null);
        }
        else
        {
            // Spawn teleporter back?
            var position = startPoint.position;
            position.y += 1f;
            playerController.TeleportPlayer(position);
        }

        BakeNavmesh();
        Minimap.instance.GenerateMinimap(grid);

        floorCorners.Clear();
        wallProps.Clear();
        floorCorners = GetCorners();
        Debug.Log($"Got {floorCorners.Count} corners!");
        SpawnEnvironment(floorMesh.edgeVertices);
        SpawnLitter();

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

    void SetupArena(Arena arena)
    {
        // Delete dungeon
        if (floorMesh != null)
            Destroy(floorMesh.gameObject);

        if (wallMesh != null)
            Destroy(wallMesh.gameObject);

        if (roofMesh != null)
            Destroy(roofMesh.gameObject);

        if (environment != null)
            Destroy(environment);

        grid.Bake();
        for (int z = 0; z < grid.cells.z; z++)
        {
            for (int x = 0; x < grid.cells.x; x++)
            {
                var current = grid.grid[x, 0, z];
                if (!current.flag.Equals(GridCell.GridFlag.WALKABLE))
                {
                    bool isWall = TileIsAdjacent(current, GridCell.GridFlag.WALKABLE);
                    if (isWall)
                        current.flag = GridCell.GridFlag.WALL;
                    else
                        current.flag = GridCell.GridFlag.OCCUPIED;
                }
            }
        }
        Minimap.instance.GenerateMinimap(grid);

        // Move player
        playerController.TeleportPlayer(arena.playerSpawn.position);

        PlayerStats.instance.bossVideo.SetActive(true);
        bossCutsceneEnter.SetDirectAudioVolume(0, (AudioManagerRevised.instance.GetMasterVolume() * AudioManagerRevised.instance.GetSfxVolume()) / 1f);
        cutscenePlaying = true;
        bossCutsceneEnter.Play();
        WaitThenExecute(() =>
        {
            cutscenePlaying = false;
            PlayerStats.instance.bossVideo.SetActive(false);

            //Spawn wizard
            var finalWizard = Instantiate(wizard, arena.wizardSpawn.position, Quaternion.identity);
            var finalWizardAI = finalWizard.GetComponent<AIWizard>();
        }, (int) bossCutsceneEnter.clip.length);

    }

    void ClearDungeon()
    {
        DeleteAllObjectsWithTag("Weapon");
        DeleteAllObjectsWithTag("Prop");
        DeleteAllObjectsWithTag("Enemy");
        DeleteAllObjectsWithTag("EnemyProjectile");
        DeleteAllObjectsWithTag("Projectile");
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
        if (WeaponManager.instance.currentWeapon.weaponId == 4)
        {
            if (WeaponManager.instance.currentWeapon.functionality != null)
            {
                // This is fuckin dumb
                WeaponGrapple weaponGrapple = (WeaponGrapple)WeaponManager.instance.currentWeapon.functionality;
                var grapple = weaponGrapple.grapple;

                if (grapple != null)
                {
                    if (grapple.isPulling)
                    {
                        grapple.RetrieveHook();
                    }
                }

            }
        }

        rooms.Clear();

        grid.Clear();
        Minimap.instance.ClearMinimap(grid);
        // Clear navmesh
        ClearNavmesh();
        navmesh.Clear();

        WaitThenExecute(() =>
        {
            Debug.Log("Now generate");
            Generate(levels[levelIndex]);
        }, waitUntil: true, condition: navmesh.Count == 0);
    }

    public void Regenerate()
    {
        // Reloads scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
        SpawnEntities();

        // Matilde if you need to. Start behaviour tree's beyond this point.

        Debug.Log("Got assignables");
    }

    private void SpawnEntities()
    {
        int entityCount = maxEntities * (levelIndex + 1);
        for (int i = 0; i < entityCount; i++)
        {
            GridCell cell = GetRandomEntityCell();
            SpawnRandomEntity(cell);
        }
    }

    public void SpawnLitter()
    {
        for (int i = 0; i < maxLitter; i++)
        {
            GridCell cell = GetRandomLitterCell();
            var litter = SpawnRandomLitter(cell);
            if (litter == null)
                break;
        }
    }

    GridCell GetRandomEntityCell()
    {
        Vector3 position = GetRandomPointOnNavmesh();
        GridCell cell = navAgent.GetGridCellAt((int)position.x, (int)position.y, (int)position.z);

        bool isInStart = CellIsInRoom(cell, 0);
        bool isInHallway = false;
        for (int j = 0; j < rooms.Count; j++)
        {
            bool hallwayCheck = CellIsInHallway(cell, rooms[j]);
            if (hallwayCheck)
            {
                isInHallway = true;
                break;
            }
        }

        bool isValidTile = cell.flag.Equals(GridCell.GridFlag.OCCUPIED);

        if (!isInStart && !isInHallway && isValidTile)
            return cell;
        else // Lol this is bad
            return GetRandomEntityCell();
    }

    GridCell GetRandomLitterCell()
    {
        Vector3 position = GetRandomPointOnNavmesh();
        GridCell cell = navAgent.GetGridCellAt((int)position.x, (int)position.y, (int)position.z);
        bool isValidTile = cell.flag.Equals(GridCell.GridFlag.OCCUPIED) || cell.flag.Equals(GridCell.GridFlag.WALL);
        if (isValidTile)
            return cell;
        else
            return GetRandomLitterCell();
    }

    private Vector3 GetRandomPointOnNavmesh()
    {
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();

        // Pick the first indice of a random triangle in the nav mesh
        int t = Random.Range(0, navMeshData.indices.Length - 3);

        // Select a random point on it
        Vector3 point = Vector3.Lerp(navMeshData.vertices[navMeshData.indices[t]], navMeshData.vertices[navMeshData.indices[t + 1]], Random.value);
        Vector3.Lerp(point, navMeshData.vertices[navMeshData.indices[t + 2]], Random.value);

        return point;
    }

    public GameObject SpawnPrefab(GameObject prefab, Vector3 position, Vector3 rotation)
    {
        position.y += -0.5f;
        GameObject spawned = Instantiate(prefab, position, Quaternion.Euler(rotation));
        spawned.name = $"{prefab.name}_{position.ToString()}_{rotation.ToString()}";
        spawned.transform.SetParent(roomParent);
        return spawned;
    }

    void WaitThenExecute(Action callback, int length = 1, bool waitEndOfFrame = false, bool waitUntil = false, bool condition = false)
    {
        StartCoroutine(WaitThenExecuteEnumerator(callback, length, waitEndOfFrame, waitUntil, condition));
    }

    IEnumerator WaitThenExecuteEnumerator(Action callback, int length, bool waitEndOfFrame, bool waitUntil, bool condition)
    {
        if (!waitEndOfFrame && !waitUntil)
            yield return new WaitForSeconds(length);
        else if (waitEndOfFrame && !waitUntil)
            yield return new WaitForEndOfFrame();
        else
            yield return new WaitUntil(() => condition);

        callback?.Invoke();
    }

    /// <summary>
    /// Grab all the corners of the mesh - wip
    /// </summary>
    /// <returns></returns>
    public List<RoomMeshGenerator.Edge> GetCorners()
    {
        List<RoomMeshGenerator.Edge> corners = new List<RoomMeshGenerator.Edge>();
        var edges = floorMesh.edgeVertices;
        for (int i = 1; i < edges.Count - 1; i++)
        {
            var prev = edges[i - 1];
            var current = edges[i];
            var next = edges[i + 1];

            if (current.x == next.x - 1 && current.z == prev.z + 1)
            {
                corners.Add(current);
            }
        }
        return corners;
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

    public object[] GetRandomProp()
    {
        bool placeDecor = (int)Random.Range(wallDecorRate.x, wallDecorRate.y) == 0;

        if (!placeDecor)
        {
            var prop = prefabs.Where(e => e.type.Equals(RoomPrefab.RoomPropType.WALL_PROP)).ToList();
            if (prop.Count <= 0)
                return null;

            int index = Random.Range(0, prop.Count);
            return new object[] { 0, prop[index].prefab };
        }
        else
        {
            var decor = prefabs.Where(e => e.type.Equals(RoomPrefab.RoomPropType.WALL_DECOR)).ToList();
            if (decor.Count <= 0)
                return null;

            int index = Random.Range(0, decor.Count);
            return new object[] { 1, decor[index].prefab };
        }
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
        //pos.y += 0.5f;
        return entities[index].Spawn(pos, Vector3.zero);
    }

    public GameObject SpawnRandomLitter(GridCell cell)
    {
        var litter = prefabs.Where(e => e.type.Equals(RoomPrefab.RoomPropType.LITTER)).ToList();
        if (litter.Count == 0)
        {
            return null;
        }
        int index = Random.Range(0, litter.Count);
        var pos = cell.position;
        pos.y += 0.05f;
        return litter[index].Spawn(pos, new Vector3(90, 0, 0));
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
    void SpawnEnvironment(List<RoomMeshGenerator.Edge> edgeVertices)
    {
        for (int i = 0; i < edgeVertices.Count; i++)
        {
            var range = UnityEngine.Random.Range(wallPropRate.x, wallPropRate.y);

            bool spawnLightInsteadOfProp = Mathf.RoundToInt(UnityEngine.Random.Range(lightOrPropChance.x, lightOrPropChance.y)) == 0;

            if (spawnLightInsteadOfProp)
            {
                var spawn = i % wallLightChance == 0;
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
                    wallProps.Add(l);
                }
            }
            else
            {
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
                    if ((int)prop[0] == 0)
                    {
                        position.y += 0.5f;
                    }
                    else if ((int)prop[0] == 1)
                    {
                        position.y += (wallMesh.wallHeight / 2) + 0.5f;
                    }
                    if (IsValidPropPosition(position)/* && WallPropPlacementIsValid(position)*/)
                    {
                        var direction = edgeVertices[i].DirectionAsVector3();
                        var p = SpawnPrefab((GameObject)prop[1], position, direction);
                        p.transform.SetParent(environment.transform);
                        wallProps.Add(p);
                    }
                }
            }
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].centres.Count > 1)
            {
                for (int j = 0; j < rooms[i].centres.Count; j++)
                {
                    SpawnProps(i, j);
                }
            }
            else
            {
                SpawnProps(i, 0);
            }
        }
    }

    void SpawnProps(int roomIndex, int centreIndex)
    {
        // Spawn a light
        var light = GetRandomCeilingLight();
        if (light == null)
        {
            Debug.LogError("No light prefab found!");
            return;
        }

        var lightPosition = rooms[roomIndex].centres[centreIndex];
        lightPosition.y = wallMesh.wallHeight - 0.5f;
        var lt = SpawnPrefab(light, lightPosition, Vector3.zero);
        lt.transform.SetParent(environment.transform);

        var prop = GetRandomCentreProp();
        if (prop == null)
        {
            Debug.LogError("No prop prefab found!");
            return;
        }
        var position = rooms[roomIndex].centres[centreIndex];
        //position.y += 0.5f;
        var gridCellCoords = navAgent.PositionAsGridCoordinates(position);
        GridCell cell = navAgent.GetGridCellAt((int)gridCellCoords.x, (int)gridCellCoords.y, (int)gridCellCoords.z);

        float distanceFromStart = Vector3.Distance(rooms[0].centres[0], position);
        float distanceFromEnd = Vector3.Distance(rooms[rooms.Count - 1].centres[0], position);

        // Todo, check distance between center points to see if is beyond threashold

        bool isBeyondThreashold = true;

        if (rooms[roomIndex].centres.Count > 1)
        {
            int nextIndex = centreIndex + 1;
            if (nextIndex <= rooms[roomIndex].centres.Count - 1)
            {
                var position2 = rooms[roomIndex].centres[nextIndex];
                var distanceBetweenCentres = Vector3.Distance(position, position2);
                isBeyondThreashold = distanceBetweenCentres > centreSafeZoneThreashold;
            }
        }

        if (distanceFromStart > startSafeZoneThreashold && distanceFromEnd > endSafeZoneThreashold && isBeyondThreashold)
        {
            Vector3 rot = new Vector3(0, Random.Range(0, 360), 0);
            var l = SpawnPrefab(prop, position, rot);
            l.transform.SetParent(environment.transform);
        }

    }

    public bool IsValidPropPosition(Vector3 point)
    {
        if (wallProps.Count == 0)
            return true;

        float radius = 3f;
        bool valid = true;
        for (int i = 0; i < wallProps.Count; i++)
        {
            var position = wallProps[i].transform.position;
            float distance = Vector3.Distance(point, position);
            if (distance < radius)
            {
                valid = false;
                break;
            }
        }
        return valid;
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
            case 3:
                for (int i = 0; i < 2; i++)
                {
                    if (adjacent[i] != null)
                    {
                        if (adjacent[i].flag.Equals(flag))
                            return true;
                    }
                }
                break;
            case 4:
                for (int i = 2; i < 4; i++)
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

    public bool WallPropPlacementIsValid(Vector3 point)
    {
        GridCell current = navAgent.GetGridCellAt((int)point.x, 0, (int)point.z);
        var adjacentHorizontalVoid = TileIsAdjacent(current, GridCell.GridFlag.WALKABLE, 3);
        var adjacentHorizontalOccupied = TileIsAdjacent(current, GridCell.GridFlag.OCCUPIED, 3);

        var adjacentVerticalVoid = TileIsAdjacent(current, GridCell.GridFlag.WALKABLE, 4);
        var adjacentVerticalOccupied = TileIsAdjacent(current, GridCell.GridFlag.OCCUPIED, 4);

        if (adjacentHorizontalVoid)
            return false;
        else if (adjacentHorizontalOccupied)
            return false;
        else if (adjacentVerticalVoid)
            return false;
        else if (adjacentVerticalOccupied)
            return false;

        return true;
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

    private bool CellIsInPrisonCell(GridCell current)
    {
        Vector3 playerCoords = PositionAsGridCoordinates(start.centres[0]);
        for (int z = -3; z < 4; z++)
        {
            for (int x = -3; x < 4; x++)
            {
                GridCell cell = navAgent.GetGridCellAt((int)playerCoords.x + x, (int)playerCoords.y, (int)playerCoords.z + z);
                if (cell != null)
                {
                    if (current.position.Equals(cell.position))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool CellIsOnTeleporter(GridCell current)
    {
        Vector3 cellCoords = PositionAsGridCoordinates(current.position);
        for (int z = -5; z < 6; z++)
        {
            for (int x = -5; x < 6; x++)
            {
                GridCell cell = navAgent.GetGridCellAt((int)cellCoords.x + x, (int)cellCoords.y, (int)cellCoords.z + z);
                if (cell != null)
                {
                    if (current.position.Equals(cell.position))
                    {
                        return true;
                    }
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
        foreach (RoomMeshGenerator.Edge corner in floorCorners)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(corner.origin, corner.origin + (Vector3.up / 2));
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(corner.origin + (Vector3.one / 2), 0.1f);
        }
    }
#endif
}

public class CellInfo : MonoBehaviour
{
    public Vector3 cellRotation = Vector3.zero;
}