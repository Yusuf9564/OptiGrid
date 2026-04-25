using UnityEngine;
using System.Collections.Generic;

public class Navigator : MonoBehaviour
{
    private List<Node> path;
    private int targetIndex = 0;
    public float speed = 8f;
    private string systemName;

    private float totalCO2 = 0f;
    private Quaternion lastRotation;
    private Vector3 lastPosition;
    private bool isCalculating = false;

    public void StartJourney(List<Node> newPath, string system)
    {
        path = newPath;
        systemName = system;
        targetIndex = 0;
        totalCO2 = 0f;
        lastRotation = transform.rotation;
        lastPosition = transform.position;
        isCalculating = true;
    }

    void Update()
    {
        if (path == null || targetIndex >= path.Count) return;

        Vector3 targetPos = path[targetIndex].transform.position;
        targetPos.y = transform.position.y;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        Vector3 dir = targetPos - transform.position;
        if (dir.magnitude > 0.1f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);

        if (isCalculating)
        {
            float dist = Vector3.Distance(transform.position, lastPosition);
            float angleDiff = Quaternion.Angle(transform.rotation, lastRotation);
            totalCO2 += (dist * 0.05f) + (angleDiff * 0.02f);
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            targetIndex++;
            if (targetIndex >= path.Count) Complete();
        }
    }

    void Complete()
    {
        isCalculating = false;

        // systemName'e göre key belirle
        string keyName;
        if (systemName == "TRANSİT") keyName = "Hex";
        else if (systemName == "ÜÇGEN") keyName = "Tri";
        else if (systemName == "KARE") keyName = "Square";
        else keyName = systemName;

        PlayerPrefs.SetFloat(keyName + "_RealCO2", totalCO2);
        PlayerPrefs.Save();

        Debug.Log($"{keyName} CO2 Kaydedildi: {totalCO2:F2}kg");
        path = null;
    }
}