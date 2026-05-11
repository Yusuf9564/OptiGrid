using UnityEngine;
using System.Collections.Generic;

public class CityBuilder_Triangle : MonoBehaviour
{
    private GridGenerator_Triangle gridGen;
    private GameObject[] wasteAssets;

    [Header("Atık Dağılım Ayarları")]
    [Range(0f, 1f)] public float spawnChance = 0.65f;
    public float yOffset = 0.05f;
    public float minBuildingSpacing = 1.8f;
    public Vector3 reservedBankPos;

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
        wasteAssets = Resources.LoadAll<GameObject>("Buildings/Residential");
    }

    public void BuildTriangleCity()
    {
        if (gridGen == null || gridGen.allNodes == null || gridGen.allNodes.Count == 0) return;
        ClearCity();

        float s = gridGen.spacing;
        float maxDist = s * 1.25f;
        HashSet<string> processed = new HashSet<string>();

        int blockIndex = 0; // Blok sayacı

        foreach (Node n1 in gridGen.allNodes)
        {
            foreach (Node n2 in n1.Neighbors)
            {
                if (Vector3.Distance(n1.transform.position, n2.transform.position) > maxDist) continue;
                foreach (Node n3 in n2.Neighbors)
                {
                    if (n3 == n1 || !n3.Neighbors.Contains(n1)) continue;

                    int[] ids = { n1.GetInstanceID(), n2.GetInstanceID(), n3.GetInstanceID() };
                    System.Array.Sort(ids);
                    string key = ids[0] + "_" + ids[1] + "_" + ids[2];
                    if (processed.Contains(key)) continue;
                    processed.Add(key);

                    Vector3 center = (n1.transform.position + n2.transform.position + n3.transform.position) / 3f;
                    center.y = 0f;

                    // --- REZERVASYON KONTROLÜ ---
                    if (blockIndex == 0)
                    {
                        reservedBankPos = center; // İşte tam o boşluğun merkezini kaydettik
                        Debug.Log("Triangle Bank Alanı Kaydedildi: " + reservedBankPos);
                        blockIndex++;
                        continue;
                    }

                    BuildTriangleBlock(center, n1, n2, n3, s);
                    blockIndex++;
                }
            }
        }
    }

    void BuildTriangleBlock(Vector3 center, Node n1, Node n2, Node n3, float s)
    {
        // Eşkenar üçgenin iç çemberi çok dardır, güvenli alanı %45'e çekiyoruz
        float inRadius = (s / (2f * Mathf.Sqrt(3f))) * 0.45f;

        // Üçgen içine 4x4 yerine 2x2 veya 3x3 dizmek taşmayı önler
        int gridCols = 2;
        int gridRows = 2;

        // Genişlik çarpanını (bw) s * 0.12f gibi çok daha küçük bir değere çekiyoruz
        float bw = s * 0.12f;
        float spacing = s * 0.15f;

        float stepX = (inRadius * 2f) / (gridCols + 1);
        float stepZ = (inRadius * 2f) / (gridRows + 1);

        Vector3 edge1 = (n2.transform.position - n1.transform.position).normalized;
        float a1 = Mathf.Atan2(edge1.z, edge1.x) * Mathf.Rad2Deg;
        float[] roadAngles = { a1, a1 + 120f, a1 + 240f };

        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridCols; col++)
            {
                if (Random.value > spawnChance) continue;

                float ox = (col - (gridCols - 1) * 0.5f) * stepX;
                float oz = (row - (gridRows - 1) * 0.5f) * stepZ;
                Vector3 spawnPos = new Vector3(center.x + ox, 0f, center.z + oz);

                // IsInsideTriangle kontrolü zaten dışa taşmayı engelliyor
                if (!IsInsideTriangle(spawnPos, center, inRadius)) continue;
                if (IsOccupied(spawnPos, spacing)) continue;

                if (wasteAssets.Length > 0)
                {
                    GameObject prefab = wasteAssets[Random.Range(0, wasteAssets.Length)];
                    float yRot = roadAngles[row % 3] + Random.Range(-5f, 5f);

                    // bw değerini doğrudan gönderiyoruz
                    SpawnBuilding(prefab, spawnPos, bw, yRot, spacing);
                }
            }
        }
    }

    void SpawnBuilding(GameObject prefab, Vector3 pos, float scale, float yRot, float radius)
    {
        GameObject b = Instantiate(prefab, pos, Quaternion.identity, transform);
        b.transform.rotation = Quaternion.Euler(0f, yRot, 0f);

        // finalScale'i hem gelen scale hem de radius'un küçük bir yüzdesiyle sınırlıyoruz
        // Hangisi daha küçükse onu alarak objeyi iyice ufalatıyoruz
        float finalScale = Mathf.Min(scale, radius * 0.6f);

        b.transform.localScale = new Vector3(finalScale, finalScale, finalScale);

        Vector3 cPos = b.transform.position;
        // Yüksekliğin yarısı kadar yukarı + yOffset
        cPos.y += (finalScale * 0.6f) + yOffset;
        b.transform.position = cPos;

        occupiedPositions.Add(pos);
        occupiedRadii.Add(radius);
    }

    bool IsInsideTriangle(Vector3 pos, Vector3 center, float inRadius)
    {
        float dx = pos.x - center.x; float dz = pos.z - center.z;
        for (int i = 0; i < 3; i++)
        {
            float angleRad = (90f + 120f * i) * Mathf.Deg2Rad;
            if (dx * Mathf.Cos(angleRad) + dz * Mathf.Sin(angleRad) > inRadius) return false;
        }
        return true;
    }

    bool IsOccupied(Vector3 pos, float radius)
    {
        for (int i = 0; i < occupiedPositions.Count; i++)
            if (Vector3.Distance(occupiedPositions[i], pos) < (occupiedRadii[i] + radius) * 0.5f) return true;
        return false;
    }

    void ClearCity()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            if (transform.GetChild(i).GetComponent<Node>() == null) DestroyImmediate(transform.GetChild(i).gameObject);
        occupiedPositions.Clear(); occupiedRadii.Clear();
    }
}