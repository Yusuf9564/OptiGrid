using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("WASD Hareket")]
    public float moveSpeed = 30f;
    public float fastMultiplier = 2.5f;

    [Header("Orbit (Orta Tık)")]
    public float orbitSensitivity = 1.5f;
    private Vector3 orbitCenter;
    private bool isOrbiting = false;
    private float orbitYaw = 0f;  // Yatay açı (Y ekseni)
    private float orbitPitch = 45f; // Dikey açı (X ekseni) — başlangıç

    [Header("Zoom (Scroll)")]
    public float zoomSpeed = 100f;
    public float minZoom = 5f;
    public float maxZoom = 150f;
    private float currentZoomDistance = 25f;

    [Header("K Kilidi — Tepe Görünümü")]
    public Vector3 lockedPosition = new Vector3(0f, 250f, 0f);
    public Vector3 lockedRotation = new Vector3(90f, 0f, 0f);
    public float lockTransitionSpeed = 5f;

    [Header("C Kilidi — Araç Takip")]
    public float followHeightRatio =0.7f;
    public float followSmoothness = 5f;
    private Transform targetVehicle;

    private enum CameraMode { Free, Locked, Follow }
    private CameraMode currentMode = CameraMode.Free;

    private Transform focusTarget;
    private bool isFocusing = false;

    // Pitch sınırları
    private const float PITCH_MIN = 10f;
    private const float PITCH_MAX = 85f;

    void Start()
    {
        // Başlangıç açılarını kameranın mevcut euler'ından al
        // eulerAngles.y 0-360, eulerAngles.x 0-360 olabilir — normalize et
        orbitYaw = transform.eulerAngles.y;
        float rawX = transform.eulerAngles.x;
        // Unity'de yukarı bakış 270-360 arasında gelir, bunu 0'a normalize et
        orbitPitch = rawX > 180f ? rawX - 360f : rawX;
        orbitPitch = Mathf.Clamp(orbitPitch, PITCH_MIN, PITCH_MAX);

        currentZoomDistance = Mathf.Clamp(
            Vector3.Distance(transform.position, Vector3.zero),
            minZoom, maxZoom);
    }

    void Update()
    {
        // Focus modu aktifse diğer kontrolleri devre dışı bırak
        if (isFocusing && focusTarget != null)
        {
            HandleFocusUpdate();
            return;
        }

        HandleModeSwitch();
        HandleZoom();

        switch (currentMode)
        {
            case CameraMode.Locked: HandleLockedMode(); break;
            case CameraMode.Follow: HandleFollowMode(); break;
            case CameraMode.Free:
                HandleOrbit();
                HandleWASD();
                break;
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.001f) return;

        if (currentMode == CameraMode.Follow)
        {
            currentZoomDistance = Mathf.Clamp(
                currentZoomDistance - scroll * zoomSpeed,
                minZoom, maxZoom);
        }
        else if (currentMode == CameraMode.Free && !isOrbiting)
        {
            Vector3 next = transform.position + transform.forward * scroll * zoomSpeed;
            if (next.y > 2f) transform.position = next;
        }
    }

    void HandleModeSwitch()
    {
        if (Input.GetKeyDown(KeyCode.K))
            currentMode = (currentMode == CameraMode.Locked)
                ? CameraMode.Free : CameraMode.Locked;

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (currentMode == CameraMode.Follow)
            {
                currentMode = CameraMode.Free;
            }
            else
            {
                Navigator nav = Object.FindFirstObjectByType<Navigator>();
                if (nav != null)
                {
                    targetVehicle = nav.transform;
                    currentMode = CameraMode.Follow;
                    currentZoomDistance = Mathf.Clamp(
                        Vector3.Distance(transform.position, targetVehicle.position),
                        minZoom, maxZoom);
                }
            }
        }
    }

    void HandleLockedMode()
    {
        transform.position = Vector3.Lerp(
            transform.position, lockedPosition,
            Time.deltaTime * lockTransitionSpeed);
        transform.rotation = Quaternion.Lerp(
            transform.rotation, Quaternion.Euler(lockedRotation),
            Time.deltaTime * lockTransitionSpeed);
    }

    void HandleFollowMode()
    {
        if (targetVehicle == null) { currentMode = CameraMode.Free; return; }

        float verticalOffset = currentZoomDistance * followHeightRatio;
        float horizontalOffset = currentZoomDistance * (1.5f - followHeightRatio);

        Vector3 desired = targetVehicle.position
            + new Vector3(0f, verticalOffset, -horizontalOffset);

        transform.position = Vector3.Lerp(
            transform.position, desired,
            Time.deltaTime * followSmoothness);

        Quaternion lookRot = Quaternion.LookRotation(
            targetVehicle.position - transform.position);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, lookRot,
            Time.deltaTime * followSmoothness);
    }

    void HandleOrbit()
    {
        bool startWithMiddle = Input.GetMouseButtonDown(2);
        bool startWithNLeft = Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.N);

        if (startWithMiddle || startWithNLeft)
        {
            isOrbiting = true;
            Ray ray = new Ray(transform.position, transform.forward);

            orbitCenter = Physics.Raycast(ray, out RaycastHit hit)
                ? hit.point
                : transform.position + transform.forward * 50f;

            currentZoomDistance = Mathf.Clamp(
                Vector3.Distance(transform.position, orbitCenter),
                minZoom, maxZoom);
        }

        
        if (Input.GetMouseButtonUp(2) || Input.GetMouseButtonUp(0))
        {
            isOrbiting = false;
        }

        
        if (!isOrbiting) return;

        orbitYaw += Input.GetAxis("Mouse X") * orbitSensitivity * 5f;
        orbitPitch -= Input.GetAxis("Mouse Y") * orbitSensitivity * 5f;
        orbitPitch = Mathf.Clamp(orbitPitch, PITCH_MIN, PITCH_MAX);

        Quaternion rot = Quaternion.Euler(orbitPitch, orbitYaw, 0f);
        transform.position = orbitCenter + rot * new Vector3(0f, 0f, -currentZoomDistance);
        transform.rotation = rot;
    }

    void HandleWASD()
    {
        if (isOrbiting) return;

        float spd = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? fastMultiplier : 1f);
        Vector3 fwd = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) move += fwd;
        if (Input.GetKey(KeyCode.S)) move -= fwd;
        if (Input.GetKey(KeyCode.A)) move -= right;
        if (Input.GetKey(KeyCode.D)) move += right;

        if (move.magnitude > 0.01f)
        {
            transform.position += move.normalized * spd * Time.deltaTime;
            // WASD sonrası orbit açılarını güncelle — geçişte sıçrama olmaz
            orbitYaw = transform.eulerAngles.y;
            float rawX = transform.eulerAngles.x;
            orbitPitch = rawX > 180f ? rawX - 360f : rawX;
            orbitPitch = Mathf.Clamp(orbitPitch, PITCH_MIN, PITCH_MAX);
        }
    }

    void HandleFocusUpdate()
    {
        Vector3 targetPos = focusTarget.position + new Vector3(0f, 8f, -10f);
        transform.position = Vector3.Lerp(
            transform.position, targetPos, Time.deltaTime * 4f);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(focusTarget.position - transform.position),
            Time.deltaTime * 4f);
    }

    public void FocusOnTarget(Transform newTarget)
    {
        focusTarget = newTarget;
        isFocusing = true;
        Invoke(nameof(StopFocusing), 3.5f);
    }

    void StopFocusing() => isFocusing = false;
}