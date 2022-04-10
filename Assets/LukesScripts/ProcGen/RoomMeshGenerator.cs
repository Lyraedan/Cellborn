using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomMeshGenerator : MonoBehaviour
{
    public enum FaceDirection
    {
        UNASSIGNED, NORTH, SOUTH, EAST, WEST
    }

    [System.Serializable]
    public struct Edge
    {
        public float x
        {
            get
            {
                return origin.x;
            }
        }
        public float y
        {
            get
            {
                return origin.y;
            }
        }
        public float z
        {
            get
            {
                return origin.z;
            }
        }

        public Vector3 origin;
        public FaceDirection direction;

        public bool EdgeMatches(Edge edge)
        {
            return this.origin.Equals(edge.origin);
        }
    }

    private Grid grid;
    public float tileSize = 1f, wallHeight = 1f;

    public Material material;

    public List<Vector3> vertices = new List<Vector3>();
    public List<int> tris = new List<int>();
    public List<Vector3> normals = new List<Vector3>();
    public List<Vector2> uvs = new List<Vector2>();
    public List<Edge> edgeVertices = new List<Edge>();

    [HideInInspector] public Mesh mesh;
    [HideInInspector] public MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    public bool showVertices = false;
    public bool showEdges = false;
    public bool showEdgeDirection = false;

    /// <summary>
    /// Only called by floor mesh object
    /// </summary>
    /// <param name="grid"></param>
    public void GenerateFloor(Grid grid)
    {
        this.grid = grid;
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshCollider = gameObject.AddComponent<MeshCollider>();

        mesh = new Mesh();
        meshRenderer.sharedMaterial = material;

        int index = 0;
        for (int z = 0; z < grid.cells.z; z++)
        {
            for (int x = 0; x < grid.cells.x; x++)
            {
                GridCell current = grid.grid[x, 0, z];
                if (current.flag.Equals(GridCell.GridFlag.OCCUPIED) ||
                    current.flag.Equals(GridCell.GridFlag.WALL) ||
                    current.flag.Equals(GridCell.GridFlag.CORNER))
                {
                    AddQuadAt(x, z, index);
                    index += 4;

                    if (current.flag.Equals(GridCell.GridFlag.WALL) ||
                        current.flag.Equals(GridCell.GridFlag.CORNER))
                    {
                        int count = vertices.Count;
                        var topRight = vertices[count - 1];
                        var topLeft = vertices[count - 2];
                        var bottomRight = vertices[count - 3];
                        var bottomLeft = vertices[count - 4];

                        var adjacent = RoomGenerator.instance.GetAdjacentCells(current);

                        if (adjacent[RoomGenerator.UP] != null)
                        {
                            if (adjacent[RoomGenerator.UP].flag.Equals(GridCell.GridFlag.WALKABLE))
                            {
                                edgeVertices.Add(new Edge()
                                {
                                    origin = topRight,
                                    direction = FaceDirection.SOUTH
                                });
                                edgeVertices.Add(new Edge()
                                {
                                    origin = topLeft,
                                    direction = FaceDirection.SOUTH
                                });
                            }
                        }
                        if (adjacent[RoomGenerator.LEFT] != null)
                        {
                            if (adjacent[RoomGenerator.LEFT].flag.Equals(GridCell.GridFlag.WALKABLE))
                            {
                                edgeVertices.Add(new Edge()
                                {
                                    origin = topLeft,
                                    direction = FaceDirection.EAST
                                });
                                edgeVertices.Add(new Edge()
                                {
                                    origin = bottomLeft,
                                    direction = FaceDirection.EAST
                                });
                            }
                        }
                        if (adjacent[RoomGenerator.RIGHT] != null)
                        {
                            if (adjacent[RoomGenerator.RIGHT].flag.Equals(GridCell.GridFlag.WALKABLE))
                            {
                                edgeVertices.Add(new Edge()
                                {
                                    origin = topRight,
                                    direction = FaceDirection.WEST
                                });
                                edgeVertices.Add(new Edge()
                                {
                                    origin = bottomRight,
                                    direction = FaceDirection.WEST
                                });
                            }
                        }
                        if (adjacent[RoomGenerator.DOWN] != null)
                        {
                            if (adjacent[RoomGenerator.DOWN].flag.Equals(GridCell.GridFlag.WALKABLE))
                            {
                                edgeVertices.Add(new Edge()
                                {
                                    origin = bottomRight,
                                    direction = FaceDirection.NORTH
                                });
                                edgeVertices.Add(new Edge()
                                {
                                    origin = bottomLeft,
                                    direction = FaceDirection.NORTH
                                });
                            }
                        }
                    }
                }
            }
        }
        edgeVertices = edgeVertices.Distinct().ToList();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    /// <summary>
    /// Only called by wall mesh object
    /// </summary>
    /// <param name="floorMesh"></param>
    public void GenerateWalls(RoomMeshGenerator floorMesh)
    {
        this.grid = floorMesh.grid;
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshCollider = gameObject.AddComponent<MeshCollider>();

        mesh = new Mesh();
        meshRenderer.sharedMaterial = material;

        var edges = floorMesh.edgeVertices;
        List<Edge> walls = new List<Edge>();
        for (int i = 0; i < edges.Count; i++)
        {
            Edge edge = edges[i];
            Vector3 raised = edge.origin;
            raised.y = wallHeight;

            Edge raisedWall = new Edge()
            {
                origin = raised,
                direction = edge.direction
            };
            walls.Add(edge);
            walls.Add(raisedWall);
        }
        edgeVertices = walls;
        edgeVertices = edgeVertices.Distinct().ToList();

        BuildEdges();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    public void GenerateCeiling(RoomMeshGenerator floorMesh)
    {
        this.grid = floorMesh.grid;
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshCollider = gameObject.AddComponent<MeshCollider>();

        mesh = new Mesh();
        meshRenderer.sharedMaterial = material;

        int index = 0;
        for (int z = 0; z < grid.cells.z; z++)
        {
            for (int x = 0; x < grid.cells.x; x++)
            {
                GridCell current = grid.grid[x, 0, z];
                if (current.flag.Equals(GridCell.GridFlag.OCCUPIED) ||
                    current.flag.Equals(GridCell.GridFlag.WALL) ||
                    current.flag.Equals(GridCell.GridFlag.CORNER))
                {
                    AddCeilingAt(x, z, index);
                    index += 4;
                }
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        meshRenderer.enabled = false; // Hide roof
    }

    public void BuildEdges()
    {
        for(int i = 0; i < edgeVertices.Count; i++)
        {
            Edge edge = edgeVertices[i];
            vertices.Add(edge.origin);
        }

        int index = 0;
        for (int i = 0; i < vertices.Count; i++)
        {
            int mod = i % 4;
            if(mod == 0)
            {
                int count = vertices.Count;
                var topRight = vertices[count - 1];
                var topLeft = vertices[count - 2];
                var bottomRight = vertices[count - 3];
                var bottomLeft = vertices[count - 4];

                tris.Add(index);
                tris.Add(index + 2);
                tris.Add(index + 1);

                tris.Add(index + 1);
                tris.Add(index + 2);
                tris.Add(index + 3);

                Vector3 normalA = CalculateNormal(topLeft, bottomLeft, bottomRight);
                Vector3 normalB = CalculateNormal(topLeft, bottomRight, topRight);

                normals.Add(normalA);
                normals.Add(normalA);
                normals.Add(normalA);
                normals.Add(normalB);

                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(0, 1));
                uvs.Add(new Vector2(1, 1));

                index += 4;
            }
        }
    }

    public Vector3 FindNearestPoint(Vector3 origin, Vector3 direction)
    {
        direction.Normalize();
        Vector3 point = edgeVertices[edgeVertices.Count - 1].origin; // replaced parameter
        Vector3 lhs = point - origin;

        float dot = Vector3.Dot(lhs, direction);
        return origin + direction * dot;
    }

    Vector3 DirectionAsVector(FaceDirection direction)
    {
        switch (direction)
        {
            case FaceDirection.NORTH:
                return new Vector3(0, 0, 1);
            case FaceDirection.SOUTH:
                return new Vector3(0, 0, -1);
            case FaceDirection.EAST:
                return new Vector3(1, 0, 0);
            case FaceDirection.WEST:
                return new Vector3(-1, 0, 0);
            default:
                return Vector3.zero;
        }
    }

    public void AddEdge(Edge edge, int index)
    {
        float x = edge.origin.x;
        float z = edge.origin.z;

        Vector3 topLeft = Vector3.zero;
        Vector3 topRight = Vector3.zero;
        Vector3 bottomLeft = Vector3.zero;
        Vector3 bottomRight = Vector3.zero;

        switch (edge.direction)
        {
            case FaceDirection.NORTH:
                bottomLeft = new Vector3(x, 0, z);
                bottomRight = new Vector3(x + tileSize, 0, z);
                topLeft = new Vector3(x, wallHeight, z);
                topRight = new Vector3(x + tileSize, wallHeight, z);
                break;
            case FaceDirection.SOUTH:
                bottomLeft = new Vector3(x, 0, z);
                bottomRight = new Vector3(x - tileSize, 0, z);
                topLeft = new Vector3(x, wallHeight, z);
                topRight = new Vector3(x - tileSize, wallHeight, z);
                break;
            case FaceDirection.WEST:
                bottomLeft = new Vector3(x, 0, z);
                bottomRight = new Vector3(x, 0, z + tileSize);
                topLeft = new Vector3(x, wallHeight, z);
                topRight = new Vector3(x, wallHeight, z + tileSize);
                break;
            case FaceDirection.EAST:
                bottomLeft = new Vector3(x, 0, z);
                bottomRight = new Vector3(x, 0, z - tileSize);
                topLeft = new Vector3(x, wallHeight, z);
                topRight = new Vector3(x, wallHeight, z - tileSize);
                break;
        }

        vertices.Add(topLeft);
        vertices.Add(topRight);
        vertices.Add(bottomLeft);
        vertices.Add(bottomRight);

        // Generate triangles
        tris.Add(index);
        tris.Add(index + 2);
        tris.Add(index + 1);

        tris.Add(index + 1);
        tris.Add(index + 2);
        tris.Add(index + 3);

        // Generate normals
        Vector3 normalA = CalculateNormal(topLeft, bottomLeft, bottomRight);
        Vector3 normalB = CalculateNormal(topLeft, bottomRight, topRight);

        normals.Add(normalA);
        normals.Add(normalA);
        normals.Add(normalA);
        normals.Add(normalB);

        // Generate UVs
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
    }

    public void AddQuadAt(int x, int z, int index)
    {
        Vector3 topLeft = new Vector3(x, 0, z);
        Vector3 topRight = new Vector3(x + tileSize, 0, z);
        Vector3 bottomLeft = new Vector3(x, 0, z + tileSize);
        Vector3 bottomRight = new Vector3(x + tileSize, 0, z + tileSize);

        vertices.Add(topLeft);
        vertices.Add(topRight);
        vertices.Add(bottomLeft);
        vertices.Add(bottomRight);

        // Generate triangles
        tris.Add(index);
        tris.Add(index + 2);
        tris.Add(index + 1);

        tris.Add(index + 1);
        tris.Add(index + 2);
        tris.Add(index + 3);

        // Generate normals
        Vector3 normalA = CalculateNormal(topLeft, bottomLeft, bottomRight);
        Vector3 normalB = CalculateNormal(topLeft, bottomRight, topRight);

        normals.Add(normalA);
        normals.Add(normalA);
        normals.Add(normalA);
        normals.Add(normalB);

        // Generate UVs
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
    }

    public void AddCeilingAt(int x, int z, int index)
    {
        Vector3 topLeft = new Vector3(x, wallHeight, z);
        Vector3 topRight = new Vector3(x + tileSize, wallHeight, z);
        Vector3 bottomLeft = new Vector3(x, wallHeight, z + tileSize);
        Vector3 bottomRight = new Vector3(x + tileSize, wallHeight, z + tileSize);

        vertices.Add(topLeft);
        vertices.Add(topRight);
        vertices.Add(bottomLeft);
        vertices.Add(bottomRight);

        // Generate triangles
        tris.Add(index);
        tris.Add(index + 2);
        tris.Add(index + 1);

        tris.Add(index + 1);
        tris.Add(index + 2);
        tris.Add(index + 3);

        // Generate normals
        Vector3 normalA = CalculateNormal(topLeft, bottomLeft, bottomRight);
        Vector3 normalB = CalculateNormal(topLeft, bottomRight, topRight);

        normals.Add(normalA);
        normals.Add(normalA);
        normals.Add(normalA);
        normals.Add(normalB);

        // Generate UVs
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
    }

    public Vector3 CalculateNormal(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 a = new Vector3(p2.x - p1.x, p2.y - p1.y, p2.z - p1.z);
        Vector3 b = new Vector3(p3.x - p1.x, p3.y - p1.y, p3.z - p1.z);

        Vector3 normal = new Vector3(a.y * b.z - a.z * b.y,
                                     a.z * b.x - a.x * b.z,
                                     a.x * b.y - a.y * b.x);
        return normal.normalized;
    }

    public void AddEdgeToList(Edge edge)
    {
        bool canAdd = true;
        if (edgeVertices.Count > 0)
        {
            for (int i = 0; i < edgeVertices.Count; i++)
            {
                Edge element = edgeVertices[i];
                if (element.origin.Equals(edge.origin))
                {
                    canAdd = false;
                    break;
                }
            }
        }
        if (canAdd)
        {
            edgeVertices.Add(edge);
        }
    }

    private void OnDrawGizmos()
    {
        if (showVertices)
        {
            if (vertices.Count > 0)
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(transform.position + vertices[i], 0.1f);
                }

                for (int i = 0; i < vertices.Count - 1; i++)
                {
                    Vector3 pos = transform.position + vertices[i];
                    Vector3 pos2 = transform.position + vertices[i + 1];
                    int mod = i % 6;
                    switch (mod)
                    {
                        case 0:
                            Gizmos.color = Color.red;
                            break;
                        case 1:
                            Gizmos.color = Color.green;
                            break;
                        case 2:
                            Gizmos.color = Color.blue;
                            break;
                        case 3:
                            Gizmos.color = Color.yellow;
                            break;
                        case 4:
                            Gizmos.color = Color.magenta;
                            break;
                        case 5:
                            Gizmos.color = Color.cyan;
                            break;
                    }
                    Gizmos.DrawLine(pos, pos2);
                }
            }
        }
        if (showEdges)
        {
            if (edgeVertices.Count > 0)
            {
                for (int i = 0; i < edgeVertices.Count; i++)
                {
                    Edge edge = edgeVertices[i];

                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(transform.position + edge.origin, 0.1f);
                    //UnityEditor.Handles.Label(transform.position + edge.origin, $"{edge.origin}");

                    if (showEdgeDirection)
                    {
                        var lineStart = transform.position + edge.origin;
                        if (edge.direction.Equals(FaceDirection.NORTH))
                        {
                            Gizmos.color = Color.red;
                            Gizmos.DrawLine(lineStart, lineStart + transform.forward);
                        }
                        if (edge.direction.Equals(FaceDirection.SOUTH))
                        {
                            Gizmos.color = Color.green;
                            Gizmos.DrawLine(lineStart, lineStart + -transform.forward);
                        }
                        if (edge.direction.Equals(FaceDirection.EAST))
                        {
                            Gizmos.color = Color.blue;
                            Gizmos.DrawLine(lineStart, lineStart + transform.right);

                        }
                        if (edge.direction.Equals(FaceDirection.WEST))
                        {
                            Gizmos.color = Color.yellow;
                            Gizmos.DrawLine(lineStart, lineStart + -transform.right);
                        }
                    }
                }
            }
        }
    }
}