using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Kelompok4.AR
{
    public class PengaturARObjectRecognition : MonoBehaviour
    {
        [Header("Referensi Objek")]
        [SerializeField] private GameObject bukuVirtual3D;
        [SerializeField] private GameObject floatingCanvas;
        [SerializeField] private InteraksiBukuAR interaksiBuku;
        private PelacakTanganKameraAR pelacakTangan;

        [Header("Referensi UI HUD Layar")]
        [SerializeField] private TextMeshProUGUI txtStatus;
        [SerializeField] private Button btnMunculkanScreen;
        [SerializeField] private TextMeshProUGUI txtBtnScreen;
        [SerializeField] private Button btnReset;
        [SerializeField] private Button btnAutoPutar;
        [SerializeField] private GameObject bottomControlsPanel;

        [Header("Referensi Tombol Melayang (World Space)")]
        [SerializeField] private Button btnMunculkanFloating;
        [SerializeField] private TextMeshProUGUI txtBtnFloating;

        [Header("Status Saat Ini")]
        public bool isTargetFound = false;
        public bool isObjectSpawned = false;

        private float lastToggleTime = 0f;

        private void Awake()
        {
            // 1. Pastikan PelacakTanganKameraAR terpasang dan aktif
            if (pelacakTangan == null)
            {
                pelacakTangan = GetComponent<PelacakTanganKameraAR>();
                if (pelacakTangan == null)
                {
                    pelacakTangan = gameObject.AddComponent<PelacakTanganKameraAR>();
                }
            }

            // 2. Hubungkan referensi buku 3D di scene
            if (bukuVirtual3D == null)
            {
                GameObject it = GameObject.Find("ImageTarget");
                if (it != null)
                {
                    Transform b = it.transform.Find("Buku_Virtual_3D");
                    if (b != null) bukuVirtual3D = b.gameObject;
                }
            }
            if (interaksiBuku == null && bukuVirtual3D != null)
            {
                interaksiBuku = bukuVirtual3D.GetComponent<InteraksiBukuAR>();
            }
            if (btnMunculkanFloating == null)
            {
                GameObject it = GameObject.Find("ImageTarget");
                if (it != null)
                {
                    btnMunculkanFloating = it.GetComponentInChildren<Button>(true);
                }
            }
            if (txtBtnFloating == null && btnMunculkanFloating != null)
            {
                txtBtnFloating = btnMunculkanFloating.GetComponentInChildren<TextMeshProUGUI>(true);
            }

            // 3. Pastikan seluruh elemen HUD Layar (Canvas_HUD, Status, Reset, Auto-Putar) tersedia
            EnsureHUDAndControls();
        }

        private void Start()
        {
            // 1. Pastikan buku 3D dalam kondisi tersembunyi di awal
            SetBukuActive(false);

            // 2. Hubungkan listener tombol HUD layar
            if (btnMunculkanScreen != null)
            {
                btnMunculkanScreen.onClick.RemoveListener(ToggleMunculkan3D);
                btnMunculkanScreen.onClick.AddListener(ToggleMunculkan3D);
            }

            // Hindari double-toggle pada tombol melayang
            if (btnMunculkanFloating != null)
            {
                btnMunculkanFloating.onClick.RemoveListener(ToggleMunculkan3D);
            }

            if (btnReset != null && interaksiBuku != null)
            {
                btnReset.onClick.RemoveListener(interaksiBuku.ResetTransform);
                btnReset.onClick.AddListener(interaksiBuku.ResetTransform);
            }
            if (btnAutoPutar != null && interaksiBuku != null)
            {
                btnAutoPutar.onClick.RemoveListener(interaksiBuku.ToggleAutoRotate);
                btnAutoPutar.onClick.AddListener(interaksiBuku.ToggleAutoRotate);
            }

            // 3. Set tampilan awal
            UpdateUI();
        }

        private void Update()
        {
            // Update status teks secara dinamis berdasarkan Hand Tracking saat objek sedang aktif
            if (isTargetFound && isObjectSpawned)
            {
                UpdateHandTrackingStatusText();
            }
        }

        private void UpdateHandTrackingStatusText()
        {
            if (txtStatus == null) return;

            if (pelacakTangan != null && pelacakTangan.isHandDetected)
            {
                if (pelacakTangan.isPinching)
                {
                    txtStatus.text = "AR BUKU 3D (HAND TRACKING)\n<size=20><color=#FFD700>🟡 Gestur Mencubit: Geser tangan untuk memutar / membalikkan cepat</color></size>";
                }
                else
                {
                    txtStatus.text = "AR BUKU 3D (HAND TRACKING)\n<size=20><color=#00E5FF>🟢 Tangan Terdeteksi: Geser tangan untuk Memutar atau Membalikkan</color></size>";
                }
            }
            else
            {
                txtStatus.text = "AR BUKU 3D (HAND TRACKING)\n<size=20><color=#64B5F6>✋ Letakkan tangan di depan kamera untuk berinteraksi di udara</color></size>";
            }
        }

        // Fungsi khusus untuk mengaktifkan/menonaktifkan buku 3D beserta seluruh Renderer & Collider
        public void SetBukuActive(bool active)
        {
            if (bukuVirtual3D == null) return;

            bukuVirtual3D.SetActive(active);

            Renderer[] renderers = bukuVirtual3D.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                r.enabled = active;
            }

            Collider[] colliders = bukuVirtual3D.GetComponentsInChildren<Collider>(true);
            foreach (Collider c in colliders)
            {
                c.enabled = active;
            }
        }

        // Dipanggil otomatis oleh event OnTargetFound milik Vuforia DefaultObserverEventHandler
        public void OnTargetFound()
        {
            isTargetFound = true;
            Debug.Log("[PengaturAR] >>> Buku Pariwisata TERDETEKSI oleh kamera! <<<");

            // Jika sebelumnya pengguna sudah memunculkan objek 3D, tampilkan kembali
            SetBukuActive(isObjectSpawned);

            UpdateUI();
        }

        // Dipanggil otomatis oleh event OnTargetLost milik Vuforia DefaultObserverEventHandler
        public void OnTargetLost()
        {
            isTargetFound = false;
            Debug.Log("[PengaturAR] >>> Buku Pariwisata keluar dari pandangan kamera. <<<");

            // Sembunyikan buku sementara saat target hilang, tapi jangan ubah isObjectSpawned
            SetBukuActive(false);

            UpdateUI();
        }

        // Dipanggil saat tombol "Munculkan Objek 3D" ditekan
        public void ToggleMunculkan3D()
        {
            // Proteksi Debounce: jika terpanggil ganda dalam waktu kurang dari 0.4 detik, abaikan panggilan kedua!
            if (Time.time - lastToggleTime < 0.4f)
            {
                Debug.LogWarning("[PengaturAR] ToggleMunculkan3D diabaikan karena duplicate call!");
                return;
            }
            lastToggleTime = Time.time;

            isObjectSpawned = !isObjectSpawned;
            Debug.Log("[PengaturAR] >>> ToggleMunculkan3D: isObjectSpawned = " + isObjectSpawned + " <<<");

            SetBukuActive(isObjectSpawned);

            if (isObjectSpawned && interaksiBuku != null)
            {
                interaksiBuku.ResetTransform();
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            // Tampilan saat target belum terdeteksi sama sekali
            if (!isTargetFound)
            {
                if (txtStatus != null)
                {
                    txtStatus.text = "AR BUKU PARIWISATA 3D\n<size=22><color=#FFCC00>Arahkan kamera ke gambar sampul Buku Pariwisata</color></size>";
                }

                if (!isObjectSpawned)
                {
                    if (btnMunculkanScreen != null) btnMunculkanScreen.gameObject.SetActive(false);
                    if (bottomControlsPanel != null) bottomControlsPanel.SetActive(false);
                }
                return;
            }

            // Tampilan saat target SUDAH terdeteksi tapi belum dimunculkan
            if (!isObjectSpawned)
            {
                if (txtStatus != null)
                {
                    txtStatus.text = "AR BUKU PARIWISATA 3D\n<size=22><color=#00E676>Buku Pariwisata Terdeteksi! Tekan tombol untuk memunculkan 3D</color></size>";
                }

                if (btnMunculkanScreen != null)
                {
                    btnMunculkanScreen.gameObject.SetActive(true);
                }
                if (txtBtnScreen != null)
                {
                    txtBtnScreen.text = "MUNCULKAN OBJEK 3D";
                }
                if (txtBtnFloating != null)
                {
                    txtBtnFloating.text = "MUNCULKAN OBJEK 3D";
                }
                if (bottomControlsPanel != null)
                {
                    bottomControlsPanel.SetActive(false);
                }
            }
            else
            {
                UpdateHandTrackingStatusText();

                if (btnMunculkanScreen != null)
                {
                    btnMunculkanScreen.gameObject.SetActive(true);
                }
                if (txtBtnScreen != null)
                {
                    txtBtnScreen.text = "SEMBUNYIKAN OBJEK 3D";
                }
                if (txtBtnFloating != null)
                {
                    txtBtnFloating.text = "SEMBUNYIKAN OBJEK 3D";
                }
                if (bottomControlsPanel != null)
                {
                    bottomControlsPanel.SetActive(true);
                }
            }
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

            // Status Banner (Top)
            if (txtStatus == null)
            {
                Transform panelStatus = hudGO.transform.Find("Panel_Status");
                if (panelStatus == null)
                {
                    GameObject pGO = new GameObject("Panel_Status", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                    pGO.transform.SetParent(hudGO.transform, false);

                    RectTransform pr = pGO.GetComponent<RectTransform>();
                    pr.anchorMin = new Vector2(0f, 1f);
                    pr.anchorMax = new Vector2(1f, 1f);
                    pr.pivot = new Vector2(0.5f, 1f);
                    pr.anchoredPosition = new Vector2(0f, -40f);
                    pr.sizeDelta = new Vector2(-60f, 150f);

                    UnityEngine.UI.Image pImg = pGO.GetComponent<UnityEngine.UI.Image>();
                    pImg.color = new Color(0.04f, 0.08f, 0.16f, 0.88f);

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
                    tr.offsetMin = new Vector2(25f, 10f);
                    tr.offsetMax = new Vector2(-25f, -10f);

                    txtStatus = tGO.GetComponent<TextMeshProUGUI>();
                    txtStatus.fontSize = 24;
                    txtStatus.alignment = TextAlignmentOptions.Center;
                    txtStatus.color = Color.white;
                    txtStatus.text = "AR BUKU PARIWISATA 3D\n<size=20><color=#FFCC00>Arahkan kamera ke gambar sampul Buku Pariwisata</color></size>";
                }
                else
                {
                    txtStatus = txtT.GetComponent<TextMeshProUGUI>();
                }
            }

            // Bottom Controls (Reset & Auto Putar)
            if (bottomControlsPanel == null)
            {
                Transform bPanel = hudGO.transform.Find("Panel_BottomControls");
                if (bPanel == null)
                {
                    GameObject bGO = new GameObject("Panel_BottomControls", typeof(RectTransform));
                    bGO.transform.SetParent(hudGO.transform, false);

                    RectTransform br = bGO.GetComponent<RectTransform>();
                    br.anchorMin = new Vector2(0.5f, 0f);
                    br.anchorMax = new Vector2(0.5f, 0f);
                    br.pivot = new Vector2(0.5f, 0f);
                    br.anchoredPosition = new Vector2(0f, 40f);
                    br.sizeDelta = new Vector2(480f, 75f);

                    HorizontalLayoutGroup layout = bGO.AddComponent<HorizontalLayoutGroup>();
                    layout.spacing = 18f;
                    layout.childAlignment = TextAnchor.MiddleCenter;
                    layout.childControlWidth = false;
                    layout.childControlHeight = false;

                    // Button Reset
                    GameObject rGO = new GameObject("Btn_Reset", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(Button));
                    rGO.transform.SetParent(bGO.transform, false);
                    rGO.GetComponent<RectTransform>().sizeDelta = new Vector2(210f, 60f);
                    rGO.GetComponent<UnityEngine.UI.Image>().color = new Color(0.2f, 0.45f, 0.85f, 0.9f);
                    btnReset = rGO.GetComponent<Button>();

                    GameObject rtGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                    rtGO.transform.SetParent(rGO.transform, false);
                    RectTransform rtr = rtGO.GetComponent<RectTransform>();
                    rtr.anchorMin = Vector2.zero; rtr.anchorMax = Vector2.one; rtr.sizeDelta = Vector2.zero;
                    TextMeshProUGUI rtmp = rtGO.GetComponent<TextMeshProUGUI>();
                    rtmp.fontSize = 17; rtmp.fontStyle = FontStyles.Bold; rtmp.alignment = TextAlignmentOptions.Center;
                    rtmp.text = "RESET POSISI"; rtmp.color = Color.white;

                    // Button Auto-Putar
                    GameObject aGO = new GameObject("Btn_AutoPutar", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(Button));
                    aGO.transform.SetParent(bGO.transform, false);
                    aGO.GetComponent<RectTransform>().sizeDelta = new Vector2(210f, 60f);
                    aGO.GetComponent<UnityEngine.UI.Image>().color = new Color(0.12f, 0.65f, 0.38f, 0.9f);
                    btnAutoPutar = aGO.GetComponent<Button>();

                    GameObject atGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                    atGO.transform.SetParent(aGO.transform, false);
                    RectTransform atr = atGO.GetComponent<RectTransform>();
                    atr.anchorMin = Vector2.zero; atr.anchorMax = Vector2.one; atr.sizeDelta = Vector2.zero;
                    TextMeshProUGUI atmp = atGO.GetComponent<TextMeshProUGUI>();
                    atmp.fontSize = 17; atmp.fontStyle = FontStyles.Bold; atmp.alignment = TextAlignmentOptions.Center;
                    atmp.text = "AUTO-PUTAR"; atmp.color = Color.white;

                    bottomControlsPanel = bGO;
                }
                else
                {
                    bottomControlsPanel = bPanel.gameObject;
                    btnReset = bottomControlsPanel.transform.Find("Btn_Reset")?.GetComponent<Button>();
                    btnAutoPutar = bottomControlsPanel.transform.Find("Btn_AutoPutar")?.GetComponent<Button>();
                }
            }
        }
    }
}
