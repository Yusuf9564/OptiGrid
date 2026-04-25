using UnityEngine;
using System.Collections.Generic;

public class CityBuilder_Triangle : MonoBehaviour
{
    private GridGenerator_Triangle gridGen;
    private GameObject[] skyscrapers;
    private GameObject[] midRangeBuildings;
    private GameObject[] residentialHouses;

    [Header("Yerleşim Ayarları")]
    public float minBuildingSpacing = 1.8f;

    [Header("Bölge Eşikleri (spacing çarpanı)")]
    public float skyscraperZoneMultiplier = 2f;
    public float midRangeZoneMultiplier = 5f;

    private List<Vector3> occupiedPositions = new List<Vector3>();
    private List<float> occupiedRadii = new List<float>();

    void Start()
    {
        gridGen = GetComponent<GridGenerator_Triangle>();
        LoadAssets();
        Invoke(nameof(BuildTriangleCity), 0.8f);
    }

    void LoadAssets()
    {
        skyscrapers = Resources.LoadAll<GameObject>("Buildings/Skyscrapers");
        midRangeBuildings = Resources.LoadAll<GameObject>("Buildings/MidRange");
        residentialHouses = Resources.LoadAll<GameObject>("Buildings/Residential");
    }

    public void BuildTriangleCity()
    {
        if (gridGen == null || gridGen.allNodes == null || gridGen.allNodes.Count == 0) return;
        ClearCity();

        float s = gridGen.spacing;
        float maxDist = s * 1.25f;

        HashSet<string> processed = new HashSet<string>();

        foreach (Node n1 in gridGen.allNodes)
        {
            foreach (Node n2 in n1.Neighbors)
            {
                if (Vector3.Distance(n1.transform.position, n2.transform.position) > maxDist) continue;

                foreach (Node n3 in n2.Neighbors)
                {
                    if (n3 == n1) continue;
                    if (Vector3.Distance(n2.transform.position, n3.transform.position) > maxDist) continue;
                    if (Vector3.Distance(n3.transform.position, n1.transform.position) > maxDist) continue;
                    if (!n3.Neighbors.Contains(n1)) continue;

                    // Her üçgeni yalnızca bir kez işle
                    int id1 = n1.GetInstanceID();
                    int id2 = n2.GetInstanceID();
                    int id3 = n3.GetInstanceID();
                    int minId = Mathf.Min(id1, Mathf.Min(id2, id3));
                    int maxId = Mathf.Max(id1, Mathf.Max(id2, id3));
                    int midId = id1 + id2 + id3 - minId - maxId;

                    string key = minId + "_" + midId + "_" + maxId;
                    if (processed.Contains(key)) continue;
                    processed.Add(key);

                    Vector3 center = (n1.transform.position +
                                     n2.transform.position +
                                     n3.transform.position) / 3f;
                    center.y = 0f;

                    BuildTriangleBlock(center, n1, n2, n3, s);
                }
            }
        }
    }

