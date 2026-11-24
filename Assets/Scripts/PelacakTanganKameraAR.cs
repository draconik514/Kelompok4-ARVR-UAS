using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using Vuforia;
using Mediapipe;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Tasks.Vision.Core;
using Mediapipe.Tasks.Core;

namespace Kelompok4.AR
{
    /// <summary>
    /// OneEuroFilter2D:
    /// Algoritma filter adaptif industri (digunakan oleh MediaPipe & VR/AR)
    /// untuk menghasilkan pelacakan 21 landmark tangan yang sangat halus (buttery-smooth),
    /// bebas gemetar/jitter saat tangan diam, dan responsif tanpa lag saat bergerak cepat.
    /// </summary>
    public class OneEuroFilter2D
    {
        private float minCutoff;
        private float beta;
        private float dCutoff;

        private Vector2 xPrev;
        private Vector2 dxPrev;
        private float tPrev;
        private bool initialized;

        public OneEuroFilter2D(float minCutoff = 1.2f, float beta = 0.02f, float dCutoff = 1.0f)
        {
            this.minCutoff = minCutoff;
            this.beta = beta;
            this.dCutoff = dCutoff;
            this.initialized = false;
        }

        public Vector2 Filter(Vector2 x, float t)
        {
            if (!initialized)
            {
                initialized = true;
                xPrev = x;
                dxPrev = Vector2.zero;
                tPrev = t;
                return x;
            }

            float dt = t - tPrev;
            if (dt <= 0.0001f) return xPrev;
            tPrev = t;

            Vector2 dx = (x - xPrev) / dt;
            float aD = Alpha(dt, dCutoff);
            Vector2 dxHat = Vector2.Lerp(dxPrev, dx, aD);
            dxPrev = dxHat;

            float cutoffX = minCutoff + beta * Mathf.Abs(dxHat.x);
            float cutoffY = minCutoff + beta * Mathf.Abs(dxHat.y);
            float aX = Alpha(dt, cutoffX);
            float aY = Alpha(dt, cutoffY);

            Vector2 xHat = new Vector2(
                Mathf.Lerp(xPrev.x, x.x, aX),
                Mathf.Lerp(xPrev.y, x.y, aY)
            );
            xPrev = xHat;
            return xHat;
        }

        private float Alpha(float dt, float cutoff)
        {
            float tau = 1.0f / (2f * Mathf.PI * cutoff);
            return 1.0f / (1.0f + tau / dt);
        }

        public void Reset()
        {
            initialized = false;
        }
    }

    /// <summary>
    /// PelacakTanganKameraAR:
    /// Integrasi Resmi Google MediaPipe Hand Landmarker (21 Landmark 3D) dengan Vuforia Engine.
    /// Menghubungkan frame buffer kamera RGB888 secara zero-copy ke model AI TFLite MediaPipe.
    /// Melacak 21 titik sendi dan tulang tangan nyata dengan kestabilan OneEuroFilter,
    /// mendeteksi gestur mencubit (pinch) secara proporsional, serta mengendalikan interaksi objek 3D di udara.
    /// </summary>
    public class PelacakTanganKameraAR : MonoBehaviour
    {
        public static PelacakTanganKameraAR Instance { get; private set; }

        [Header("Pengaturan MediaPipe HandLandmarker")]
        [Tooltip("Confidence minimum untuk deteksi tangan")]
        [Range(0.1f, 1.0f)]
        public float minDetectionConfidence = 0.5f;
        [Tooltip("Confidence minimum untuk keberadaan tangan")]
        [Range(0.1f, 1.0f)]
        public float minPresenceConfidence = 0.5f;
        [Tooltip("Confidence minimum untuk pelacakan beruntun antar frame")]
        [Range(0.1f, 1.0f)]
        public float minTrackingConfidence = 0.5f;

