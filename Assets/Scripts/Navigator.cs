using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Navigator : MonoBehaviour
{
    private List<Node> path;
    private int targetIndex = 0;
    public float speed = 10f;
    private string systemName;

    private float totalCO2 = 0f;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private bool isCalculating = false;
    private bool isWaiting = false;

    [Header("Envanter")]
    public int woodCount = 0;
    public int metalCount = 0;
    public const int targetWood = 7;
    public const int targetMetal = 4;
    public int totalBenchesProduced = 0;

    [Header("Üretim")]
    public GameObject bankPrefab;

    private List<int> stopIndexes = new List<int>();

    public void StartJourney(List<Node> newPath, string system)
    {
        path = newPath;
        systemName = system;
        targetIndex = 0;
        totalCO2 = 0f;
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        isCalculating = true;
        isWaiting = false;
        woodCount = 0;
        metalCount = 0;
        DetermineStops();
    }

    void DetermineStops()
    {
        stopIndexes.Clear();
        if (path == null || path.Count <= 5) return;

        int stopCount = Mathf.Clamp((path.Count - 4) / 3, 1, 5);
        int attempts = 0;
        while (stopIndexes.Count < stopCount && attempts < 100)
        {
            int r = Random.Range(2, path.Count - 2);
            if (!stopIndexes.Contains(r)) stopIndexes.Add(r);
            attempts++;
        }
    }

    void Update()
    {
        if (path == null || targetIndex >= path.Count || isWaiting) return;

        if (Input.GetKeyDown(KeyCode.O)) TryProduceBench();

        Vector3 targetPos = path[targetIndex].transform.position;
        targetPos.y = transform.position.y;

        // Hareket
        transform.position = Vector3.MoveTowards(
            transform.position, targetPos, speed * Time.deltaTime);

        // Rotasyon
        Vector3 dir = targetPos - transform.position;
        if (dir.magnitude > 0.1f)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                10f * Time.deltaTime);

        // CO2 hesaplama
        // Formül: mesafe * yakıt katsayısı + dönüş açısı * manevra katsayısı
        // Birim: kg CO2 (simüle edilmiş, ölçeklendirme katsayılarıyla)
        if (isCalculating)
        {
            float dist = Vector3.Distance(transform.position, lastPosition);
            float angleDiff = Quaternion.Angle(transform.rotation, lastRotation);
            totalCO2 += (dist * 0.05f) + (angleDiff * 0.02f);
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }

        // Hedefe ulaşıldı mı
        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            if (stopIndexes.Contains(targetIndex))
                StartCoroutine(CollectWaste());

            targetIndex++;

            if (targetIndex >= path.Count)
            {
                isCalculating = false;
                SaveCarbonData();
            }
        }
    }

    void SaveCarbonData()
    {
        // systemName → PlayerPrefs key dönüşümü
        string keyName;
        if (systemName == "TRANSİT") keyName = "Hex";
        else if (systemName == "ÜÇGEN") keyName = "Tri";
        else if (systemName == "KARE") keyName = "Square";
        else keyName = systemName;

        PlayerPrefs.SetFloat(keyName + "_RealCO2", totalCO2);
        PlayerPrefs.SetInt(keyName + "_Olculdu", 1);
        PlayerPrefs.Save();

        Debug.Log($"[Navigator] {keyName} → CO2: {totalCO2:F2}kg kaydedildi.");
    }

    IEnumerator CollectWaste()
    {
        isWaiting = true;

        // Kalan durak sayısına göre kaynak dağıt
        int remainingStops = 0;
        foreach (int i in stopIndexes)
            if (i >= targetIndex) remainingStops++;
        remainingStops = Mathf.Max(1, remainingStops);

        int remWood = Mathf.Max(0, targetWood - woodCount);
        int remMetal = Mathf.Max(0, targetMetal - metalCount);

        int addWood = (remainingStops == 1)
            ? remWood
            : Random.Range(1, Mathf.Max(2, remWood / remainingStops + 2));
        int addMetal = (remainingStops == 1)
            ? remMetal
            : Random.Range(1, Mathf.Max(2, remMetal / remainingStops + 2));

        woodCount = Mathf.Min(woodCount + addWood, targetWood);
        metalCount = Mathf.Min(metalCount + addMetal, targetMetal);

        Debug.Log($"[Navigator] Atık toplandı → Odun: {woodCount}/{targetWood}, Metal: {metalCount}/{targetMetal}");

        yield return new WaitForSeconds(1.2f);
        isWaiting = false;
    }

    public void TryProduceBench()
    {
        if (woodCount < targetWood || metalCount < targetMetal)
        {
            Debug.LogWarning($"[Navigator] Yetersiz kaynak! Odun: {woodCount}/{targetWood}, Metal: {metalCount}/{targetMetal}");
            return;
        }

        if (bankPrefab == null)
            bankPrefab = Resources.Load<GameObject>("BankaModeli");

        if (bankPrefab == null)
        {
            Debug.LogError("[Navigator] BankaModeli prefabı bulunamadı!");
            return;
        }

        // Spawn pozisyonu — aktif grid sistemine göre belirle
        Vector3 spawnPos = GetBankSpawnPosition();
        spawnPos.y = 0.372f;

        GameObject newBench = Instantiate(bankPrefab, spawnPos, Quaternion.identity);
        woodCount = 0;
        metalCount = 0;
        totalBenchesProduced++;

        Debug.Log($"[Navigator] Üretim başarılı! Toplam: {totalBenchesProduced}");

        var cam = Object.FindFirstObjectByType<CameraController>();
        if (cam != null) cam.FocusOnTarget(newBench.transform);
    }

    Vector3 GetBankSpawnPosition()
    {
        // 1. Hexagonal grid
        var hexGrid = Object.FindFirstObjectByType<GridGenerator>();
        if (hexGrid != null && hexGrid.hexCenters != null && hexGrid.hexCenters.Count > 0)
            return hexGrid.hexCenters[0];

        // 2. Üçgen grid — merkez node
        var triGrid = Object.FindFirstObjectByType<GridGenerator_Triangle>();
        if (triGrid != null && triGrid.allNodes != null && triGrid.allNodes.Count > 0)
            return triGrid.allNodes[0].transform.position;

        // 3. Kare grid — ilk iki node'un ortası
        var sqGrid = Object.FindFirstObjectByType<GridGenerator_Square>();
        if (sqGrid != null && sqGrid.allNodes != null && sqGrid.allNodes.Count > 1)
        {
            Node n1 = sqGrid.allNodes[0];
            Node diag = sqGrid.GetNode(n1.GridCoord.x + 1, n1.GridCoord.y + 1);
            if (diag != null)
                return (n1.transform.position + diag.transform.position) * 0.5f;
            return n1.transform.position;
        }

        return Vector3.zero;
    }

    public float GetCO2() => totalCO2;
}