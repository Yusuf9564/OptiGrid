using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("WASD Hareket")]
    public float moveSpeed = 30f;
    public float fastMultiplier = 2.5f;

    [Header("Orbit (Orta Tık)")]
    public float orbitSensitivity = 0.5f;
    public Transform orbitTarget;

    [Header("Zoom (Scroll)")]
    public float zoomSpeed = 50f; // Biraz hızlandırdım hissiyat için
    public float minZoom = 5f;
    public float maxZoom = 300f;

    [Header("K Kilidi (Üst Bakış)")]
    public Vector3 lockedPosition = new Vector3(0f, 250f, 0f);
    public Vector3 lockedRotation = new Vector3(90f, 0f, 0f);
    public float lockTransitionSpeed = 5f;

    [Header("C Kilidi (Araç Takip)")]
    public float followHeight = 15f;
    public float followDistance = 20f;
    public float followSmoothness = 5f;
    private Transform targetVehicle;

    private enum CameraMode { Free, Locked, Follow }
    private CameraMode currentMode = CameraMode.Free;

    private float orbitDistance;
    private Vector2 orbitAngles;
    private Vector3 orbitCenter;
    private bool isOrbiting = false;

    void Start()
    {
        orbitAngles.x = transform.eulerAngles.y;
        orbitAngles.y = transform.eulerAngles.x;
        orbitDistance = Vector3.Distance(transform.position, Vector3.zero);
    }

    void Update()
    {
        HandleModeSwitch();

        switch (currentMode)
        {
            case CameraMode.Locked:
                HandleLockedMode();
                break;
            case CameraMode.Follow:
                HandleFollowMode();
                break;
            case CameraMode.Free:
                HandleOrbit();
                HandleWASD();
                HandleZoom();
                break;
        }
    }

    void HandleModeSwitch()
    {
        // K - Üst Bakış Kilidi
        if (Input.GetKeyDown(KeyCode.K))
        {
            currentMode = (currentMode == CameraMode.Locked) ? CameraMode.Free : CameraMode.Locked;
            targetVehicle = null;
        }

        // C - Araç Takip Kilidi
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (currentMode == CameraMode.Follow)
            {
                currentMode = CameraMode.Free;
                targetVehicle = null;
            }
            else
            {
                // Sahnedeki Navigator (araba) bileşenine sahip en son objeyi bul
                Navigator nav = FindFirstObjectByType<Navigator>();
                if (nav != null)
                {
                    targetVehicle = nav.transform;
                    currentMode = CameraMode.Follow;
                }
                else
                {
                    Debug.LogWarning("Takip edilecek araç bulunamadı!");
                }
            }
        }
    }

    void HandleLockedMode()
    {
        transform.position = Vector3.Lerp(transform.position, lockedPosition, Time.deltaTime * lockTransitionSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(lockedRotation), Time.deltaTime * lockTransitionSpeed);
    }

    void HandleFollowMode()
    {
        if (targetVehicle == null)
        {
            currentMode = CameraMode.Free;
            return;
        }

        // Aracın arkasında ve üstünde bir pozisyon hesapla
        // (Aracın rotasyonundan bağımsız, sabit bir açıyla takip eder - RTS stili)
        Vector3 desiredPos = targetVehicle.position + new Vector3(0, followHeight, -followDistance);

        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * followSmoothness);

        // Araca doğru yumuşakça bak
        Quaternion lookRot = Quaternion.LookRotation(targetVehicle.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * followSmoothness);

        // Takip modundayken scroll ile mesafeyi ayarla
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        followHeight = Mathf.Clamp(followHeight - scroll * zoomSpeed, 5f, 50f);
        followDistance = Mathf.Clamp(followDistance - scroll * zoomSpeed, 5f, 50f);
    }

    // --- Mevcut Kontroller (Free Mode) ---

    void HandleOrbit()
    {
        if (Input.GetMouseButtonDown(2))
        {
            isOrbiting = true;
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit)) orbitCenter = hit.point;
            else orbitCenter = transform.position + transform.forward * 50f;
            orbitDistance = Vector3.Distance(transform.position, orbitCenter);
        }

        if (Input.GetMouseButtonUp(2)) isOrbiting = false;

        if (isOrbiting)
        {
            orbitAngles.x += Input.GetAxis("Mouse X") * orbitSensitivity * 100f * Time.deltaTime;
            orbitAngles.y -= Input.GetAxis("Mouse Y") * orbitSensitivity * 100f * Time.deltaTime;
            orbitAngles.y = Mathf.Clamp(orbitAngles.y, 5f, 89f);

            Quaternion rotation = Quaternion.Euler(orbitAngles.y, orbitAngles.x, 0f);
            transform.position = orbitCenter + (rotation * new Vector3(0f, 0f, -orbitDistance));
            transform.rotation = rotation;
        }
    }

    void HandleWASD()
    {
        if (isOrbiting) return;

        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? fastMultiplier : 1f);
        float dt = Time.deltaTime;

        // Yatay Hareket (Zemin düzleminde)
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) move += forward;
        if (Input.GetKey(KeyCode.S)) move -= forward;
        if (Input.GetKey(KeyCode.A)) move -= right;
        if (Input.GetKey(KeyCode.D)) move += right;

        // --- Q ve E GERİ GELDİ ---
        if (Input.GetKey(KeyCode.E)) move += Vector3.up;
        if (Input.GetKey(KeyCode.Q)) move -= Vector3.up;
        // ------------------------

        // Hareket varsa uygula
        if (move != Vector3.zero)
        {
            transform.position += move.normalized * speed * dt;
        }
    }

    void HandleZoom()
    {
        if (isOrbiting) return;
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.position += transform.forward * scroll * zoomSpeed;
    }
}