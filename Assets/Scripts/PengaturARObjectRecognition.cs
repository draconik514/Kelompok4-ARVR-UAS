using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Vuforia;
using Image = UnityEngine.UI.Image;

namespace Kelompok4.AR
{
    /// <summary>
    /// PengaturARObjectRecognition:
    /// Arsitektur Lengkap Kelompok 4 — Object Recognition (AR) + Hand Tracking (VR):
    /// 1. Mode AR: Deteksi 5 Objek Nyata Vuforia + Sticky AR Lock (objek TIDAK hilang saat tertutup tangan!)
    ///    + Overlay Instruksi & Kartu Informasi AR Interaktif saat objek di-scan atau dipencet (Tap-to-Inspect).
    /// 2. Transisi AR -> Mode Ruang VR: Manipulasi versi virtual objek dengan 21 titik sendi Hand Tracking MediaPipe
    ///    di Ruang Studio Virtual 3D + dukungan Split-Screen Stereoscopic VR Box & Gyroscope.
    /// </summary>
    public class PengaturARObjectRecognition : MonoBehaviour
    {
        [System.Serializable]
        public class EntriTargetObjek
        {
            public string targetName;
            public string displayName;
            public ObserverBehaviour observer;
            public GameObject model3D;
            public InteraksiBukuAR interaksi;
            public bool sedangTerlacak;
        }

        [Header("Referensi Objek Legacy (Kompatibilitas)")]
        [SerializeField] private GameObject bukuVirtual3D;
        [SerializeField] private GameObject floatingCanvas;
        [SerializeField] private InteraksiBukuAR interaksiBuku;
        [SerializeField] private GameObject[] daftarObjekSehariHari;
        private PelacakTanganKameraAR pelacakTangan;

        [Header("Daftar 5 Target Vuforia Aktif")]
        public List<EntriTargetObjek> daftarTarget = new List<EntriTargetObjek>();

        [Header("Referensi UI HUD Layar")]
        [SerializeField] private TextMeshProUGUI txtStatus;
        [SerializeField] private Button btnMunculkanScreen;
        [SerializeField] private TextMeshProUGUI txtBtnScreen;
        [SerializeField] private Button btnReset;
        [SerializeField] private Button btnAutoPutar;
        [SerializeField] private Button btnGantiObjek;
        [SerializeField] private TextMeshProUGUI txtBtnGantiObjek;
        [SerializeField] private GameObject bottomControlsPanel;

        [Header("Referensi Tombol Melayang (World Space)")]
        [SerializeField] private Button btnMunculkanFloating;
        [SerializeField] private TextMeshProUGUI txtBtnFloating;

        [Header("Status Sistem AR & VR")]
        public bool isTargetFound = false;
        public bool isObjectSpawned = true;
        public bool isVRMode = false;
        public bool isStereoSplitVR = false;

        // Sticky Lock AR & Anchor Objek di Depan Kamera (agar saat tangan diangkat menutupi target, objek TIDAK hilang!)
        private int indeksAktif = -1;
        private bool sedangStickyHoldAR = false;
        private GameObject cameraStageAnchor;
        private GameObject stageCloneGO;
        private InteraksiBukuAR stageInteraksi;

        // Lingkungan Ruang Studio VR & Kamera Stereo Cardboard/VR Box
        private GameObject vrStudioRoot;
        private Camera stereoLeftCam;
        private Camera stereoRightCam;

        // UI Panel Informasi & Instruksi AR + Tombol Transisi VR
        private GameObject panelInfoAR;
        private TextMeshProUGUI txtInfoJudul;
        private TextMeshProUGUI txtInfoKategori;
        private TextMeshProUGUI txtInfoBody;
        private TextMeshProUGUI txtInfoInstruksi;
        private GameObject badgeTapHintGO;
        private TextMeshProUGUI txtBadgeTapHint;
        private Button btnInfoToggle;
        private TextMeshProUGUI txtBtnInfoToggle;
        private Button btnModeVR;
        private TextMeshProUGUI txtBtnModeVR;
        private Button btnStereoVRBox;
        private TextMeshProUGUI txtBtnStereoVRBox;
        private TextMeshProUGUI txtBtnResetLabel;

        // Deteksi Tap (Pencet Objek 3D di Layar)
        private Vector2 pointerDownPos;
        private float pointerDownTime;
        private bool pointerDownValid;

        private static readonly Dictionary<string, string> PetaNamaTarget = new Dictionary<string, string>
        {
            { "Buku_Pariwisata", "Buku Pariwisata Togean" },
            { "2_Buku_Campus", "Buku Tulis iBook Campus" },
            { "3_Kalkulator_Citizen", "Kalkulator Citizen SDC-810BN" },
            { "4_Penghapus_Faber", "Penghapus Faber-Castell" },
            { "1_Pulpen_Pilot", "Pulpen Pilot G-2" }
        };

        private void Awake()
        {
            // PENTING: Matikan MSAA & OcclusionCulling pada ARCamera agar video kamera belakang
            // di smartphone Android (Mali/Adreno OpenGL ES) TIDAK PERNAH berubah menjadi layar hitam saat objek 3D muncul!
            QualitySettings.antiAliasing = 0;
            KonfigurasiKameraARAntiHitam();

            if (pelacakTangan == null)
            {
                pelacakTangan = GetComponent<PelacakTanganKameraAR>();
                if (pelacakTangan == null)
                {
                    pelacakTangan = gameObject.AddComponent<PelacakTanganKameraAR>();
                }
            }

            DaftarkanSemuaTargetVuforia();
            EnsureHUDAndControls();
            EnsureExtendedARVRUI();
        }

        private void KonfigurasiKameraARAntiHitam()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject arCamGO = GameObject.Find("ARCamera");
                if (arCamGO != null) cam = arCamGO.GetComponent<Camera>();
            }

