using UnityEngine;
using System.Collections.Generic;

public class CityBuilder : MonoBehaviour
{
    private GridGenerator gridGen;

    private GameObject[] skyscrapers;
    private GameObject[] midRangeBuildings;
    private GameObject[] residentialHouses;

    [Header("Bölge Eşikleri (hexSize çarpanı)")]
    public float skyscraperZoneMultiplier = 2f;
    public float midRangeZoneMultiplier = 4f;

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
        skyscrapers = Resources.LoadAll<GameObject>("Buildings/Skyscrapers");
        midRangeBuildings = Resources.LoadAll<GameObject>("Buildings/MidRange");
        residentialHouses = Resources.LoadAll<GameObject>("Buildings/Residential");
    }

    public void BuildCity()
    {
        if (gridGen == null || gridGen.hexCenters == null || gridGen.hexCenters.Count == 0)
        {
            Debug.LogWarning("CityBuilder: hexCenters boş!");
            return;
        }
        ClearCity();

        foreach (Vector3 hexCenter in gridGen.hexCenters)
            BuildHexCell(hexCenter);
    }

    void BuildHexCell(Vector3 center)
    {
        float s = gridGen.hexSize;
        float dist = new Vector2(center.x, center.z).magnitude;

        bool isSky = dist < s * skyscraperZoneMultiplier;
        bool isMid = dist < s * midRangeZoneMultiplier;

        float inRadius = s * Mathf.Sqrt(3f) * 0.5f * 0.80f;

        // ─────────────────────────────────────────────
        // Bölgeye göre ızgara ve bina parametreleri
        // Gökdelen: 2x2, büyük bina → sadece 1-2 tanesi sığar
        // MidRange:  3x2, orta bina → 3-4 tanesi sığar
        // Konut:     4x4, küçük bina → 8-12 tanesi sığar
        // ─────────────────────────────────────────────
        int gridCols, gridRows;
        float bw, bhMin, bhMax, spacing;

        if (isSky)
        {
            gridCols = 2; gridRows = 2;
            bw = s * 0.45f;
            bhMin = 5f; bhMax = 9f;
            spacing = s * 0.55f; // büyük bina → geniş mesafe → az sayıda
        }
        else if (isMid)
        {
            gridCols = 3; gridRows = 2;
            bw = s * 0.32f;
            bhMin = 2f; bhMax = 3.5f;
            spacing = s * 0.38f; // orta bina → orta mesafe
        }
        else
        {
            gridCols = 4; gridRows = 4;
            bw = s * 0.20f;
            bhMin = 0.5f; bhMax = 1.3f;
            spacing = s * 0.22f; // küçük bina → sık
        }

        float stepX = (inRadius * 2f) / (gridCols + 1);
        float stepZ = (inRadius * 2f) / (gridRows + 1);

        float[] rowAngles = { 0f, 60f, 0f, 60f };
        List<GameObject> usedPrefabs = new List<GameObject>();

        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridCols; col++)
            {
                float ox = (col - (gridCols - 1) * 0.5f) * stepX;
                float oz = (row - (gridRows - 1) * 0.5f) * stepZ;

                Vector3 spawnPos = new Vector3(center.x + ox, 0f, center.z + oz);

                if (!IsInsideHex(spawnPos, center, inRadius)) continue;
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

                float baseAngle = rowAngles[row % rowAngles.Length];
                float jitter = Random.Range(-5f, 5f);

                usedPrefabs.Add(prefab);
                SpawnBuilding(prefab, spawnPos, bh, bwActual, baseAngle + jitter, spacing);
            }
        }
    }

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
            // Her bina kendi yarıçapını + yeni binanın yarıçapını korur
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