using UnityEngine;
using System.Collections.Generic;

public class CityBuilder : MonoBehaviour
{
    private GridGenerator gridGen;
    private GameObject[] wasteAssets;

    [Header("Atık Dağılım Ayarları")]
    [Range(0f, 1f)] public float spawnChance = 0.7f;
    public float yOffset = 0.05f;

    private List<Vector3> occupiedPositions = new List<Vector3>();
    private List<float> occupiedRadii = new List<float>();

    void Start()
    {
        gridGen = GetComponent<GridGenerator>();
        LoadAssets();
        Invoke(nameof(BuildCity), 0.8f);
    }

    void LoadAssets()
    {
        wasteAssets = Resources.LoadAll<GameObject>("Buildings/Residential");
    }

    public void BuildCity()
    {
        if (gridGen == null || gridGen.hexCenters == null || gridGen.hexCenters.Count == 0)
        {
            Debug.LogWarning("CityBuilder: hexCenters boş!");
            return;
        }
        ClearCity();

        // --- REZERVASYON SİSTEMİ ---
        for (int i = 0; i < gridGen.hexCenters.Count; i++)
        {
            // Eğer bu ilk altıgense (0. indeks), buraya çöp yapma, boş bırak.
            if (i == 0)
            {
                Debug.Log("Merkez nokta (Index 0) Bank üretimi için boş bırakıldı.");
                continue;
            }

            BuildHexCell(gridGen.hexCenters[i]);
        }
    }

    void BuildHexCell(Vector3 center)
    {
        float s = gridGen.hexSize;
        float inRadius = s * Mathf.Sqrt(3f) * 0.5f * 0.80f;

        int gridCols = 4;
        int gridRows = 4;
        float bw = s * 0.20f;
        float bhMin = 0.5f;
        float bhMax = 1.3f;
        float spacing = s * 0.22f;

        float stepX = (inRadius * 2f) / (gridCols + 1);
        float stepZ = (inRadius * 2f) / (gridRows + 1);

        float[] rowAngles = { 0f, 60f, 0f, 60f };
        List<GameObject> usedPrefabs = new List<GameObject>();

        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridCols; col++)
            {
                if (Random.value > spawnChance) continue;

                float ox = (col - (gridCols - 1) * 0.5f) * stepX;
                float oz = (row - (gridRows - 1) * 0.5f) * stepZ;

                Vector3 spawnPos = new Vector3(center.x + ox, yOffset, center.z + oz);

                if (!IsInsideHex(spawnPos, center, inRadius)) continue;
                if (IsOccupied(spawnPos, spacing)) continue;

                GameObject prefab = null;
                float bwActual = bw;

                if (wasteAssets != null && wasteAssets.Length > 0)
                {
                    prefab = PickUnused(wasteAssets, usedPrefabs);
                    bwActual = bw * 0.85f;
                }

                if (prefab == null) continue;

                float baseAngle = rowAngles[row % rowAngles.Length];
                float jitter = Random.Range(-5f, 5f);

                usedPrefabs.Add(prefab);
                SpawnBuilding(prefab, spawnPos, Random.Range(bhMin, bhMax), bwActual, baseAngle + jitter, spacing);
            }
        }
    }

    // --- YARDIMCI FONKSİYONLAR ---

    bool IsInsideHex(Vector3 pos, Vector3 center, float inRadius)
    {
        float dx = pos.x - center.x;
        float dz = pos.z - center.z;
        for (int i = 0; i < 6; i++)
        {
            float angleRad = (30f + 60f * i) * Mathf.Deg2Rad;
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

        float uniformScale = w;
        b.transform.localScale = new Vector3(uniformScale, uniformScale, uniformScale);

        Vector3 currentPos = b.transform.position;
        float liftAmount = (uniformScale * 0.55f) + yOffset + 0.1f;

        currentPos.y += liftAmount;
        b.transform.position = currentPos;

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