            if (cam != null)
            {
                cam.allowMSAA = false;
                cam.allowHDR = false;
                cam.useOcclusionCulling = false;
                cam.nearClipPlane = 0.015f;
                cam.farClipPlane = 50f;
            }
        }

        private void DaftarkanSemuaTargetVuforia()
        {
            daftarTarget.Clear();
            ObserverBehaviour[] observers = FindObjectsByType<ObserverBehaviour>(FindObjectsInactive.Include);

            foreach (ObserverBehaviour obs in observers)
            {
                if (obs == null) continue;

                Canvas[] worldCanvases = obs.GetComponentsInChildren<Canvas>(true);
                foreach (Canvas wc in worldCanvases)
                {
                    if (wc != null && wc.renderMode == RenderMode.WorldSpace)
                    {
                        wc.gameObject.SetActive(false);
                    }
                }

                InteraksiBukuAR inter = obs.GetComponentInChildren<InteraksiBukuAR>(true);
                GameObject modelGO = inter != null ? inter.gameObject : null;
                if (modelGO == null && obs.transform.childCount > 0)
                {
                    for (int i = 0; i < obs.transform.childCount; i++)
                    {
                        Transform ch = obs.transform.GetChild(i);
                        if (ch.GetComponent<Canvas>() == null)
                        {
                            modelGO = ch.gameObject;
                            inter = modelGO.GetComponent<InteraksiBukuAR>();
                            if (inter == null) inter = modelGO.AddComponent<InteraksiBukuAR>();
                            break;
                        }
                    }
                }

                string tName = string.IsNullOrEmpty(obs.TargetName) ? obs.gameObject.name : obs.TargetName;
                string dName = PetaNamaTarget.TryGetValue(tName, out string mapped) ? mapped : tName.Replace("_", " ");

                EntriTargetObjek entri = new EntriTargetObjek
                {
                    targetName = tName,
                    displayName = dName,
                    observer = obs,
                    model3D = modelGO,
                    interaksi = inter,
                    sedangTerlacak = false
                };

                daftarTarget.Add(entri);

                obs.OnTargetStatusChanged -= HandleObserverStatusChanged;
                obs.OnTargetStatusChanged += HandleObserverStatusChanged;
            }

            string[] urutanIdeal = { "Buku_Pariwisata", "2_Buku_Campus", "3_Kalkulator_Citizen", "4_Penghapus_Faber", "1_Pulpen_Pilot" };
            daftarTarget.Sort((a, b) =>
            {
                int ia = System.Array.IndexOf(urutanIdeal, a.targetName);
                int ib = System.Array.IndexOf(urutanIdeal, b.targetName);
                if (ia < 0) ia = 99;
                if (ib < 0) ib = 99;
                return ia.CompareTo(ib);
            });

            if (daftarTarget.Count > 0 && daftarTarget[0].model3D != null)
            {
                bukuVirtual3D = daftarTarget[0].model3D;
                interaksiBuku = daftarTarget[0].interaksi;
            }
        }

        private void OnDestroy()
        {
            foreach (var entri in daftarTarget)
            {
                if (entri.observer != null)
                {
                    entri.observer.OnTargetStatusChanged -= HandleObserverStatusChanged;
                }
            }
        }

        private void Start()
        {
            KonfigurasiKameraARAntiHitam();

            foreach (var entri in daftarTarget)
            {
                bool trackedNow = entri.observer != null && IsStatusTracked(entri.observer.TargetStatus);
                entri.sedangTerlacak = trackedNow;
                if (entri.model3D != null)
                {
                    SetObjekVisible(entri.model3D, trackedNow && !isVRMode);
                }
            }

            if (btnReset != null)
            {
                btnReset.onClick.RemoveAllListeners();
                btnReset.onClick.AddListener(AksiResetAtauScanUlang);
            }

            if (btnAutoPutar != null)
            {
                btnAutoPutar.onClick.RemoveAllListeners();
                btnAutoPutar.onClick.AddListener(ToggleAutoPutarObjekAktif);
            }

            if (btnGantiObjek != null)
            {
                btnGantiObjek.onClick.RemoveAllListeners();
                btnGantiObjek.onClick.AddListener(GantiObjekBerikutnya);
            }

            RefreshTrackingState();
            UpdateSemuaLabelTombol();
            UpdateUI();
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Space))
            {
                GantiObjekBerikutnya();
            }
#endif
            // Perbarui posisi objek yang sedang dikunci di depan kamera (Sticky AR Hold maupun Mode Ruang VR)
            if ((sedangStickyHoldAR || isVRMode) && cameraStageAnchor != null && Camera.main != null)
            {
                Transform camT = Camera.main.transform;
                float dist = isVRMode ? 0.38f : 0.35f;
                float yOffset = isVRMode ? -0.01f : -0.02f;
                cameraStageAnchor.transform.position = camT.position + camT.forward * dist + camT.up * yOffset;
                cameraStageAnchor.transform.rotation = Quaternion.LookRotation(camT.forward, camT.up) * Quaternion.Euler(-25f, 0f, 0f);
            }

            // Deteksi ketukan jari / klik pada Objek 3D (Tap-to-Inspect Informasi & Instruksi AR!)
            DeteksiKetukanPadaObjek3D();

            if (isTargetFound || sedangStickyHoldAR || isVRMode)
            {
                UpdateHandTrackingStatusText();
            }
        }

        private void DeteksiKetukanPadaObjek3D()
        {
            if (Input.touchCount == 1)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began)
                {
                    pointerDownValid = !(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId));
                    pointerDownPos = t.position;
                    pointerDownTime = Time.time;
                }
                else if (t.phase == TouchPhase.Ended && pointerDownValid)
                {
                    pointerDownValid = false;
                    if ((Time.time - pointerDownTime) < 0.32f && Vector2.Distance(t.position, pointerDownPos) < 28f)
                    {
                        CobaRaycastTapObjek(t.position);
                    }
                }
            }