        [Header("Pengaturan Orientasi & Sumbu")]
        [Tooltip("Rotasi input frame ke MediaPipe agar telapak tangan terdeteksi tegak (0, 90, 180, 270). Android portrait umumnya 90")]
        public int rotasiMediaPipe = 90;
        [Tooltip("Tukar sumbu X dan Y pada layar saat posisi tegak (Portrait)")]
        public bool tukarSumbuXY = true;
        [Tooltip("Balik arah sumbu X")]
        public bool balikSumbuX = false;
        [Tooltip("Balik arah sumbu Y")]
        public bool balikSumbuY = false;

        [Header("Pengaturan Gestur & Sensitivitas")]
        [Tooltip("Ambang rasio jarak jempol-telunjuk terhadap ukuran telapak untuk gestur mencubit")]
        [Range(0.15f, 0.6f)]
        public float ambangPinchRatio = 0.38f;

        [Header("Status Pelacakan Real-Time")]
        public bool isHandDetected = false;
        public bool isPinching = false;
        public Vector2 handPositionNormalized = new Vector2(0.5f, 0.5f);
        public Vector2 handScreenPosition = Vector2.zero;
        public Vector2 handDelta = Vector2.zero;
        public float handArea = 0f;

        [Header("Visual Feedback UI")]
        public KerangkaTanganUI kerangkaTanganUI;
        public RectTransform reticleHandUI;
        public UnityEngine.UI.Image reticleImage;
        public TextMeshProUGUI txtReticleStatus;

        // Komponen Inti MediaPipe
        private HandLandmarker handLandmarker = null;
        private HandLandmarkerResult handResult;
        private bool isMediaPipeReady = false;
        private bool isMediaPipeInitializing = false;
        private long lastTimestampMs = 0;

        // 21 Titik Sendi MediaPipe & Filter
        private readonly Vector2[] rawLandmarks = new Vector2[21];
        private readonly Vector2[] filteredLandmarks = new Vector2[21];
        private readonly OneEuroFilter2D[] filters = new OneEuroFilter2D[21];

        private Vector2 smoothHandPos = new Vector2(0.5f, 0.5f);
        private Vector2 previousHandPos = new Vector2(0.5f, 0.5f);
        private bool wasPinching = false;
        private float lastHandSeenTime = 0f;
        private bool isVuforiaFormatRegistered = false;

#if UNITY_EDITOR
        private Vector3 lastMousePos;
        private bool isMouseSimulating = false;
#endif

        [AOT.MonoPInvokeCallback(typeof(Mediapipe.ImageFrame.Deleter))]
        private static void NoOpDeleter(IntPtr ptr)
        {
            // Memori dikelola langsung oleh Vuforia Camera Device
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this)
            {
                Destroy(this);
                return;
            }

            // Inisialisasi 21 OneEuroFilter untuk setiap titik sendi MediaPipe
            for (int i = 0; i < 21; i++)
            {
                filters[i] = new OneEuroFilter2D(1.2f, 0.02f, 1.0f);
            }

            handResult = HandLandmarkerResult.Alloc(1);
            EnsureHUDAndReticle();
        }

