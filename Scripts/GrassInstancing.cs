using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GrassInstancing : MonoBehaviour
{
    public int grassCount = 1000;
    public float grassHeight = 1f;
    public float grassWidth = 0.1f;
    public Material grassMaterial;

    private Mesh grassMesh;
    private Matrix4x4[] matrices;

    void Start()
    {
        CreateGrassMesh();
        CreateGrassInstances();
    }

    void CreateGrassMesh()
    {
        grassMesh = new Mesh();

        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-grassWidth * 0.5f, 0, 0),
            new Vector3(grassWidth * 0.5f, 0, 0),
            new Vector3(-grassWidth * 0.5f, grassHeight, 0),
            new Vector3(grassWidth * 0.5f, grassHeight, 0)
        };

        int[] triangles = new int[6]
        {
            0, 2, 1,
            2, 3, 1
        };

        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };

        grassMesh.vertices = vertices;
        grassMesh.triangles = triangles;
        grassMesh.uv = uv;
        grassMesh.RecalculateNormals();
    }

    void CreateGrassInstances()
    {
        matrices = new Matrix4x4[grassCount];
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh surfaceMesh = meshFilter.sharedMesh;

        for (int i = 0; i < grassCount; i++)
        {
            Vector3 randomPoint = GetRandomPointOnMesh(surfaceMesh);
            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            matrices[i] = Matrix4x4.TRS(randomPoint, randomRotation, Vector3.one);
        }
    }

    Vector3 GetRandomPointOnMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        int randomTriangleIndex = Random.Range(0, triangles.Length / 3) * 3;
        Vector3 v1 = vertices[triangles[randomTriangleIndex]];
        Vector3 v2 = vertices[triangles[randomTriangleIndex + 1]];
        Vector3 v3 = vertices[triangles[randomTriangleIndex + 2]];

        float r1 = Random.value;
        float r2 = Random.value;
        if (r1 + r2 > 1)
        {
            r1 = 1 - r1;
            r2 = 1 - r2;
        }

        Vector3 randomPoint = v1 + r1 * (v2 - v1) + r2 * (v3 - v1);
        return transform.TransformPoint(randomPoint);
    }

    void Update()
    {
        Graphics.DrawMeshInstanced(grassMesh, 0, grassMaterial, matrices);
    }
}