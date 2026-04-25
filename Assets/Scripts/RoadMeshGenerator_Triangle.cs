using UnityEngine;
using System.Collections.Generic;

public class RoadMeshGenerator_Triangle : MonoBehaviour
{
    private GridGenerator_Triangle gridGen;

    [Header("Yol Ayarları")]
    public float roadWidth = 0.8f;
    public float sidewalkWidth = 0.3f;
    public Material roadMaterial;
    public Material sidewalkMaterial;

    private GameObject roadContainer;

    void Start()
    {
        gridGen = GetComponent<GridGenerator_Triangle>();
        Invoke("GenerateRoads", 1.2f);
    }

    public void GenerateRoads()
    {
        if (gridGen == null || gridGen.allNodes == null) return;
        if (roadContainer != null) Destroy(roadContainer);

        // Konteynırı oluştur ve "Ignore Raycast" katmanına al (Layer 2)
        roadContainer = new GameObject("TRIANGLE_STREET_NETWORK");
        roadContainer.layer = 2;

        HashSet<string> drawnEdges = new HashSet<string>();

        foreach (Node n1 in gridGen.allNodes)
        {
            CreateTriangleJunction(n1.transform.position);

            foreach (Node n2 in n1.Neighbors)
            {
                float dist = Vector3.Distance(n1.transform.position, n2.transform.position);
                if (dist > gridGen.spacing * 1.2f) continue;

                string edgeId = GetEdgeId(n1, n2);
                if (drawnEdges.Contains(edgeId)) continue;

                DrawTriangleStreet(n1.transform.position, n2.transform.position);
                drawnEdges.Add(edgeId);
            }
        }
    }

    string GetEdgeId(Node n1, Node n2)
    {
        int id1 = n1.GetInstanceID();
        int id2 = n2.GetInstanceID();
        return id1 < id2 ? id1 + "_" + id2 : id2 + "_" + id1;
    }

    void DrawTriangleStreet(Vector3 start, Vector3 end)
    {
        Vector3 dir = (end - start).normalized;
        float dist = Vector3.Distance(start, end);
        Vector3 center = (start + end) / 2f;

        // ASFALT
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.name = "Triangle_Asphalt";
        road.transform.SetParent(roadContainer.transform);
        road.layer = 2; // Ignore Raycast

        road.transform.position = center + Vector3.up * 0.01f;
        road.transform.rotation = Quaternion.LookRotation(dir);
        road.transform.localScale = new Vector3(roadWidth, 0.01f, dist);

        if (roadMaterial != null) road.GetComponent<Renderer>().material = roadMaterial;

        // COLLIDER TEMİZLİĞİ (Tıklamayı engellememesi için)
        Destroy(road.GetComponent<BoxCollider>());

        CreateSidewalk(center, dir, dist, 1);
        CreateSidewalk(center, dir, dist, -1);
    }

    void CreateSidewalk(Vector3 center, Vector3 dir, float length, int side)
    {
        Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;
        Vector3 sidewalkPos = center + (right * (roadWidth / 2f + sidewalkWidth / 2f) * side) + Vector3.up * 0.02f;

        GameObject sidewalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sidewalk.name = "Sidewalk";
        sidewalk.transform.SetParent(roadContainer.transform);
        sidewalk.layer = 2; // Ignore Raycast

        sidewalk.transform.position = sidewalkPos;
        sidewalk.transform.rotation = Quaternion.LookRotation(dir);
        sidewalk.transform.localScale = new Vector3(sidewalkWidth, 0.015f, length);

        if (sidewalkMaterial != null) sidewalk.GetComponent<Renderer>().material = sidewalkMaterial;

        // COLLIDER TEMİZLİĞİ
        Destroy(sidewalk.GetComponent<BoxCollider>());
    }

    void CreateTriangleJunction(Vector3 pos)
    {
        GameObject junction = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        junction.name = "Junction_Core";
        junction.transform.SetParent(roadContainer.transform);
        junction.layer = 2; // Ignore Raycast

        junction.transform.position = pos + Vector3.up * 0.012f;

        float size = roadWidth + (sidewalkWidth * 2);
        junction.transform.localScale = new Vector3(size, 0.005f, size);

        if (roadMaterial != null) junction.GetComponent<Renderer>().material = roadMaterial;

        // KRİTİK: Silindirde CapsuleCollider olur, onu siliyoruz
        Destroy(junction.GetComponent<CapsuleCollider>());
    }
}