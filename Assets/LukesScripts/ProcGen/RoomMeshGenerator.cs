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
        public GridCell cell;

        public bool EdgeMatches(Edge edge)
        {
            return this.origin.Equals(edge.origin);
        }

        public Vector3 DirectionAsVector3()
        {
            switch(direction)
            {
                case FaceDirection.NORTH:
                    return new Vector3(0, 0, 0);
                case FaceDirection.SOUTH:
                    return new Vector3(0, 180, 0);
                case FaceDirection.EAST:
                    return new Vector3(0, 90, 0);
                case FaceDirection.WEST:
                    return new Vector3(0, -90, 0);
                default:
                    return new Vector3(0, 0, 0);
            }
        }
    }

    private Grid grid;
    public float tileSize = 1f, wallHeight = 1f;

    public Material material;
    public PhysicMaterial physicsMaterial;

    public List<Vector3> vertices = new List<Vector3>();
    public List<int> tris = new List<int>();
    public List<Vector3> normals = new List<Vector3>();
    public List<Vector2> uvs = new List<Vector2>();
    public List<Edge> edgeVertices = new List<Edge>();

    [HideInInspector] public Mesh mesh;
    [HideInInspector] public MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    public bool ReadWriteEnabled
    {
        get
        {
            return mesh.isReadable;
        }
    }

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
                        current.flag.Equals(GridCell.GridFlag.CORNER) ||
                        current.flag.Equals(GridCell.GridFlag.OCCUPIED))
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
                                    direction = FaceDirection.SOUTH,
                                    cell = current
                                });
                                edgeVertices.Add(new Edge()
                                {
                                    origin = topLeft,
                                    direction = FaceDirection.SOUTH,
                                    cell = current
                                });
                            }
                        }
                        else
                        {
                            edgeVertices.Add(new Edge()
                            {
                                origin = topRight,
                                direction = FaceDirection.SOUTH,
                                cell = current
                            });
                            edgeVertices.Add(new Edge()
                            {
                                origin = topLeft,
                                direction = FaceDirection.SOUTH,
                                cell = current
                            });
                        }

                        if (adjacent[RoomGenerator.LEFT] != null)
                        {
                            if (adjacent[RoomGenerator.LEFT].flag.Equals(GridCell.GridFlag.WALKABLE))
                            {
                                edgeVertices.Add(new Edge()
                                {
                                    origin = topLeft,
                                    direction = FaceDirection.EAST,
                                    cell = current
                                });
                                edgeVertices.Add(new Edge()
                                {
                                    origin = bottomLeft,
                                    direction = FaceDirection.EAST,
                                    cell = current
                                });
                            }
                        }
                        else
                        {
                            edgeVertices.Add(new Edge()
                            {
                                origin = topLeft,
                                direction = FaceDirection.EAST,
                                cell = current
                            });
                            edgeVertices.Add(new Edge()
                            {
                                origin = bottomLeft,
                                direction = FaceDirection.EAST,
                                cell = current
                            });
                        }

                        if (adjacent[RoomGenerator.RIGHT] != null)
                        {
                            if (adjacent[RoomGenerator.RIGHT].flag.Equals(GridCell.GridFlag.WALKABLE))
                            {
                                edgeVertices.Add(new Edge()
                                {
                                    origin = topRight,
                                    direction = FaceDirection.WEST,
                                    cell = current
                                });
                                edgeVertices.Add(new Edge()
                                {
                                    origin = bottomRight,
                                    direction = FaceDirection.WEST,
                                    cell = current
                                });
                            }
                        }
                        else
                        {
                            edgeVertices.Add(new Edge()
                            {
                                origin = topRight,
                                direction = FaceDirection.WEST,
                                cell = current
                            });
                            edgeVertices.Add(new Edge()
                            {
                                origin = bottomRight,
                                direction = FaceDirection.WEST,
                                cell = current
                            });
                        }

                        if (adjacent[RoomGenerator.DOWN] != null)
                        {
                            if (adjacent[RoomGenerator.DOWN].flag.Equals(GridCell.GridFlag.WALKABLE))
                            {
                                edgeVertices.Add(new Edge()
                                {
                                    origin = bottomRight,
                                    direction = FaceDirection.NORTH,
                                    cell = current
                                });
                                edgeVertices.Add(new Edge()
                                {
                                    origin = bottomLeft,
                                    direction = FaceDirection.NORTH,
                                    cell = current
                                });
                            }
                        }
                        else
                        {
                            edgeVertices.Add(new Edge()
                            {
                                origin = bottomRight,
                                direction = FaceDirection.NORTH,
                                cell = current
                            });
                            edgeVertices.Add(new Edge()
                            {
                                origin = bottomLeft,
                                direction = FaceDirection.NORTH,
                                cell = current
                            });
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

        // Extrude
        float floorThickness = 0.1f;
        Matrix4x4[] matrix = new Matrix4x4[] {
                new Matrix4x4(new Vector4(1, 0, 0, 0),
                              new Vector4(0, 1, 0, 0),
                              new Vector4(0, 0, 1, 0),
                              new Vector4(0, 0, 0, 1)),

                new Matrix4x4(new Vector4(1, 0, 0, 0),
                              new Vector4(0, 1, 0, 0),
                              new Vector4(0, 0, 1, 0),
                              new Vector4(0, -floorThickness, 0, 1))
        };

        Mesh extruded = new Mesh();
        MeshExtrusion.ExtrudeMesh(mesh, extruded, matrix, false);

        meshFilter.mesh = extruded;
        meshCollider.sharedMesh = extruded;
        meshCollider.material = physicsMaterial;
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

        edgeVertices = floorMesh.edgeVertices;

        int index = 0;
        for (int z = 0; z < grid.cells.z; z++)
        {
            for (int x = 0; x < grid.cells.x; x++)
            {
                GridCell current = grid.grid[x, 0, z];
                if (current.flag.Equals(GridCell.GridFlag.WALL) ||
                    current.flag.Equals(GridCell.GridFlag.CORNER) ||
                    current.flag.Equals(GridCell.GridFlag.OCCUPIED))
                {
                    index = DrawWall(current, index);
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

        /*
        float floorThickness = 1f;
        Matrix4x4[] matrix = new Matrix4x4[] {
            new Matrix4x4(new Vector4(1, 0, 0, 0),
                          new Vector4(0, 1, 0, 0),
                          new Vector4(0, 0, 1, 0),
                          new Vector4(0, 0, 0, 0)),

            new Matrix4x4(new Vector4(1, 0, 0, 0),
                          new Vector4(0, 1, 0, 0),
                          new Vector4(0, 0, 1, 0),
                          new Vector4(0, floorThickness, 0, 0))
        };

        Mesh extruded = new Mesh();
        MeshExtrusion.ExtrudeMesh(mesh, extruded, matrix, false);
        */
        meshFilter.mesh = mesh;

        foreach (MeshFilter meshFilter in GameObject.FindObjectsOfType<MeshFilter>())
        {
            meshFilter.mesh.SetIndices(meshFilter.mesh.GetIndices(0).Concat(meshFilter.mesh.GetIndices(0).Reverse()).ToArray(), MeshTopology.Triangles, 0);
        }
        meshCollider.sharedMesh = meshFilter.mesh;
        meshCollider.material = physicsMaterial;

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

        foreach (MeshFilter meshFilter in GameObject.FindObjectsOfType<MeshFilter>())
        {
            meshFilter.mesh.SetIndices(meshFilter.mesh.GetIndices(0).Concat(meshFilter.mesh.GetIndices(0).Reverse()).ToArray(), MeshTopology.Triangles, 0);
        }
        meshCollider.sharedMesh = meshFilter.mesh;
        meshCollider.material = physicsMaterial;

        meshRenderer.enabled = SuperSecret.secretEnabled; // Hide roof
    }

    public int DrawWall(GridCell current, int index)
    {
        var position = current.position;
        int i = index;
        var adjacent = RoomGenerator.instance.GetAdjacentCells(current);

        Edge forward = new Edge()
        {
            origin = position,
            direction = FaceDirection.SOUTH,
            cell = current
        };
        Edge back = new Edge()
        {
            origin = position,
            direction = FaceDirection.NORTH,
            cell = current
        };
        Edge left = new Edge()
        {
            origin = position,
            direction = FaceDirection.WEST,
            cell = current
        };
        Edge right = new Edge()
        {
            origin = position,
            direction = FaceDirection.EAST,
            cell = current
        };

        if(adjacent[RoomGenerator.UP] != null)
        {
            if(adjacent[RoomGenerator.UP].flag.Equals(GridCell.GridFlag.WALKABLE))
            {
                AddEdge(forward, i);
                i += 4;
            }
        }
        else
        {
            AddEdge(forward, i);
            i += 4;
        }

        if (adjacent[RoomGenerator.DOWN] != null)
        {
            if (adjacent[RoomGenerator.DOWN].flag.Equals(GridCell.GridFlag.WALKABLE))
            {
                AddEdge(back, i);
                i += 4;
            }
        }
        else
        {
            AddEdge(back, i);
            i += 4;
        }

        if (adjacent[RoomGenerator.LEFT] != null)
        {
            if (adjacent[RoomGenerator.LEFT].flag.Equals(GridCell.GridFlag.WALKABLE))
            {
                AddEdge(left, i);
                i += 4;
            }
        }
        else
        {
            AddEdge(left, i);
            i += 4;
        }

        if (adjacent[RoomGenerator.RIGHT] != null)
        {
            if (adjacent[RoomGenerator.RIGHT].flag.Equals(GridCell.GridFlag.WALKABLE))
            {
                AddEdge(right, i);
                i += 4;
            }
        }
        else
        {
            AddEdge(right, i);
            i += 4;
        }

        return i;
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
                bottomLeft = new Vector3(x, 0, z + tileSize);
                bottomRight = new Vector3(x + tileSize, 0, z + tileSize);
                topLeft = new Vector3(x, wallHeight, z + tileSize);
                topRight = new Vector3(x + tileSize, wallHeight, z + tileSize);
                break;
            case FaceDirection.WEST:
                bottomLeft = new Vector3(x, 0, z);
                bottomRight = new Vector3(x, 0, z + tileSize);
                topLeft = new Vector3(x, wallHeight, z);
                topRight = new Vector3(x, wallHeight, z + tileSize);
                break;
            case FaceDirection.EAST:
                bottomLeft = new Vector3(x + tileSize, 0, z);
                bottomRight = new Vector3(x + tileSize, 0, z + tileSize);
                topLeft = new Vector3(x + tileSize, wallHeight, z);
                topRight = new Vector3(x + tileSize, wallHeight, z + tileSize);
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

    public Edge GetClosestEdgeAt(Vector3 point)
    {
        Dictionary<int, float> distances = new Dictionary<int, float>();
        for(int i = 0; i < edgeVertices.Count; i++)
        {
            distances.Add(i, Vector3.Distance(edgeVertices[i].origin, point));
        }
        var sorted = distances.ToList();

        sorted.Sort((a, b) => a.Value.CompareTo(b.Value));
        return edgeVertices[sorted[0].Key];
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