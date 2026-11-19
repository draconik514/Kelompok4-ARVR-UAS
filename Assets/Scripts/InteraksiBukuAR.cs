using UnityEngine;

namespace Kelompok4.AR
{
    /// <summary>
    /// InteraksiBukuAR:
    /// Mengendalikan interaksi 3D model Buku Pariwisata menggunakan HAND TRACKING (kamera optik di udara).
    /// Menggerakkan tangan ke kiri/kanan memutar buku secara horizontal.
    /// Menggerakkan tangan ke atas/bawah memutar dan membalikkan buku secara vertikal (flip).
    /// Gestur CUBITAN (PINCH) melipatgandakan kecepatan putaran untuk membalikkan objek dengan cepat.
    /// Skala objek dijaga 100% stabil (tidak membesar-kecil sendiri akibat noise kamera).
    /// </summary>
    public class InteraksiBukuAR : MonoBehaviour
    {
        [Header("Pengaturan Hand Tracking (Interaksi di Udara)")]
        [Tooltip("Sensitivitas putaran objek saat tangan bergerak di udara")]
        public float kecepatanPutarSentuh = 220.0f; // Nama field dipertahankan demi kompatibilitas serialisasi Unity
        public float kecepatanPutarMouse = 5.0f;
        public float dampingKecepatan = 5.0f;
        public bool autoRotate = false;
        public float kecepatanAutoRotate = 20.0f;

        [System.NonSerialized]
        public float pengaliKecepatanPinch = 2.2f;

        private Vector3 initialLocalPos;
        private Quaternion initialLocalRot;
        private Vector3 initialLocalScale;

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

        private void Awake()
        {
            mainCamera = Camera.main;
            initialLocalPos = transform.localPosition;
            initialLocalRot = transform.localRotation;
            initialLocalScale = transform.localScale;

            // Fallback pengaman jika skala lokal terdeteksi 0 atau tidak wajar
            if (initialLocalScale.x < 0.01f || initialLocalScale.y < 0.001f || initialLocalScale.z < 0.01f)
            {
                initialLocalScale = new Vector3(0.2f, 0.03f, 0.266f);
                transform.localScale = initialLocalScale;
            }
            if (initialLocalPos == Vector3.zero)
            {
                initialLocalPos = new Vector3(0f, 0.04f, 0f);
                transform.localPosition = initialLocalPos;
            }

            // Pastikan kecepatan putaran memiliki nilai optimal untuk koordinat normalisasi hand tracking
            if (kecepatanPutarSentuh < 50f)
            {
                kecepatanPutarSentuh = 320.0f;
            }
            pengaliKecepatanPinch = 2.2f;
        }

        private void OnEnable()
        {
            lastInteractionTime = Time.time;
            angularVelocity = Vector2.zero;

            // Pastikan skala selalu terkunci stabil pada ukuran normal awal
            if (initialLocalScale.x > 0.001f)
            {
                transform.localScale = initialLocalScale;
            }
        }

        private void Update()
        {
            bool interactedThisFrame = false;
            PelacakTanganKameraAR pelacak = PelacakTanganKameraAR.Instance;

            // =========================================================================
            // 1. KENDALI HAND TRACKING DI UDARA: MEMUTAR & MEMBALIKKAN OBJEK
            // =========================================================================
            if (pelacak != null && pelacak.isHandDetected)
            {
                Vector2 delta = pelacak.handDelta;

                // Tangan bergerak di udara -> Langsung putar/balikkan objek dengan responsivitas tinggi!
                if (delta.sqrMagnitude > 0.00000001f)
                {
                    // Faktor pengali: saat mencubit (pinch), putaran lebih cepat & responsif untuk membalikkan objek
                    float speed = pelacak.isPinching ? (kecepatanPutarSentuh * pengaliKecepatanPinch) : kecepatanPutarSentuh;

                    // Rotasi diselaraskan dengan sudut pandang kamera (View Space) agar gerakan tangan selalu presisi
                    Vector3 axisYaw = GetCameraUpAxis();
                    Vector3 axisPitch = GetCameraRightAxis();

                    // 1. Gerakan Horizontal (Kiri/Kanan): Memutar buku sejajar sumbu vertikal layar kamera
                    float rotY = -delta.x * speed;

                    // 2. Gerakan Vertikal (Atas/Bawah): Memutar / Membalikkan buku sejajar sumbu horizontal layar kamera
                    float rotX = delta.y * speed;

                    transform.Rotate(axisYaw, rotY, Space.World);
                    transform.Rotate(axisPitch, rotX, Space.World);

                    // Simpan momentum putaran
                    angularVelocity = new Vector2(rotX, rotY) / Mathf.Max(Time.deltaTime, 0.001f) * 0.045f;
                    interactedThisFrame = true;
                }

                // CATATAN PENTING: Fitur scaling dari noise kamera telah dinonaktifkan total
                // agar objek tetap stabil dan tidak membesar/mengecil sendiri!
            }

#if UNITY_EDITOR
            // Pengujian Tambahan di Editor: Drag Mouse Kiri
            if (pelacak == null || !pelacak.isHandDetected)
            {
                if (Input.GetMouseButton(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
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
            }
#endif

            // =========================================================================
            // 2. INERSIA & MOMENTUM (Redaman Halus Saat Tangan Berhenti / Diangkat)
            // =========================================================================
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

                // 3. Auto-Rotate Berputar Halus jika fitur diaktifkan
                if (!isInteracting && autoRotate && (Time.time - lastInteractionTime > 1.0f))
                {
                    Vector3 axisYaw = GetCameraUpAxis();
                    transform.Rotate(axisYaw, kecepatanAutoRotate * Time.deltaTime, Space.World);
                }
            }
        }

        /// <summary>
        /// Membalikkan objek 180 derajat secara instan
        /// </summary>
        public void BalikkanObjek180()
        {
            transform.Rotate(Vector3.up, 180f, Space.Self);
            angularVelocity = Vector2.zero;
        }

        public void ResetTransform()
        {
            if (initialLocalScale.x < 0.01f)
            {
                initialLocalPos = new Vector3(0f, 0.04f, 0f);
                initialLocalRot = Quaternion.identity;
                initialLocalScale = new Vector3(0.2f, 0.03f, 0.266f);
            }
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
