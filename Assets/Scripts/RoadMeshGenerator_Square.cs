using UnityEngine;
using System.Collections.Generic;

public class RoadMeshGenerator_Square : MonoBehaviour
{
    private GridGenerator_Square gridGen;

    [Header("Yol Ayarları")]
    public float roadWidth = 2.0f;      // Kare sisteminde yollar biraz daha geniş olabilir
    public float sidewalkWidth = 0.6f;
    public Material roadMaterial;
    public Material sidewalkMaterial;

    private GameObject roadContainer;

    void Start()
    {
        gridGen = GetComponent<GridGenerator_Square>();
        Invoke("GenerateRoads", 0.8f);
    }

    public void GenerateRoads()
    {
        if (gridGen == null || gridGen.allNodes == null) return;
        if (roadContainer != null) Destroy(roadContainer);

        roadContainer = new GameObject("SQUARE_STREET_NETWORK");

        HashSet<string> drawnEdges = new HashSet<string>();

        foreach (Node n1 in gridGen.allNodes)
        {
            // Kavşak (Kare şeklinde)
            CreateJunction(n1.transform.position);

            foreach (Node n2 in n1.Neighbors)
            {
                // Çapraz komşuları Neighbors içinden değil, sadece düz yolları çiz
                if (Vector3.Distance(n1.transform.position, n2.transform.position) > gridGen.spacing + 0.1f) continue;

                string edgeId = GetEdgeId(n1, n2);
                if (drawnEdges.Contains(edgeId)) continue;

                DrawSquareStreet(n1.transform.position, n2.transform.position);
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

    void DrawSquareStreet(Vector3 start, Vector3 end)
    {
        Vector3 dir = (end - start).normalized;
        float dist = Vector3.Distance(start, end);
        Vector3 center = (start + end) / 2f;

        // ASFALT ŞERİDİ (Küp kullanarak derinlik veriyoruz)
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.name = "Square_Asphalt";
        road.transform.SetParent(roadContainer.transform);
        road.transform.position = center + Vector3.up * 0.02f;
        road.transform.rotation = Quaternion.LookRotation(dir);
        road.transform.localScale = new Vector3(roadWidth, 0.01f, dist);

        if (roadMaterial != null) road.GetComponent<Renderer>().material = roadMaterial;
        Destroy(road.GetComponent<BoxCollider>());

        // KALDIRIMLAR
        CreateSidewalk(center, dir, dist, 1);
        CreateSidewalk(center, dir, dist, -1);
    }

    void CreateSidewalk(Vector3 center, Vector3 dir, float length, int side)
    {
        Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;
        Vector3 sidewalkPos = center + (right * (roadWidth / 2f + sidewalkWidth / 2f) * side) + Vector3.up * 0.03f;

        GameObject sidewalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sidewalk.name = "Sidewalk";
        sidewalk.transform.SetParent(roadContainer.transform);
        sidewalk.transform.position = sidewalkPos;
        sidewalk.transform.rotation = Quaternion.LookRotation(dir);
        sidewalk.transform.localScale = new Vector3(sidewalkWidth, 0.01f, length);

        if (sidewalkMaterial != null) sidewalk.GetComponent<Renderer>().material = sidewalkMaterial;
        Destroy(sidewalk.GetComponent<BoxCollider>());
    }

    void CreateJunction(Vector3 pos)
    {
        GameObject junction = GameObject.CreatePrimitive(PrimitiveType.Cube);
        junction.name = "Square_Junction";
        junction.transform.SetParent(roadContainer.transform);
        junction.transform.position = pos + Vector3.up * 0.015f;
        junction.transform.localScale = new Vector3(roadWidth + sidewalkWidth * 2.2f, 0.01f, roadWidth + sidewalkWidth * 2.2f);

        if (roadMaterial != null) junction.GetComponent<Renderer>().material = roadMaterial;
        Destroy(junction.GetComponent<BoxCollider>());
    }
}