    void BuildTriangleBlock(Vector3 center, Node n1, Node n2, Node n3, float s)
    {
        float dist = new Vector2(center.x, center.z).magnitude;

        bool isSky = dist < s * skyscraperZoneMultiplier;
        bool isMid = dist < s * midRangeZoneMultiplier;

        // Eşkenar üçgenin iç çember yarıçapı = kenar / (2 * sqrt(3))
        // %55 al — binalar kesinlikle yola taşmaz
        float inRadius = (s / (2f * Mathf.Sqrt(3f))) * 0.55f;

        // Bölgeye göre ızgara ve bina parametreleri
        int gridCols, gridRows;
        float bw, bhMin, bhMax, spacing;

        if (isSky)
        {
            gridCols = 2; gridRows = 2;
            bw = s * 0.28f;  // 0.42 → 0.28
            bhMin = 5f; bhMax = 9f;
            spacing = s * 0.35f;  // 0.52 → 0.35
        }
        else if (isMid)
        {
            gridCols = 3; gridRows = 2;
            bw = s * 0.20f;  // 0.30 → 0.20
            bhMin = 2f; bhMax = 3.5f;
            spacing = s * 0.25f;  // 0.36 → 0.25
        }
        else
        {
            gridCols = 4; gridRows = 4;
            bw = s * 0.18f;
            bhMin = 0.5f; bhMax = 1.3f;
            spacing = s * 0.20f;
        }

        float stepX = (inRadius * 2f) / (gridCols + 1);
        float stepZ = (inRadius * 2f) / (gridRows + 1);

        // Üçgenin kenar yönleri — yola paralel rotasyon için
        Vector3 edge1 = (n2.transform.position - n1.transform.position).normalized;
        Vector3 edge2 = (n3.transform.position - n2.transform.position).normalized;
        Vector3 edge3 = (n1.transform.position - n3.transform.position).normalized;

        float a1 = Mathf.Atan2(edge1.z, edge1.x) * Mathf.Rad2Deg;
        float a2 = Mathf.Atan2(edge2.z, edge2.x) * Mathf.Rad2Deg;
        float a3 = Mathf.Atan2(edge3.z, edge3.x) * Mathf.Rad2Deg;
        float[] roadAngles = { a1, a2, a3 };

        List<GameObject> usedPrefabs = new List<GameObject>();

        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridCols; col++)
            {
                float ox = (col - (gridCols - 1) * 0.5f) * stepX;
                float oz = (row - (gridRows - 1) * 0.5f) * stepZ;

                Vector3 spawnPos = new Vector3(center.x + ox, 0f, center.z + oz);

                if (!IsInsideTriangle(spawnPos, center, inRadius)) continue;
                if (IsOccupied(spawnPos, spacing)) continue;

                GameObject prefab = null;
                float bwActual = bw;
                float bh = Random.Range(bhMin, bhMax);

                if (isSky && skyscrapers != null && skyscrapers.Length > 0)
                {
                    prefab = PickUnused(skyscrapers, usedPrefabs);
                }
                else if (isMid && midRangeBuildings != null && midRangeBuildings.Length > 0)
                {
                    prefab = PickUnused(midRangeBuildings, usedPrefabs);
                }
                else if (residentialHouses != null && residentialHouses.Length > 0)
                {
                    prefab = PickUnused(residentialHouses, usedPrefabs);
                    bwActual = bw * 0.85f;
                }

                if (prefab == null) continue;

                // Yola paralel rotasyon
                float baseAngle = roadAngles[row % roadAngles.Length];
                float jitter = Random.Range(-5f, 5f);

                usedPrefabs.Add(prefab);
                SpawnBuilding(prefab, spawnPos, bh, bwActual, baseAngle + jitter, spacing);
            }
        }
    }

    // Üçgenin iç çemberine göre sınır kontrolü
    bool IsInsideTriangle(Vector3 pos, Vector3 center, float inRadius)
    {
        float dx = pos.x - center.x;
        float dz = pos.z - center.z;

        // Eşkenar üçgenin 3 kenar normali — her 120°'de bir
        for (int i = 0; i < 3; i++)
        {
            float angleRad = (90f + 120f * i) * Mathf.Deg2Rad;
            float dot = dx * Mathf.Cos(angleRad) + dz * Mathf.Sin(angleRad);
            if (dot > inRadius) return false;
        }
        return true;
    }

    bool IsOccupied(Vector3 pos, float radius)
    {
        for (int i = 0; i < occupiedPositions.Count; i++)
        {
            float minDist = (occupiedRadii[i] + radius) * 0.5f;
            if (Vector3.Distance(occupiedPositions[i], pos) < minDist)
                return true;
        }
        return false;
    }

    GameObject PickUnused(GameObject[] pool, List<GameObject> used)
    {
        List<GameObject> available = new List<GameObject>();
        foreach (GameObject g in pool)
            if (!used.Contains(g)) available.Add(g);

        if (available.Count == 0)
        {
            used.Clear();
            return pool[Random.Range(0, pool.Length)];
        }
        return available[Random.Range(0, available.Count)];
    }

    void SpawnBuilding(GameObject prefab, Vector3 pos, float h, float w, float yRot, float radius)
    {
        GameObject b = Instantiate(prefab, pos, Quaternion.identity, transform);
        b.transform.rotation = Quaternion.Euler(0f, yRot, 0f);
        b.transform.localScale = new Vector3(w, h, w);
        occupiedPositions.Add(pos);
        occupiedRadii.Add(radius);
    }

    void ClearCity()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.GetComponent<Node>() == null)
                DestroyImmediate(child.gameObject);
        }
        occupiedPositions.Clear();
        occupiedRadii.Clear();
    }
}