using UnityEngine;
using System.Collections.Generic;

public class RoadMeshGenerator : MonoBehaviour
{
    private GridGenerator gridGen;

    [Header("Yol Ayarları")]
    public float roadWidth = 1.8f;
    public float sidewalkWidth = 0.5f;
    public Material roadMaterial;
    public Material sidewalkMaterial;

    private GameObject roadContainer;

    void Start()
    {
        gridGen = GetComponent<GridGenerator>();
        // CityBuilder'dan biraz daha sonra çalışsın ki temizlik bitsin
        Invoke("GenerateRoads", 1.0f);
    }

    public void GenerateRoads()
    {
        if (gridGen == null || gridGen.allNodes == null) return;

        // Eski yolları temizle (Eğer varsa)
        if (roadContainer != null) Destroy(roadContainer);

        // Yolları korumak için özel bir obje oluşturuyoruz
        roadContainer = new GameObject("STREET_NETWORK");
        roadContainer.transform.position = Vector3.zero;

        HashSet<string> drawnEdges = new HashSet<string>();

        foreach (Node n1 in gridGen.allNodes)
        {
            CreateJunction(n1.transform.position);

            foreach (Node n2 in n1.Neighbors)
            {
                string edgeId = GetEdgeId(n1, n2);
                if (drawnEdges.Contains(edgeId)) continue;

                DrawStreet(n1.transform.position, n2.transform.position);
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

    void DrawStreet(Vector3 start, Vector3 end)
    {
        Vector3 dir = (end - start).normalized;
        float dist = Vector3.Distance(start, end);
        Vector3 center = (start + end) / 2f;

        // ASFALT ŞERİDİ
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube); // Quad yerine Cube yapalım kalınlık olsun
        road.name = "Street_Segment";
        road.transform.SetParent(roadContainer.transform);

        // Pozisyonu tam ortaya al
        road.transform.position = center + Vector3.up * 0.02f;

        // Yolu hedefe doğru döndür
        road.transform.rotation = Quaternion.LookRotation(dir);

        // BURASI KRİTİK: X = yol genişliği, Y = çok ince (yükseklik), Z = iki node arasındaki mesafe
        road.transform.localScale = new Vector3(roadWidth, 0.01f, dist);

        if (roadMaterial != null) road.GetComponent<Renderer>().material = roadMaterial;

        // Collider'ı yok et ki seçimleri bozmasın
        Destroy(road.GetComponent<BoxCollider>());
    }

    void CreateSidewalk(Vector3 center, Vector3 dir, float length, int side)
    {
        Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;
        Vector3 sidewalkPos = center + (right * (roadWidth / 2f + sidewalkWidth / 2f) * side) + Vector3.up * 0.06f;

        GameObject sidewalk = GameObject.CreatePrimitive(PrimitiveType.Quad);
        sidewalk.name = "Sidewalk";
        sidewalk.transform.SetParent(roadContainer.transform);
        sidewalk.transform.position = sidewalkPos;
        sidewalk.transform.rotation = Quaternion.LookRotation(Vector3.up, dir);
        sidewalk.transform.localScale = new Vector3(sidewalkWidth, length, 1f);

        if (sidewalkMaterial != null) sidewalk.GetComponent<Renderer>().material = sidewalkMaterial;
        Destroy(sidewalk.GetComponent<Collider>());
    }

    void CreateJunction(Vector3 pos)
    {
        // Kavşakları silindir yerine Quad yapalım, daha hafif olur
        GameObject junction = GameObject.CreatePrimitive(PrimitiveType.Quad);
        junction.name = "Junction";
        junction.transform.SetParent(roadContainer.transform);
        // Yolların hafifçe altında kalsın ki birleşimler düzgün görünsün
        junction.transform.position = pos + Vector3.up * 0.04f;
        junction.transform.rotation = Quaternion.Euler(90, 0, 0);
        junction.transform.localScale = new Vector3(roadWidth + sidewalkWidth * 2f, roadWidth + sidewalkWidth * 2f, 1f);

        if (roadMaterial != null) junction.GetComponent<Renderer>().material = roadMaterial;
        Destroy(junction.GetComponent<Collider>());
    }
}