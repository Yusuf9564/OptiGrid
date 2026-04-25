using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class InputManager_Hexagonal : MonoBehaviour
{
    public Node nodeA, nodeB;
    public LineRenderer lineRenderer;
    private Pathfinding pathfinding;

    [Header("UI Elemanları")]
    public TextMeshProUGUI distanceText;

    private Dictionary<Node, Color> originalColors = new Dictionary<Node, Color>();
    private bool colorsCached = false;

    void Start()
    {
        pathfinding = GetComponent<Pathfinding>();
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = 0.15f;
            lineRenderer.endWidth = 0.15f;
            lineRenderer.positionCount = 0;
        }
        if (distanceText != null) distanceText.text = "A Noktasını Seçiniz...";
    }

    void CacheOriginalColors()
    {
        if (colorsCached) return;
        originalColors.Clear();
        Node[] allNodes = FindObjectsByType<Node>(FindObjectsSortMode.None);
        foreach (Node n in allNodes)
        {
            Renderer r = n.GetComponent<Renderer>();
            if (r != null) originalColors[n] = r.material.color;
        }
        colorsCached = true;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Node clickedNode = hit.collider.GetComponent<Node>();
                if (clickedNode != null)
                {
                    CacheOriginalColors();
                    SetNodes(clickedNode);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space)) ResetPath();
    }

    void SetNodes(Node node)
    {
        if (nodeA == null)
        {
            nodeA = node;
            node.GetComponent<Renderer>().material.color = Color.green;
            if (distanceText != null) distanceText.text = "B Noktasını Seçiniz...";
        }
        else if (nodeB == null && node != nodeA)
        {
            nodeB = node;
            node.GetComponent<Renderer>().material.color = Color.red;
            DrawPath();
        }
    }

    void DrawPath()
    {
        List<Node> path = pathfinding.FindPath(nodeA, nodeB);

        if (path != null && path.Count > 0)
        {
            float totalDist = 0f;
            lineRenderer.positionCount = path.Count;

            for (int i = 0; i < path.Count; i++)
            {
                lineRenderer.SetPosition(i, path[i].transform.position + Vector3.up * 0.15f);
                if (i > 0)
                    totalDist += Vector3.Distance(
                        path[i].transform.position,
                        path[i - 1].transform.position);

                if (path[i] != nodeA && path[i] != nodeB)
                    path[i].GetComponent<Renderer>().material.color = new Color(0.5f, 0.8f, 1f);
            }

            float directDist = Vector3.Distance(nodeA.transform.position, nodeB.transform.position);
            float inefficiency = ((totalDist / directDist) - 1f) * 100f;

            if (distanceText != null)
            {
                distanceText.text =
                    $"<size=120%><b>ALTIGЕN LOJİSTİK VERİSİ</b></size>\n" +
                    $"<color=#00FF00>Rota Uzunluğu:</color> {totalDist:F2}m\n" +
                    $"<color=#FFFFFF>Doğrusal Hat:</color> {directDist:F2}m\n" +
                    $"<color=#00FFFF>Dolambaç Oranı:</color> <color=red>%{inefficiency:F1}</color>\n" +
                    $"<size=80%>Durum: Rota Optimize Edildi</size>";
            }

            // Sonucu PlayerPrefs'e kaydet
            PlayerPrefs.SetFloat("Hex_RotaUzunlugu", totalDist);
            PlayerPrefs.SetFloat("Hex_DogruMesafe", directDist);
            PlayerPrefs.SetFloat("Hex_Dolambac", inefficiency);
            PlayerPrefs.SetInt("Hex_Olculdu", 1);
            PlayerPrefs.Save();

            GameObject[] cars = Resources.LoadAll<GameObject>("Cars");
            if (cars.Length > 0)
            {
                GameObject car = Instantiate(
                    cars[Random.Range(0, cars.Length)],
                    nodeA.transform.position,
                    Quaternion.identity);
                car.transform.localScale = Vector3.one * 0.2f;
                car.AddComponent<Navigator>().StartJourney(path, "TRANSİT");
            }
        }
        else
        {
            if (distanceText != null)
                distanceText.text = "<color=red>Rota Bulunamadı!</color>";
        }
    }

    public void ResetPath()
    {
        Node[] nodes = FindObjectsByType<Node>(FindObjectsSortMode.None);
        foreach (Node n in nodes)
        {
            if (originalColors.TryGetValue(n, out Color original))
                n.GetComponent<Renderer>().material.color = original;
        }

        if (lineRenderer != null) lineRenderer.positionCount = 0;

        foreach (var car in FindObjectsByType<Navigator>(FindObjectsSortMode.None))
            Destroy(car.gameObject);

        nodeA = null;
        nodeB = null;
        colorsCached = false;
        if (distanceText != null) distanceText.text = "A Noktasını Seçiniz...";
    }
}