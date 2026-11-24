using UnityEngine;
using UnityEngine.EventSystems;

namespace Kelompok4.AR
{
    /// <summary>
    /// InteraksiBukuAR:
    /// Mengendalikan interaksi 3D Digital Twin (Buku Pariwisata, Buku Campus, Kalkulator, Penghapus, Pulpen)
    /// menggunakan HAND TRACKING MediaPipe di udara, dengan fallback sentuhan layar & mouse.
    /// </summary>
    public class InteraksiBukuAR : MonoBehaviour
    {
        [Header("Pengaturan Hand Tracking (Interaksi di Udara)")]
        [Tooltip("Sensitivitas putaran objek saat tangan bergerak di udara")]
        public float kecepatanPutarSentuh = 240.0f;
        public float kecepatanPutarMouse = 5.0f;
        public float dampingKecepatan = 5.0f;
        public bool autoRotate = false;
        public float kecepatanAutoRotate = 25.0f;

        [System.NonSerialized]
        public float pengaliKecepatanPinch = 2.2f;

        private Vector3 initialLocalPos;
        private Quaternion initialLocalRot;
        private Vector3 initialLocalScale;
        private bool initialCaptured = false;

        private Vector2 angularVelocity;
        private float lastInteractionTime = 0f;
        private bool isInteracting = false;
        private Camera mainCamera;

        private Vector3 GetCameraUpAxis()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            return mainCamera != null ? mainCamera.transform.up : Vector3.up;
        }

        private Vector3 GetCameraRightAxis()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            return mainCamera != null ? mainCamera.transform.right : Vector3.right;
        }

        public void CaptureInitialTransform()
        {
            initialLocalPos = transform.localPosition;
            initialLocalRot = transform.localRotation;
            initialLocalScale = transform.localScale;
            if (initialLocalScale.sqrMagnitude < 0.00001f)
            {
                initialLocalScale = Vector3.one;
                transform.localScale = initialLocalScale;
            }
            initialCaptured = true;
        }

        private void Awake()
        {
            mainCamera = Camera.main;
            if (!initialCaptured)
            {
                CaptureInitialTransform();
            }

            if (kecepatanPutarSentuh < 50f)
            {
                kecepatanPutarSentuh = 280.0f;
            }
            pengaliKecepatanPinch = 2.2f;
        }

        private void OnEnable()
        {
            if (!initialCaptured)
            {
                CaptureInitialTransform();
            }
            lastInteractionTime = Time.time;
            angularVelocity = Vector2.zero;
            transform.localScale = initialLocalScale;
        }

        public bool IsCurrentlyVisible()
        {
            if (!gameObject.activeInHierarchy) return false;
            Renderer[] renderers = GetComponentsInChildren<Renderer>(false);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].enabled)
                {
                    return true;
                }
            }
            return false;
        }

        private void Update()
        {
            if (!IsCurrentlyVisible()) return;

            bool interactedThisFrame = false;
            PelacakTanganKameraAR pelacak = PelacakTanganKameraAR.Instance;

            // 1. KENDALI UTAMA: HAND TRACKING MEDIAPIPE DI UDARA
            if (pelacak != null && pelacak.isHandDetected)
            {
                Vector2 delta = pelacak.handDelta;
                if (delta.sqrMagnitude > 0.00000001f)
                {
                    float speed = pelacak.isPinching ? (kecepatanPutarSentuh * pengaliKecepatanPinch) : kecepatanPutarSentuh;

                    Vector3 axisYaw = GetCameraUpAxis();
                    Vector3 axisPitch = GetCameraRightAxis();

                    float rotY = -delta.x * speed;
                    float rotX = delta.y * speed;

                    transform.Rotate(axisYaw, rotY, Space.World);
                    transform.Rotate(axisPitch, rotX, Space.World);

                    angularVelocity = new Vector2(rotX, rotY) / Mathf.Max(Time.deltaTime, 0.001f) * 0.045f;
                    interactedThisFrame = true;
                }
            }
            else
            {
                // 2. FALLBACK SENTUHAN LAYAR ANDROID ATAU MOUSE DI EDITOR
                bool pointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
                if (!pointerOverUI && Input.touchCount == 1)
                {
                    Touch touch = Input.GetTouch(0);
                    bool touchOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId);
                    if (!touchOverUI && touch.phase == TouchPhase.Moved)
                    {
                        Vector3 axisYaw = GetCameraUpAxis();
                        Vector3 axisPitch = GetCameraRightAxis();

                        float rotY = -touch.deltaPosition.x * 0.35f;
                        float rotX = touch.deltaPosition.y * 0.35f;

                        transform.Rotate(axisYaw, rotY, Space.World);
                        transform.Rotate(axisPitch, rotX, Space.World);

                        angularVelocity = new Vector2(rotX, rotY) * 6f;
                        interactedThisFrame = true;
                    }
                }
#if UNITY_EDITOR
                else if (!pointerOverUI && Input.GetMouseButton(0))
                {
                    float mouseX = Input.GetAxis("Mouse X");
                    float mouseY = Input.GetAxis("Mouse Y");
                    if (Mathf.Abs(mouseX) > 0.001f || Mathf.Abs(mouseY) > 0.001f)
                    {
                        Vector3 axisYaw = GetCameraUpAxis();
                        Vector3 axisPitch = GetCameraRightAxis();

                        float rotY = -mouseX * kecepatanPutarMouse * 2.5f;
                        float rotX = mouseY * kecepatanPutarMouse * 2.5f;

                        transform.Rotate(axisYaw, rotY, Space.World);
                        transform.Rotate(axisPitch, rotX, Space.World);

                        angularVelocity = new Vector2(rotX, rotY) * 8f;
                        interactedThisFrame = true;
                    }
                }
#endif
            }

            // 3. INERSIA & AUTO-ROTATE
            if (interactedThisFrame)
            {
                isInteracting = true;
                lastInteractionTime = Time.time;
            }
            else
            {
                if (angularVelocity.sqrMagnitude > 0.01f)
                {
                    Vector3 axisYaw = GetCameraUpAxis();
                    Vector3 axisPitch = GetCameraRightAxis();

                    transform.Rotate(axisYaw, angularVelocity.y * Time.deltaTime, Space.World);
                    transform.Rotate(axisPitch, angularVelocity.x * Time.deltaTime, Space.World);
                    angularVelocity = Vector2.Lerp(angularVelocity, Vector2.zero, dampingKecepatan * Time.deltaTime);
                }
                else
                {
                    isInteracting = false;
                }

                if (!isInteracting && autoRotate && (Time.time - lastInteractionTime > 0.8f))
                {
                    Vector3 axisYaw = GetCameraUpAxis();
                    transform.Rotate(axisYaw, kecepatanAutoRotate * Time.deltaTime, Space.World);
                }
            }
        }

        public void BalikkanObjek180()
        {
            transform.Rotate(Vector3.up, 180f, Space.Self);
            angularVelocity = Vector2.zero;
        }

        public void ResetTransform()
        {
            if (!initialCaptured) CaptureInitialTransform();
            transform.localPosition = initialLocalPos;
            transform.localRotation = initialLocalRot;
            transform.localScale = initialLocalScale;
            angularVelocity = Vector2.zero;
            lastInteractionTime = Time.time;
        }

        public void ToggleAutoRotate()
        {
            autoRotate = !autoRotate;
        }
    }
}
