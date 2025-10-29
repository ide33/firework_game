using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfViewVisualizer : MonoBehaviour
{
    [Range(0, 360)] public float viewAngle = 90f;
    public float viewDistance = 10f;
    public int rayCount = 60;
    public LayerMask obstacleMask;

    private Mesh mesh;
    private MeshRenderer meshRenderer;
    private Material runtimeMaterial;

    void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        meshRenderer = GetComponent<MeshRenderer>();
        runtimeMaterial = new Material(Shader.Find("Unlit/Color")); // 内部で仮作成
        runtimeMaterial.color = new Color(1, 0, 0, 0.25f); // 半透明の赤（仮）
        meshRenderer.material = runtimeMaterial;
    }

    void LateUpdate()
    {
        GenerateViewMesh();
    }

    void GenerateViewMesh()
    {
        float angleStep = viewAngle / rayCount;
        float startAngle = -viewAngle / 2;
        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = Vector3.zero; // 中心

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = startAngle + i * angleStep;
            Vector3 dir = DirFromAngle(angle);
            Vector3 vertex = dir * viewDistance;

            if (Physics.Raycast(transform.position + Vector3.up * 0.2f, transform.TransformDirection(dir), out RaycastHit hit, viewDistance, obstacleMask))
                vertex = dir * hit.distance;

            vertices[i + 1] = vertex;

            if (i < rayCount)
            {
                int vertIndex = i + 1;
                int triIndex = i * 3;
                triangles[triIndex] = 0;
                triangles[triIndex + 1] = vertIndex + 1;
                triangles[triIndex + 2] = vertIndex;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    Vector3 DirFromAngle(float angle)
    {
        float rad = Mathf.Deg2Rad * angle;
        return new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
    }

    public void SetColor(Color color)
    {
        if (runtimeMaterial != null)
            runtimeMaterial.color = color;
    }
}