#if UNITY_EDITOR
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    pointerDownValid = !(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject());
                    pointerDownPos = Input.mousePosition;
                    pointerDownTime = Time.time;
                }
                else if (Input.GetMouseButtonUp(0) && pointerDownValid)
                {
                    pointerDownValid = false;
                    if ((Time.time - pointerDownTime) < 0.32f && Vector2.Distance((Vector2)Input.mousePosition, pointerDownPos) < 20f)
                    {
                        CobaRaycastTapObjek(Input.mousePosition);
                    }
                }
            }
#endif
        }

        private void CobaRaycastTapObjek(Vector2 screenPos)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, 10f))
            {
                InteraksiBukuAR hitInteraksi = hit.collider.GetComponentInParent<InteraksiBukuAR>();
                if (hitInteraksi != null && indeksAktif >= 0)
                {
                    // Objek 3D dipencet! Buka/tutup Kartu Informasi & Instruksi AR
                    TogglePanelInformasiAR();
                }
            }
        }

        private bool IsStatusTracked(TargetStatus status)
        {
            return status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED;
        }

        private void HandleObserverStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
        {
            bool tracked = IsStatusTracked(targetStatus);
            int changedIdx = -1;

            for (int i = 0; i < daftarTarget.Count; i++)
            {
                if (daftarTarget[i].observer == behaviour)
                {
                    changedIdx = i;
                    daftarTarget[i].sedangTerlacak = tracked;
                    break;
                }
            }

            if (changedIdx < 0) return;

            // Jika sedang di dalam Mode Ruang VR penuh, jangan ganggu panggung VR dengan event kamera luar
            if (isVRMode)
            {
                if (daftarTarget[changedIdx].model3D != null)
                {
                    SetObjekVisible(daftarTarget[changedIdx].model3D, false);
                }
                RefreshTrackingState();
                return;
            }

            if (tracked)
            {
                bool gantiTargetBaru = (indeksAktif != changedIdx);
                indeksAktif = changedIdx;

                // Matikan clone sticky hold karena target fisik aslinya sedang terlihat jelas oleh kamera!
                Quaternion rotasiTerakhir = stageCloneGO != null ? stageCloneGO.transform.localRotation : Quaternion.identity;
                bool punyaRotasiSticky = (stageCloneGO != null && !gantiTargetBaru);
                HapusStageClone();
                sedangStickyHoldAR = false;

                // Hanya tampilkan model pada target aktif
                for (int i = 0; i < daftarTarget.Count; i++)
                {
                    if (daftarTarget[i].model3D != null)
                    {
                        bool vis = (i == indeksAktif) && isObjectSpawned;
                        SetObjekVisible(daftarTarget[i].model3D, vis);
                    }
                }

                if (punyaRotasiSticky && daftarTarget[indeksAktif].model3D != null)
                {
                    daftarTarget[indeksAktif].model3D.transform.localRotation = rotasiTerakhir;
                }

                interaksiBuku = daftarTarget[indeksAktif].interaksi;
                PerbaruiIsiPanelInfo(daftarTarget[indeksAktif].targetName);

                // Saat target baru pertama kali di-scan di AR, tampilkan otomatis Overlay Informasi & Instruksi AR!
                if (gantiTargetBaru && panelInfoAR != null)
                {
                    panelInfoAR.SetActive(true);
                }

                Debug.Log($"[PengaturAR] >>> TARGET TERDETEKSI: {daftarTarget[changedIdx].displayName} <<<");
            }
            else
            {
                // SOLUSI INTI MASALAH TANGAN MENUTUPI KAMERA:
                // Ketika pengguna mengangkat tangan di depan kamera HP, tangan menutupi gambar target sehingga Vuforia memicu TargetLost.
                // Alih-alih menghilangkan benda 3D, kita aktifkan STICKY AR LOCK di depan kamera agar pengguna bebas memutarnya dengan tangan!
                if (daftarTarget[changedIdx].model3D != null)
                {
                    SetObjekVisible(daftarTarget[changedIdx].model3D, false);
                }

                RefreshTrackingState();
                if (!isTargetFound && indeksAktif == changedIdx && isObjectSpawned)
                {
                    Quaternion rotTerakhir = daftarTarget[changedIdx].model3D != null
                        ? daftarTarget[changedIdx].model3D.transform.localRotation
                        : Quaternion.identity;
                    AktifkanStageCloneDiDepanKamera(changedIdx, rotTerakhir, 1.05f);
                    sedangStickyHoldAR = true;
                }
            }

            RefreshTrackingState();
            UpdateSemuaLabelTombol();
            UpdateUI();
        }

        private void AktifkanStageCloneDiDepanKamera(int idx, Quaternion initialRot, float scaleMultiplier)
        {
            if (idx < 0 || idx >= daftarTarget.Count) return;
            GameObject sumber = daftarTarget[idx].model3D;
            if (sumber == null) return;

            if (cameraStageAnchor == null)
            {
                cameraStageAnchor = new GameObject("StageAnchor_CameraFront");
            }

            if (stageCloneGO != null)
            {
                Destroy(stageCloneGO);
            }

            stageCloneGO = Instantiate(sumber, cameraStageAnchor.transform);
            stageCloneGO.name = sumber.name + "_StageActive";
            stageCloneGO.transform.localPosition = Vector3.zero;
            stageCloneGO.transform.localRotation = initialRot;
            stageCloneGO.transform.localScale = sumber.transform.localScale * scaleMultiplier;

            SetObjekVisible(stageCloneGO, true);
            stageInteraksi = stageCloneGO.GetComponent<InteraksiBukuAR>();
            if (stageInteraksi == null) stageInteraksi = stageCloneGO.AddComponent<InteraksiBukuAR>();
            stageInteraksi.CaptureInitialTransform();
        }

        private void HapusStageClone()
        {
            if (stageCloneGO != null)
            {
                Destroy(stageCloneGO);
                stageCloneGO = null;
            }
            stageInteraksi = null;
        }

        private void RefreshTrackingState()
        {
            int count = 0;
            for (int i = 0; i < daftarTarget.Count; i++)
            {
                if (daftarTarget[i].sedangTerlacak) count++;
            }
            isTargetFound = count > 0;
        }

        /// <summary>
        /// Transisi AR <-> Mode Ruang VR (Hand Tracking Studio) sesuai alur wajib UAS.
        /// </summary>
        public void ToggleModeVR()
        {
            if (!isVRMode)
            {
                // Masuk ke Mode Ruang VR: jika belum ada objek yang di-scan, gunakan objek pertama (Buku Pariwisata)
                if (indeksAktif < 0 && daftarTarget.Count > 0)
                {
                    indeksAktif = 0;
                }
                if (indeksAktif < 0) return;

                isVRMode = true;
                sedangStickyHoldAR = false;

                // Sembunyikan model pada ImageTarget AR
                for (int i = 0; i < daftarTarget.Count; i++)
                {
                    if (daftarTarget[i].model3D != null)
                    {
                        SetObjekVisible(daftarTarget[i].model3D, false);
                    }
                }

                // Aktifkan Ruang Studio Virtual 3D mengelilingi ARCamera (kamera tetap hidup di belakang layar untuk MediaPipe!)
                Camera cam = Camera.main;
                if (vrStudioRoot == null && cam != null)
                {
                    vrStudioRoot = DataInformasiObjekAR.BangunRuangStudioVR(cam.transform);
                }
                if (vrStudioRoot != null) vrStudioRoot.SetActive(true);

                // Tampilkan objek virtual berukuran besar & jelas di tengah Ruang VR
                AktifkanStageCloneDiDepanKamera(indeksAktif, Quaternion.identity, 1.30f);
                PerbaruiIsiPanelInfo(daftarTarget[indeksAktif].targetName);

                // Tutup panel info besar saat pertama masuk VR agar ruang manipulasi tangan lega
                if (panelInfoAR != null) panelInfoAR.SetActive(false);

                Debug.Log($"[PengaturAR] >>> TRANSISI AR -> VR AKTIF untuk objek: {daftarTarget[indeksAktif].displayName} <<<");
            }
            else
            {
                // Kembali dari Mode VR ke Mode Kamera AR
                MatikanSplitScreenVRBox();
                isVRMode = false;
                if (vrStudioRoot != null) vrStudioRoot.SetActive(false);
                HapusStageClone();

                // Cek apakah saat ini ada target fisik yang sedang terlihat kamera AR
                RefreshTrackingState();
                if (isTargetFound)
                {
                    sedangStickyHoldAR = false;
                    for (int i = 0; i < daftarTarget.Count; i++)
                    {
                        if (daftarTarget[i].sedangTerlacak)
                        {
                            indeksAktif = i;
                            SetObjekVisible(daftarTarget[i].model3D, true);
                            break;
                        }
                    }
                }
                else if (indeksAktif >= 0)
                {
                    // Tetap pertahankan objek dalam Sticky AR di atas kamera nyata sampai pengguna tekan Scan Baru
                    AktifkanStageCloneDiDepanKamera(indeksAktif, Quaternion.identity, 1.05f);
                    sedangStickyHoldAR = true;
                }

                Debug.Log("[PengaturAR] <<< KEMBALI KE MODE KAMERA AR <<<");
            }

            UpdateSemuaLabelTombol();
            UpdateUI();
        }

        /// <summary>
        /// Toggle Split-Screen Stereoscopic (2 Lensa Kiri-Kanan) untuk Google Cardboard / VR Box di dalam Mode VR.
        /// </summary>
        public void ToggleSplitScreenVRBox()
        {
            if (!isVRMode) return;

            if (!isStereoSplitVR)
            {
                Camera mainCam = Camera.main;
                if (mainCam == null) return;

                isStereoSplitVR = true;
                Input.gyro.enabled = true;

                if (stereoLeftCam == null)
                {
                    GameObject leftGO = new GameObject("VR_LeftEyeCamera");
                    leftGO.transform.SetParent(mainCam.transform, false);
                    leftGO.transform.localPosition = new Vector3(-0.032f, 0f, 0f);
                    stereoLeftCam = leftGO.AddComponent<Camera>();
                    SalinPengaturanKameraVR(mainCam, stereoLeftCam, new Rect(0f, 0f, 0.5f, 1f));
                }

                if (stereoRightCam == null)
                {
                    GameObject rightGO = new GameObject("VR_RightEyeCamera");
                    rightGO.transform.SetParent(mainCam.transform, false);
                    rightGO.transform.localPosition = new Vector3(0.032f, 0f, 0f);
                    stereoRightCam = rightGO.AddComponent<Camera>();
                    SalinPengaturanKameraVR(mainCam, stereoRightCam, new Rect(0.5f, 0f, 0.5f, 1f));
                }

                stereoLeftCam.gameObject.SetActive(true);
                stereoRightCam.gameObject.SetActive(true);
            }
            else
            {
                MatikanSplitScreenVRBox();
            }

            UpdateSemuaLabelTombol();
        }

        private void SalinPengaturanKameraVR(Camera src, Camera dst, Rect viewport)
        {
            dst.clearFlags = CameraClearFlags.SolidColor;
            dst.backgroundColor = new Color(0.03f, 0.06f, 0.12f, 1f);
            dst.nearClipPlane = 0.015f;
            dst.farClipPlane = 20f;
            dst.fieldOfView = 60f;
            dst.depth = src.depth + 5f;
            dst.rect = viewport;
            dst.allowMSAA = false;
            dst.allowHDR = false;
            dst.useOcclusionCulling = false;
        }

        private void MatikanSplitScreenVRBox()
        {
            isStereoSplitVR = false;
            if (stereoLeftCam != null) stereoLeftCam.gameObject.SetActive(false);
            if (stereoRightCam != null) stereoRightCam.gameObject.SetActive(false);
        }

        public void TogglePanelInformasiAR()
        {
            if (panelInfoAR == null) return;
            if (indeksAktif < 0 && daftarTarget.Count > 0)
            {
                indeksAktif = 0;
            }
            if (indeksAktif >= 0 && indeksAktif < daftarTarget.Count)
            {
                PerbaruiIsiPanelInfo(daftarTarget[indeksAktif].targetName);
            }
            panelInfoAR.SetActive(!panelInfoAR.activeSelf);
            UpdateSemuaLabelTombol();
        }

        private void PerbaruiIsiPanelInfo(string targetName)
        {
            InfoEdukasiObjek info = DataInformasiObjekAR.AmbilInfo(targetName);
            if (txtInfoJudul != null) txtInfoJudul.text = info.judul.ToUpper();
            if (txtInfoKategori != null) txtInfoKategori.text = $"{info.kategori}  •  {info.spesifikasiSingkat}";
            if (txtInfoBody != null) txtInfoBody.text = info.deskripsi;
            if (txtInfoInstruksi != null) txtInfoInstruksi.text = "<color=#FFD54F><b>OVERLAY INSTRUKSI PENGGUNAAN:</b></color>\n" + info.instruksiOverlay;

            if (txtBadgeTapHint != null)
            {
                txtBadgeTapHint.text = isVRMode
                    ? $"MODE RUANG VR: <b>{info.judul}</b>  •  Gerakkan tangan / Pinch di depan kamera"
                    : $"TERKUNCI: <b>{info.judul}</b>  •  <color=#80D8FF>[Ketuk Objek 3D untuk Detail Info AR]</color>";
            }
        }

        public void GantiObjekBerikutnya()
        {
            if (daftarTarget.Count == 0) return;

            // Matikan renderer target lama
            for (int i = 0; i < daftarTarget.Count; i++)
            {
                if (daftarTarget[i].model3D != null) SetObjekVisible(daftarTarget[i].model3D, false);
            }

            indeksAktif = (indeksAktif + 1) % daftarTarget.Count;
            float mult = isVRMode ? 1.30f : 1.05f;
            AktifkanStageCloneDiDepanKamera(indeksAktif, Quaternion.identity, mult);
            sedangStickyHoldAR = !isVRMode;

            PerbaruiIsiPanelInfo(daftarTarget[indeksAktif].targetName);
            if (!isVRMode && panelInfoAR != null)
            {
                panelInfoAR.SetActive(true);
            }

            UpdateSemuaLabelTombol();
            UpdateUI();
        }

        public void AksiResetAtauScanUlang()
        {
            if (isVRMode)
            {
                if (stageInteraksi != null) stageInteraksi.ResetTransform();
                return;
            }

            // Di Mode AR: jika objek sedang dikunci oleh Sticky Hold (tanpa target fisik di depan kamera),
            // tombol ini melepaskan kunci agar pengguna kembali ke mode Scan Kamera bersih!
            if (sedangStickyHoldAR && !isTargetFound)
            {
                HapusStageClone();
                sedangStickyHoldAR = false;
                indeksAktif = -1;
                if (panelInfoAR != null) panelInfoAR.SetActive(false);
                UpdateSemuaLabelTombol();
                UpdateUI();
                return;
            }

            ResetObjekAktif();
        }

        public void ResetObjekAktif()
        {
            if (stageInteraksi != null) stageInteraksi.ResetTransform();
            foreach (var entri in daftarTarget)
            {
                if (entri.sedangTerlacak && entri.interaksi != null)
                {
                    entri.interaksi.ResetTransform();
                }
            }
        }

        public void ToggleAutoPutarObjekAktif()
        {
            if (stageInteraksi != null) stageInteraksi.ToggleAutoRotate();
            foreach (var entri in daftarTarget)
            {
                if (entri.sedangTerlacak && entri.interaksi != null)
                {
                    entri.interaksi.ToggleAutoRotate();
                }
            }
        }

        private void UpdateHandTrackingStatusText()
        {
            if (txtStatus == null) return;

            string namaObjek = (indeksAktif >= 0 && indeksAktif < daftarTarget.Count)
                ? daftarTarget[indeksAktif].displayName.ToUpper()
                : "OBJEK 3D";

            string prefixMode = isVRMode ? "MODE RUANG VR" : (isTargetFound ? "MODE AR (TARGET LIVE)" : "MODE AR (OBJEK TERKUNCI)");

            if (pelacakTangan != null && pelacakTangan.isHandDetected)
            {
                if (pelacakTangan.isPinching)
                {
                    txtStatus.text = $"{prefixMode}: {namaObjek}\n<size=19><color=#FFD700>[PINCH] Mencubit: Putaran Cepat Aktif (2.2x)</color></size>";
                }
                else
                {
                    txtStatus.text = $"{prefixMode}: {namaObjek}\n<size=19><color=#00E5FF>[HAND TRACKING AKTIF] Geser tangan di udara untuk memutar 360°</color></size>";
                }
            }
            else
            {
                string sub = isVRMode
                    ? "Angkat 1 tangan di depan kamera HP untuk memutar objek virtual di udara!"
                    : "Ketuk objek 3D untuk Info AR • Angkat tangan untuk memutar (Objek sudah terkunci anti-hilang!)";
                txtStatus.text = $"{prefixMode}: {namaObjek}\n<size=18><color=#80D8FF>{sub}</color></size>";
            }
        }

        private void SetObjekVisible(GameObject targetObj, bool visible)
        {
            if (targetObj == null) return;
            if (visible && !targetObj.activeSelf) targetObj.SetActive(true);

            Renderer[] renderers = targetObj.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                r.enabled = visible;
            }

            Collider[] colliders = targetObj.GetComponentsInChildren<Collider>(true);
            foreach (Collider c in colliders)
            {
                c.enabled = visible;
            }
        }

        private void UpdateSemuaLabelTombol()
        {
            int total = daftarTarget.Count > 0 ? daftarTarget.Count : 5;
            if (txtBtnGantiObjek != null)
            {
                txtBtnGantiObjek.text = indeksAktif >= 0
                    ? $"OBJEK ({indeksAktif + 1}/{total})"
                    : $"PILIH OBJEK (0/{total})";
            }

            if (txtBtnResetLabel != null)
            {
                txtBtnResetLabel.text = (!isVRMode && sedangStickyHoldAR && !isTargetFound)
                    ? "SCAN BARU"
                    : "RESET POSISI";
            }

            if (txtBtnModeVR != null)
            {
                txtBtnModeVR.text = isVRMode
                    ? "<< KEMBALI KE AR"
                    : "MASUK MODE VR >>";
            }

            if (btnModeVR != null)
            {
                Image img = btnModeVR.GetComponent<Image>();
                if (img != null)
                {
                    img.color = isVRMode
                        ? new Color(0.85f, 0.22f, 0.28f, 0.95f)
                        : new Color(0.42f, 0.20f, 0.90f, 0.96f);
                }
            }

            if (btnStereoVRBox != null)
            {
                btnStereoVRBox.gameObject.SetActive(isVRMode);
                if (txtBtnStereoVRBox != null)
                {
                    txtBtnStereoVRBox.text = isStereoSplitVR ? "VR BOX: 2 LAYAR" : "VR BOX: 1 LAYAR";
                }
            }

            if (txtBtnInfoToggle != null)
            {
                bool infoTerbuka = panelInfoAR != null && panelInfoAR.activeSelf;
                txtBtnInfoToggle.text = infoTerbuka ? "TUTUP INFO" : "INFO & INSTRUKSI";
            }

            if (badgeTapHintGO != null)
            {
                badgeTapHintGO.SetActive(indeksAktif >= 0);
            }
        }

        public void OnTargetFound()
        {
            RefreshTrackingState();
            UpdateUI();
        }

        public void OnTargetLost()
        {
            RefreshTrackingState();
            UpdateUI();
        }

        public void ToggleMunculkan3D()
        {
            isObjectSpawned = !isObjectSpawned;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (bottomControlsPanel != null) bottomControlsPanel.SetActive(true);

            if (!isTargetFound && !sedangStickyHoldAR && !isVRMode)
            {
                if (txtStatus != null)
                {
                    txtStatus.text = "MODE AR — DETEKSI OBJEK SEHARI-HARI\n<size=18><color=#FFCC00>Arahkan kamera ke Buku / Kalkulator / Penghapus / Pulpen (atau tekan PILIH OBJEK)</color></size>";
                }
                if (badgeTapHintGO != null) badgeTapHintGO.SetActive(false);
                return;
            }

            UpdateHandTrackingStatusText();
        }

        private void EnsureHUDAndControls()
        {
            GameObject hudGO = GameObject.Find("Canvas_HUD");
            if (hudGO == null)
            {
                hudGO = new GameObject("Canvas_HUD");
                Canvas c = hudGO.AddComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                c.sortingOrder = 50;

                CanvasScaler scaler = hudGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;

                hudGO.AddComponent<GraphicRaycaster>();
            }

            if (txtStatus == null)
            {
                Transform panelStatus = hudGO.transform.Find("Panel_Status");
                if (panelStatus == null)
                {
                    GameObject pGO = new GameObject("Panel_Status", typeof(RectTransform), typeof(Image));
                    pGO.transform.SetParent(hudGO.transform, false);

                    RectTransform pr = pGO.GetComponent<RectTransform>();
                    pr.anchorMin = new Vector2(0f, 1f);
                    pr.anchorMax = new Vector2(1f, 1f);
                    pr.pivot = new Vector2(0.5f, 1f);
                    pr.anchoredPosition = new Vector2(0f, -36f);
                    pr.sizeDelta = new Vector2(-48f, 140f);

                    pGO.GetComponent<Image>().color = new Color(0.04f, 0.08f, 0.16f, 0.88f);
                    panelStatus = pGO.transform;
                }

                Transform txtT = panelStatus.Find("Txt_Status");
                if (txtT == null)
                {
                    GameObject tGO = new GameObject("Txt_Status", typeof(RectTransform), typeof(TextMeshProUGUI));
                    tGO.transform.SetParent(panelStatus, false);

                    RectTransform tr = tGO.GetComponent<RectTransform>();
                    tr.anchorMin = Vector2.zero;
                    tr.anchorMax = Vector2.one;
                    tr.offsetMin = new Vector2(18f, 8f);
                    tr.offsetMax = new Vector2(-18f, -8f);

                    txtStatus = tGO.GetComponent<TextMeshProUGUI>();
                    txtStatus.fontSize = 22;
                    txtStatus.alignment = TextAlignmentOptions.Center;
                    txtStatus.color = Color.white;
                }
                else
                {
                    txtStatus = txtT.GetComponent<TextMeshProUGUI>();
                }
            }

            Transform bPanel = hudGO.transform.Find("Panel_BottomControls");
            if (bPanel == null)
            {
                GameObject bGO = new GameObject("Panel_BottomControls", typeof(RectTransform));
                bGO.transform.SetParent(hudGO.transform, false);

                RectTransform br = bGO.GetComponent<RectTransform>();
                br.anchorMin = new Vector2(0.5f, 0f);
                br.anchorMax = new Vector2(0.5f, 0f);
                br.pivot = new Vector2(0.5f, 0f);
                br.anchoredPosition = new Vector2(0f, 34f);
                br.sizeDelta = new Vector2(980f, 74f);

                HorizontalLayoutGroup layout = bGO.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 14f;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = false;
                layout.childControlHeight = false;

                bPanel = bGO.transform;
            }
            else
            {
                RectTransform br = bPanel.GetComponent<RectTransform>();
                br.anchoredPosition = new Vector2(0f, 34f);
                br.sizeDelta = new Vector2(980f, 74f);
            }

            bottomControlsPanel = bPanel.gameObject;

            Transform gantiT = bPanel.Find("Btn_GantiObjek");
            if (gantiT == null)
            {
                btnGantiObjek = BuatTombolUI(bPanel, "Btn_GantiObjek", "PILIH OBJEK (0/5)", new Color(0.92f, 0.45f, 0.10f, 0.95f), new Vector2(225f, 64f));
                btnGantiObjek.transform.SetAsFirstSibling();
            }
            else
            {
                btnGantiObjek = gantiT.GetComponent<Button>();
            }
            txtBtnGantiObjek = btnGantiObjek != null ? btnGantiObjek.GetComponentInChildren<TextMeshProUGUI>() : null;

            btnReset = bPanel.Find("Btn_Reset")?.GetComponent<Button>();
            if (btnReset == null)
            {
                btnReset = BuatTombolUI(bPanel, "Btn_Reset", "RESET POSISI", new Color(0.20f, 0.42f, 0.82f, 0.92f), new Vector2(205f, 64f));
            }
            txtBtnResetLabel = btnReset.GetComponentInChildren<TextMeshProUGUI>();

            btnAutoPutar = bPanel.Find("Btn_AutoPutar")?.GetComponent<Button>();
            if (btnAutoPutar == null)
            {
                btnAutoPutar = BuatTombolUI(bPanel, "Btn_AutoPutar", "AUTO-PUTAR", new Color(0.14f, 0.65f, 0.40f, 0.92f), new Vector2(205f, 64f));
            }
        }

        /// <summary>
        /// Membangun baris kontrol AR->VR kedua + Panel Kartu Informasi & Overlay Instruksi AR Interaktif.
        /// </summary>
        private void EnsureExtendedARVRUI()
        {
            GameObject hudGO = GameObject.Find("Canvas_HUD");
            if (hudGO == null) return;

            // 1. Baris Tombol Aksi Utama AR <-> VR (di atas Panel_BottomControls)
            Transform actionRowT = hudGO.transform.Find("Panel_ActionARVR");
            if (actionRowT == null)
            {
                GameObject rowGO = new GameObject("Panel_ActionARVR", typeof(RectTransform));
                rowGO.transform.SetParent(hudGO.transform, false);

                RectTransform rr = rowGO.GetComponent<RectTransform>();
                rr.anchorMin = new Vector2(0.5f, 0f);
                rr.anchorMax = new Vector2(0.5f, 0f);
                rr.pivot = new Vector2(0.5f, 0f);
                rr.anchoredPosition = new Vector2(0f, 118f);
                rr.sizeDelta = new Vector2(980f, 74f);

                HorizontalLayoutGroup hl = rowGO.AddComponent<HorizontalLayoutGroup>();
                hl.spacing = 14f;
                hl.childAlignment = TextAnchor.MiddleCenter;
                hl.childControlWidth = false;
                hl.childControlHeight = false;

                actionRowT = rowGO.transform;
            }

            btnInfoToggle = actionRowT.Find("Btn_InfoAR")?.GetComponent<Button>();
            if (btnInfoToggle == null)
            {
                btnInfoToggle = BuatTombolUI(actionRowT, "Btn_InfoAR", "INFO & INSTRUKSI", new Color(0.08f, 0.52f, 0.68f, 0.95f), new Vector2(250f, 64f));
            }
            txtBtnInfoToggle = btnInfoToggle.GetComponentInChildren<TextMeshProUGUI>();
            btnInfoToggle.onClick.RemoveAllListeners();
            btnInfoToggle.onClick.AddListener(TogglePanelInformasiAR);

            btnModeVR = actionRowT.Find("Btn_MasukVR")?.GetComponent<Button>();
            if (btnModeVR == null)
            {
                btnModeVR = BuatTombolUI(actionRowT, "Btn_MasukVR", "MASUK MODE VR >>", new Color(0.42f, 0.20f, 0.90f, 0.96f), new Vector2(290f, 64f));
            }
            txtBtnModeVR = btnModeVR.GetComponentInChildren<TextMeshProUGUI>();
            btnModeVR.onClick.RemoveAllListeners();
            btnModeVR.onClick.AddListener(ToggleModeVR);

            btnStereoVRBox = actionRowT.Find("Btn_StereoVRBox")?.GetComponent<Button>();
            if (btnStereoVRBox == null)
            {
                btnStereoVRBox = BuatTombolUI(actionRowT, "Btn_StereoVRBox", "VR BOX: 1 LAYAR", new Color(0.22f, 0.28f, 0.42f, 0.95f), new Vector2(220f, 64f));
            }
            txtBtnStereoVRBox = btnStereoVRBox.GetComponentInChildren<TextMeshProUGUI>();
            btnStereoVRBox.onClick.RemoveAllListeners();
            btnStereoVRBox.onClick.AddListener(ToggleSplitScreenVRBox);
            btnStereoVRBox.gameObject.SetActive(false);

            // 2. Badge Petunjuk Ketuk Objek (di bawah Top Status Banner)
            Transform badgeT = hudGO.transform.Find("Badge_TapHint");
            if (badgeT == null)
            {
                badgeTapHintGO = new GameObject("Badge_TapHint", typeof(RectTransform), typeof(Image), typeof(Button));
                badgeTapHintGO.transform.SetParent(hudGO.transform, false);

                RectTransform br = badgeTapHintGO.GetComponent<RectTransform>();
                br.anchorMin = new Vector2(0.5f, 1f);
                br.anchorMax = new Vector2(0.5f, 1f);
                br.pivot = new Vector2(0.5f, 1f);
                br.anchoredPosition = new Vector2(0f, -186f);
                br.sizeDelta = new Vector2(920f, 54f);

                badgeTapHintGO.GetComponent<Image>().color = new Color(0.06f, 0.16f, 0.28f, 0.88f);
                badgeTapHintGO.GetComponent<Button>().onClick.AddListener(TogglePanelInformasiAR);

                GameObject btGO = new GameObject("Txt_Badge", typeof(RectTransform), typeof(TextMeshProUGUI));
                btGO.transform.SetParent(badgeTapHintGO.transform, false);
                RectTransform btr = btGO.GetComponent<RectTransform>();
                btr.anchorMin = Vector2.zero;
                btr.anchorMax = Vector2.one;
                btr.offsetMin = new Vector2(16f, 4f);
                btr.offsetMax = new Vector2(-16f, -4f);

                txtBadgeTapHint = btGO.GetComponent<TextMeshProUGUI>();
                txtBadgeTapHint.fontSize = 18;
                txtBadgeTapHint.alignment = TextAlignmentOptions.Center;
                txtBadgeTapHint.color = Color.white;
            }
            else
            {
                badgeTapHintGO = badgeT.gameObject;
                txtBadgeTapHint = badgeTapHintGO.GetComponentInChildren<TextMeshProUGUI>();
            }
            badgeTapHintGO.SetActive(false);

            // 3. Kartu Informasi Edukasi & Overlay Instruksi AR (Muncul saat scan objek baru atau saat objek 3D dipencet!)
            Transform cardT = hudGO.transform.Find("Panel_InfoAR");
            if (cardT == null)
            {
                panelInfoAR = new GameObject("Panel_InfoAR", typeof(RectTransform), typeof(Image));
                panelInfoAR.transform.SetParent(hudGO.transform, false);

                RectTransform cr = panelInfoAR.GetComponent<RectTransform>();
                cr.anchorMin = new Vector2(0.5f, 0f);
                cr.anchorMax = new Vector2(0.5f, 0f);
                cr.pivot = new Vector2(0.5f, 0f);
                cr.anchoredPosition = new Vector2(0f, 204f);
                cr.sizeDelta = new Vector2(960f, 430f);

                panelInfoAR.GetComponent<Image>().color = new Color(0.04f, 0.09f, 0.18f, 0.93f);

                // Header Bar Biru Cyan
                GameObject headerBar = new GameObject("HeaderAccent", typeof(RectTransform), typeof(Image));
                headerBar.transform.SetParent(panelInfoAR.transform, false);
                RectTransform hr = headerBar.GetComponent<RectTransform>();
                hr.anchorMin = new Vector2(0f, 1f);
                hr.anchorMax = new Vector2(1f, 1f);
                hr.pivot = new Vector2(0.5f, 1f);
                hr.anchoredPosition = Vector2.zero;
                hr.sizeDelta = new Vector2(0f, 8f);
                headerBar.GetComponent<Image>().color = new Color(0f, 0.82f, 1f, 1f);

                // Tombol [X] Tutup di kanan atas kartu
                Button btnClose = BuatTombolUI(panelInfoAR.transform, "Btn_CloseInfo", "TUTUP [X]", new Color(0.75f, 0.22f, 0.25f, 0.92f), new Vector2(150f, 46f));
                RectTransform clr = btnClose.GetComponent<RectTransform>();
                clr.anchorMin = new Vector2(1f, 1f);
                clr.anchorMax = new Vector2(1f, 1f);
                clr.pivot = new Vector2(1f, 1f);
                clr.anchoredPosition = new Vector2(-18f, -18f);
                btnClose.onClick.AddListener(() => { panelInfoAR.SetActive(false); UpdateSemuaLabelTombol(); });

                // Judul Objek
                txtInfoJudul = BuatTeksKartu(panelInfoAR.transform, "Txt_Judul", new Vector2(26f, -18f), new Vector2(750f, 44f), 26, FontStyles.Bold, new Color(0.35f, 0.92f, 1f));
                // Kategori & Spesifikasi
                txtInfoKategori = BuatTeksKartu(panelInfoAR.transform, "Txt_Kategori", new Vector2(26f, -64f), new Vector2(900f, 34f), 17, FontStyles.Italic, new Color(0.75f, 0.85f, 0.95f));
                // Deskripsi Edukasi AR
                txtInfoBody = BuatTeksKartu(panelInfoAR.transform, "Txt_Deskripsi", new Vector2(26f, -104f), new Vector2(908f, 145f), 19, FontStyles.Normal, Color.white);
                // Kotak Overlay Instruksi
                txtInfoInstruksi = BuatTeksKartu(panelInfoAR.transform, "Txt_Instruksi", new Vector2(26f, -258f), new Vector2(908f, 155f), 18, FontStyles.Normal, new Color(0.92f, 0.96f, 1f));
            }
            else
            {
                panelInfoAR = cardT.gameObject;
                txtInfoJudul = cardT.Find("Txt_Judul")?.GetComponent<TextMeshProUGUI>();
                txtInfoKategori = cardT.Find("Txt_Kategori")?.GetComponent<TextMeshProUGUI>();
                txtInfoBody = cardT.Find("Txt_Deskripsi")?.GetComponent<TextMeshProUGUI>();
                txtInfoInstruksi = cardT.Find("Txt_Instruksi")?.GetComponent<TextMeshProUGUI>();
            }

            panelInfoAR.SetActive(false);
        }

        private Button BuatTombolUI(Transform parent, string name, string label, Color bgColor, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().sizeDelta = size;
            go.GetComponent<Image>().color = bgColor;

            GameObject tGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            tGO.transform.SetParent(go.transform, false);
            RectTransform tr = tGO.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(8f, 4f);
            tr.offsetMax = new Vector2(-8f, -4f);

            TextMeshProUGUI tmp = tGO.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = 16;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.text = label;

            return go.GetComponent<Button>();
        }

        private TextMeshProUGUI BuatTeksKartu(Transform parent, string name, Vector2 topLeftOffset, Vector2 size, float fontSize, FontStyles style, Color col)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = topLeftOffset;
            rt.sizeDelta = size;

            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.color = col;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            return tmp;
        }
    }
}
