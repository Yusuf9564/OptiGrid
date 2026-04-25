using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class InputManager_Triangle : MonoBehaviour
{
    [Header("Durak Noktaları")]
    public Node nodeA;
    public Node nodeB;

    [Header("Bileşenler")]
    public LineRenderer lineRenderer;
    public TextMeshProUGUI distanceText;
    private Pathfinding_Triangle pathfinding;

    private Dictionary<Node, Color> originalColors = new Dictionary<Node, Color>();
    private bool colorsCached = false;

    void Start()
    {
        pathfinding = GetComponent<Pathfinding_Triangle>();
        if (distanceText != null) distanceText.text = "A Noktasını Seçiniz...";
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = 0.15f;
            lineRenderer.endWidth = 0.15f;
            lineRenderer.positionCount = 0;
        }
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
                lineRenderer.SetPosition(i, path[i].transform.position + Vector3.up * 0.2f);
                if (i > 0)
                    totalDist += Vector3.Distance(
                        path[i].transform.position,
                        path[i - 1].transform.position);

                if (path[i] != nodeA && path[i] != nodeB)
                    path[i].GetComponent<Renderer>().material.color = new Color(1f, 0.6f, 0f);
            }

            float directDist = Vector3.Distance(nodeA.transform.position, nodeB.transform.position);
            float inefficiency = ((totalDist / directDist) - 1f) * 100f;

            if (distanceText != null)
            {
                distanceText.text =
                    $"<size=120%><b>ÜÇGEN TRANSİT ANALİZİ</b></size>\n" +
                    $"<color=#FF7F00>Sokak Mesafesi:</color> {totalDist:F2}m\n" +
                    $"<color=#AAAAAA>Kuş Uçuşu (A-B):</color> {directDist:F2}m\n" +
                    $"<color=yellow>Verimlilik Kaybı:</color> <color=red>%{inefficiency:F1}</color>";
            }
            PlayerPrefs.SetFloat("Tri_RotaUzunlugu", totalDist);
            PlayerPrefs.SetFloat("Tri_DogruMesafe", directDist);
            PlayerPrefs.SetFloat("Tri_Dolambac", inefficiency);
            PlayerPrefs.SetInt("Tri_Olculdu", 1);
            PlayerPrefs.Save();

            GameObject[] cars = Resources.LoadAll<GameObject>("Cars");
            if (cars.Length > 0)
            {
                GameObject car = Instantiate(
                    cars[Random.Range(0, cars.Length)],
                    nodeA.transform.position,
                    Quaternion.identity);
                car.transform.localScale = Vector3.one * 0.25f;
                car.AddComponent<Navigator>().StartJourney(path, "ÜÇGEN");
            }
        }
        else
        {
            if (distanceText != null)
                distanceText.text = "<color=red>Bağlantı Kesik!</color>";
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
        foreach (var nav in FindObjectsByType<Navigator>(FindObjectsSortMode.None))
            Destroy(nav.gameObject);

        nodeA = null;
        nodeB = null;
        colorsCached = false;
        if (distanceText != null) distanceText.text = "A Noktasını Seçiniz...";
    }
}