        private void Start()
        {
            StartCoroutine(InisialisasiMediaPipeAsync());

            VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;

            if (reticleHandUI != null)
            {
                reticleHandUI.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (VuforiaApplication.Instance != null)
            {
                VuforiaApplication.Instance.OnVuforiaStarted -= OnVuforiaStarted;
            }
            if (VuforiaBehaviour.Instance != null && VuforiaBehaviour.Instance.World != null)
            {
                VuforiaBehaviour.Instance.World.OnStateUpdated -= OnVuforiaStateUpdated;
            }

            TutupMediaPipe();
        }

        private void TutupMediaPipe()
        {
            if (handLandmarker != null)
            {
                try
                {
                    handLandmarker.Close();
                    (handLandmarker as IDisposable)?.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[MediaPipe] Peringatan saat menutup HandLandmarker: " + ex.Message);
                }
                finally
                {
                    handLandmarker = null;
                    isMediaPipeReady = false;
                }
            }
        }

        private IEnumerator InisialisasiMediaPipeAsync()
        {
            if (isMediaPipeInitializing || isMediaPipeReady) yield break;
            isMediaPipeInitializing = true;

            byte[] modelBuffer = null;

            // 1. Coba muat langsung dari Resources (sangat cepat dan andal di Android & Editor)
            TextAsset modelAsset = Resources.Load<TextAsset>("hand_landmarker");
            if (modelAsset != null && modelAsset.bytes != null && modelAsset.bytes.Length > 0)
            {
                modelBuffer = modelAsset.bytes;
                Debug.Log("[MediaPipe] Berhasil memuat model dari Resources: " + modelBuffer.Length + " bytes");
            }

            // 2. Fallback: Muat dari StreamingAssets jika Resources belum tersedia
            if (modelBuffer == null)
            {
                string streamingPath = Path.Combine(Application.streamingAssetsPath, "hand_landmarker.bytes");
                if (streamingPath.Contains("://") || streamingPath.Contains(":///"))
                {
                    using (UnityWebRequest uwr = UnityWebRequest.Get(streamingPath))
                    {
                        yield return uwr.SendWebRequest();
                        if (uwr.result == UnityWebRequest.Result.Success)
                        {
                            modelBuffer = uwr.downloadHandler.data;
                            Debug.Log("[MediaPipe] Berhasil memuat model dari StreamingAssets via WebRequest: " + modelBuffer.Length + " bytes");
                        }
                        else
                        {
                            Debug.LogError("[MediaPipe] Gagal mengunduh StreamingAssets: " + uwr.error);
                        }
                    }
                }
                else if (File.Exists(streamingPath))
                {
                    modelBuffer = File.ReadAllBytes(streamingPath);
                    Debug.Log("[MediaPipe] Berhasil membaca model dari StreamingAssets File: " + modelBuffer.Length + " bytes");
                }
            }

            if (modelBuffer == null || modelBuffer.Length == 0)
            {
                Debug.LogError("[MediaPipe] hand_landmarker.bytes tidak dapat ditemukan di Resources maupun StreamingAssets!");
                isMediaPipeInitializing = false;
                yield break;
            }

            try
            {
                // Menggunakan CPU delegate: 100% stabil, tanpa konflik OpenGL context dengan Vuforia
                var baseOptions = new BaseOptions(
                    delegateCase: BaseOptions.Delegate.CPU,
                    modelAssetBuffer: modelBuffer
                );

                var options = new HandLandmarkerOptions(
                    baseOptions: baseOptions,
                    runningMode: RunningMode.VIDEO,
                    numHands: 1,
                    minHandDetectionConfidence: minDetectionConfidence,
                    minHandPresenceConfidence: minPresenceConfidence,
                    minTrackingConfidence: minTrackingConfidence
                );

                handLandmarker = HandLandmarker.CreateFromOptions(options);
                isMediaPipeReady = true;
                Debug.Log("[MediaPipe] HandLandmarker resmi aktif dan siap melacak tangan!");
            }
            catch (Exception ex)
            {
                Debug.LogError("[MediaPipe] Inisialisasi HandLandmarker gagal: " + ex);
                isMediaPipeReady = false;
            }
            finally
            {
                isMediaPipeInitializing = false;
            }
        }

        private void OnVuforiaStarted()
        {
            RegisterCameraFormat();
            if (VuforiaBehaviour.Instance != null && VuforiaBehaviour.Instance.World != null)
            {
                VuforiaBehaviour.Instance.World.OnStateUpdated -= OnVuforiaStateUpdated;
                VuforiaBehaviour.Instance.World.OnStateUpdated += OnVuforiaStateUpdated;
            }
        }

        private void RegisterCameraFormat()
        {
            if (VuforiaBehaviour.Instance == null || VuforiaBehaviour.Instance.CameraDevice == null) return;

            bool success = VuforiaBehaviour.Instance.CameraDevice.SetFrameFormat(PixelFormat.RGB888, true);
            isVuforiaFormatRegistered = success;
            Debug.Log("[PelacakTangan] Vuforia SetFrameFormat RGB888: " + success);
        }

        private void Update()
        {
            if (!isVuforiaFormatRegistered && Time.frameCount % 60 == 0)
            {
                RegisterCameraFormat();
            }

            // Meredam delta secara halus di antara frame kamera untuk gerakan yang mulus dan bebas getaran
            if (isHandDetected)
            {
                handDelta = Vector2.Lerp(handDelta, Vector2.zero, 14f * Time.deltaTime);
            }
            else
            {
                handDelta = Vector2.zero;
            }

#if UNITY_EDITOR
            UpdateSimulasiEditor();
#endif

            UpdateReticleVisual();
        }

        private void OnVuforiaStateUpdated()
        {
            if (VuforiaBehaviour.Instance == null || VuforiaBehaviour.Instance.CameraDevice == null) return;

            Vuforia.Image camImage = VuforiaBehaviour.Instance.CameraDevice.GetCameraImage(PixelFormat.RGB888);
            if (camImage == null || camImage.BufferWidth <= 0 || camImage.BufferHeight <= 0)
            {
                return;
            }

            ProsesFrameKamera(camImage);
        }

        private void ProsesFrameKamera(Vuforia.Image camImage)
        {
            if (!isMediaPipeReady || handLandmarker == null) return;

            int width = camImage.BufferWidth;
            int height = camImage.BufferHeight;
            int stride = camImage.Stride > 0 ? camImage.Stride : width * 3;

            // Zero-copy bridge: Hubungkan pointer buffer Vuforia langsung ke Mediapipe.Image
            using var mpImage = new Mediapipe.Image(
                ImageFormat.Types.Format.Srgb,
                width,
                height,
                stride,
                camImage.PixelBufferPtr,
                NoOpDeleter
            );

            // Pastikan timestamp selalu meningkat secara monoton (persyaratan RunningMode.VIDEO)
            long currentTimestamp = (long)(Time.realtimeSinceStartup * 1000.0);
            if (currentTimestamp <= lastTimestampMs)
            {
                currentTimestamp = lastTimestampMs + 1;
            }
            lastTimestampMs = currentTimestamp;

            var imageProcessingOptions = new ImageProcessingOptions(rotationDegrees: rotasiMediaPipe);

            bool hasDetection = false;
            try
            {
                hasDetection = handLandmarker.TryDetectForVideo(mpImage, currentTimestamp, imageProcessingOptions, ref handResult);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[MediaPipe] TryDetectForVideo exception: " + ex.Message);
                return;
            }

            if (hasDetection && handResult.handLandmarks != null && handResult.handLandmarks.Count > 0)
            {
                var landmarks = handResult.handLandmarks[0].landmarks;
                if (landmarks != null && landmarks.Count >= 21)
                {
                    float currentTime = Time.time;
                    for (int i = 0; i < 21; i++)
                    {
                        var lm = landmarks[i];
                        Vector2 screenPos = KonversiKoordinatLayar(lm.x, lm.y, width, height);
                        rawLandmarks[i] = screenPos;
                        filteredLandmarks[i] = filters[i].Filter(rawLandmarks[i], currentTime);
                    }

                    // Deteksi Gestur Mencubit (Pinch) secara adaptif terhadap skala tangan
                    float pinchDist = Vector2.Distance(filteredLandmarks[4], filteredLandmarks[8]);
                    float palmScale = Vector2.Distance(filteredLandmarks[0], filteredLandmarks[9]);
                    float ambangPinch = Mathf.Max(25f, palmScale * ambangPinchRatio);
                    bool currentPinching = (pinchDist < ambangPinch);
                    bool pinchTransition = (currentPinching != wasPinching);
                    wasPinching = currentPinching;
                    isPinching = currentPinching;

                    // Posisi acuan tangan: jika mencubit, fokuskan pada titik tengah cubitan (jempol + telunjuk)
                    // jika terbuka, fokuskan pada titik tengah telapak tangan (pergelangan + pangkal jari tengah)
                    Vector2 handFocusPt = isPinching
                        ? (filteredLandmarks[4] + filteredLandmarks[8]) * 0.5f
                        : (filteredLandmarks[0] + filteredLandmarks[9]) * 0.5f;

                    Vector2 currentNorm = new Vector2(handFocusPt.x / Screen.width, handFocusPt.y / Screen.height);

                    if (isHandDetected && !pinchTransition)
                    {
                        Vector2 delta = currentNorm - previousHandPos;
                        if (balikSumbuX) delta.x = -delta.x;
                        if (balikSumbuY) delta.y = -delta.y;

                        // Threshold ultra-responsif agar gerakan tangan sekecil apapun langsung terasa
                        if (delta.sqrMagnitude > 0.00000001f)
                        {
                            handDelta = Vector2.Lerp(handDelta, delta, 22f * Time.deltaTime);
                        }
                    }
                    else
                    {
                        handDelta = Vector2.zero;
                    }

                    previousHandPos = currentNorm;
                    handPositionNormalized = currentNorm;
                    handScreenPosition = handFocusPt;
                    handArea = (palmScale * palmScale) / (Screen.width * Screen.height);

                    isHandDetected = true;
                    lastHandSeenTime = Time.time;
                    return;
                }
            }

            // Tangan tidak terdeteksi pada frame ini
            if (Time.time - lastHandSeenTime > 0.12f)
            {
                isHandDetected = false;
                isPinching = false;
                handDelta = Vector2.zero;

                for (int i = 0; i < 21; i++)
                {
                    filters[i].Reset();
                }
            }
        }

        /// <summary>
        /// Mengonversi koordinat normalisasi frame kamera Vuforia ke koordinat layar Unity
        /// dengan penyesuaian Aspect Fill (cropping) dan orientasi Portrait / Landscape.
        /// </summary>
        private Vector2 KonversiKoordinatLayar(float normX, float normY, int camWidth, int camHeight)
        {
            bool isPortrait = (Screen.height > Screen.width);

            float sxNorm;
            float syNorm;

            if (tukarSumbuXY && isPortrait)
            {
                // Sensor landscape kamera Vuforia (misal 1280x720) dipetakan ke layar smartphone Portrait (misal 1080x2400)
                float camAspect = (float)camHeight / camWidth;
                float screenAspect = (float)Screen.width / Screen.height;

                float adjY = normY;
                float adjX = normX;

                if (screenAspect < camAspect)
                {
                    float scale = camAspect / screenAspect;
                    adjY = (normY - 0.5f) * scale + 0.5f;
                }
                else if (screenAspect > camAspect)
                {
                    float scale = screenAspect / camAspect;
                    adjX = (normX - 0.5f) * scale + 0.5f;
                }

                sxNorm = balikSumbuX ? adjY : (1.0f - adjY);
                syNorm = balikSumbuY ? adjX : (1.0f - adjX);
            }
            else
            {
                // Mode Landscape atau simulasi webcam desktop
                float camAspect = (float)camWidth / camHeight;
                float screenAspect = (float)Screen.width / Screen.height;

                float adjX = normX;
                float adjY = normY;

                if (screenAspect > camAspect)
                {
                    float scale = screenAspect / camAspect;
                    adjY = (normY - 0.5f) * scale + 0.5f;
                }
                else if (screenAspect < camAspect)
                {
                    float scale = camAspect / screenAspect;
                    adjX = (normX - 0.5f) * scale + 0.5f;
                }

                sxNorm = balikSumbuX ? (1.0f - adjX) : adjX;
                syNorm = balikSumbuY ? adjY : (1.0f - adjY);
            }

            return new Vector2(
                Mathf.Clamp(sxNorm * Screen.width, 0f, Screen.width),
                Mathf.Clamp(syNorm * Screen.height, 0f, Screen.height)
            );
        }

#if UNITY_EDITOR
        private void UpdateSimulasiEditor()
        {
            if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            {
                isMouseSimulating = true;
                lastMousePos = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
            {
                isMouseSimulating = false;
                handDelta = Vector2.zero;
            }

            if (isMouseSimulating)
            {
                Vector3 currentMouse = Input.mousePosition;
                Vector2 deltaPx = (currentMouse - lastMousePos);
                lastMousePos = currentMouse;

                handScreenPosition = currentMouse;
                handPositionNormalized = new Vector2(currentMouse.x / Screen.width, currentMouse.y / Screen.height);
                handDelta = new Vector2(deltaPx.x / Screen.width, deltaPx.y / Screen.height);

                isPinching = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Space);

                float s = 240f;
                rawLandmarks[0] = (Vector2)currentMouse + new Vector2(0f, -s * 0.40f);
                rawLandmarks[1] = (Vector2)currentMouse + new Vector2(-s * 0.22f, -s * 0.18f);
                rawLandmarks[2] = (Vector2)currentMouse + new Vector2(-s * 0.36f, -s * 0.04f);
                rawLandmarks[3] = (Vector2)currentMouse + new Vector2(-s * 0.42f, s * 0.08f);
                rawLandmarks[4] = (Vector2)currentMouse + new Vector2(-s * 0.45f, s * 0.18f);

                rawLandmarks[5] = (Vector2)currentMouse + new Vector2(-s * 0.20f, s * 0.22f);
                rawLandmarks[6] = (Vector2)currentMouse + new Vector2(-s * 0.20f, s * 0.36f);
                rawLandmarks[7] = (Vector2)currentMouse + new Vector2(-s * 0.20f, s * 0.48f);
                rawLandmarks[8] = (Vector2)currentMouse + new Vector2(-s * 0.20f, s * 0.62f);

                rawLandmarks[9] = (Vector2)currentMouse + new Vector2(0.0f, s * 0.28f);
                rawLandmarks[10] = (Vector2)currentMouse + new Vector2(0.0f, s * 0.42f);
                rawLandmarks[11] = (Vector2)currentMouse + new Vector2(0.0f, s * 0.55f);
                rawLandmarks[12] = (Vector2)currentMouse + new Vector2(0.0f, s * 0.68f);

                rawLandmarks[13] = (Vector2)currentMouse + new Vector2(s * 0.18f, s * 0.24f);
                rawLandmarks[14] = (Vector2)currentMouse + new Vector2(s * 0.18f, s * 0.38f);
                rawLandmarks[15] = (Vector2)currentMouse + new Vector2(s * 0.18f, s * 0.50f);
                rawLandmarks[16] = (Vector2)currentMouse + new Vector2(s * 0.20f, s * 0.62f);

                rawLandmarks[17] = (Vector2)currentMouse + new Vector2(s * 0.32f, s * 0.14f);
                rawLandmarks[18] = (Vector2)currentMouse + new Vector2(s * 0.34f, s * 0.26f);
                rawLandmarks[19] = (Vector2)currentMouse + new Vector2(s * 0.36f, s * 0.38f);
                rawLandmarks[20] = (Vector2)currentMouse + new Vector2(s * 0.38f, s * 0.48f);

                if (isPinching)
                {
                    Vector2 pinchMid = (rawLandmarks[4] + rawLandmarks[8]) * 0.5f;
                    rawLandmarks[4] = pinchMid;
                    rawLandmarks[8] = pinchMid;
                }

                for (int i = 0; i < 21; i++)
                {
                    filteredLandmarks[i] = filters[i].Filter(rawLandmarks[i], Time.time);
                }

                isHandDetected = true;
                lastHandSeenTime = Time.time;
            }
        }
#endif

        private void UpdateReticleVisual()
        {
            if (kerangkaTanganUI != null)
            {
                if (isHandDetected)
                {
                    kerangkaTanganUI.PerbaruiKerangkaMediaPipe(filteredLandmarks, isPinching, true);
                }
                else
                {
                    kerangkaTanganUI.PerbaruiKerangkaMediaPipe(null, false, false);
                }
            }

            if (reticleHandUI != null)
            {
                if (isHandDetected)
                {
                    if (!reticleHandUI.gameObject.activeSelf) reticleHandUI.gameObject.SetActive(true);

                    reticleHandUI.position = handScreenPosition;

                    if (txtReticleStatus != null)
                    {
                        if (isPinching)
                        {
                            txtReticleStatus.text = "<color=#FFD700>[PINCH] MENCUBIT</color>\n<size=12><color=#FFFFFF>Putaran Dipercepat</color></size>";
                        }
                        else
                        {
                            txtReticleStatus.text = "<color=#00E5FF>[AKTIF] TANGAN TERLACAK</color>\n<size=12><color=#FFFFFF>Geser tangan untuk memutar / membalik</color></size>";
                        }
                    }
                }
                else
                {
                    if (reticleHandUI.gameObject.activeSelf)
                    {
                        reticleHandUI.gameObject.SetActive(false);
                    }
                }
            }
        }

        public void EnsureHUDAndReticle()
        {
            GameObject hudGO = GameObject.Find("Canvas_HUD");
            if (hudGO == null)
            {
                hudGO = new GameObject("Canvas_HUD");
                Canvas c = hudGO.AddComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                c.sortingOrder = 100;

                CanvasScaler scaler = hudGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;

                hudGO.AddComponent<GraphicRaycaster>();
            }

            if (kerangkaTanganUI == null)
            {
                Transform existingSkeleton = hudGO.transform.Find("KerangkaTangan_Overlay");
                if (existingSkeleton != null)
                {
                    kerangkaTanganUI = existingSkeleton.GetComponent<KerangkaTanganUI>();
                }
                else
                {
                    GameObject skeletonGO = new GameObject("KerangkaTangan_Overlay", typeof(RectTransform), typeof(KerangkaTanganUI));
                    skeletonGO.transform.SetParent(hudGO.transform, false);

                    RectTransform sRect = skeletonGO.GetComponent<RectTransform>();
                    sRect.anchorMin = Vector2.zero;
                    sRect.anchorMax = Vector2.one;
                    sRect.offsetMin = Vector2.zero;
                    sRect.offsetMax = Vector2.zero;

                    kerangkaTanganUI = skeletonGO.GetComponent<KerangkaTanganUI>();
                }
            }

            if (reticleHandUI == null)
            {
                Transform existingReticle = hudGO.transform.Find("HandReticle_HUD");
                if (existingReticle != null)
                {
                    reticleHandUI = existingReticle.GetComponent<RectTransform>();
                    reticleImage = existingReticle.GetComponent<UnityEngine.UI.Image>();
                    txtReticleStatus = existingReticle.GetComponentInChildren<TextMeshProUGUI>();
                }
                else
                {
                    GameObject rGO = new GameObject("HandReticle_HUD", typeof(RectTransform));
                    rGO.transform.SetParent(hudGO.transform, false);
                    reticleHandUI = rGO.GetComponent<RectTransform>();
                    reticleHandUI.sizeDelta = new Vector2(260, 80);

                    GameObject txtGO = new GameObject("Txt_HandStatus", typeof(RectTransform), typeof(TextMeshProUGUI));
                    txtGO.transform.SetParent(rGO.transform, false);
                    txtReticleStatus = txtGO.GetComponent<TextMeshProUGUI>();
                    txtReticleStatus.fontSize = 18;
                    txtReticleStatus.alignment = TextAlignmentOptions.Center;
                    txtReticleStatus.raycastTarget = false;

                    RectTransform tRect = txtGO.GetComponent<RectTransform>();
                    tRect.anchorMin = Vector2.zero;
                    tRect.anchorMax = Vector2.one;
                    tRect.offsetMin = Vector2.zero;
                    tRect.offsetMax = Vector2.zero;
                }
            }
        }
    }
}
