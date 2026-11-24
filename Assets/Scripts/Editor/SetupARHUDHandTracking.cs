#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Kelompok4.AR;

namespace Kelompok4.AR.Editor
{
    [InitializeOnLoad]
    public static class SetupARHUDHandTracking
    {
        static SetupARHUDHandTracking()
        {
            EditorApplication.delayCall += AutoEnsureIfNeeded;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.delayCall += AutoEnsureIfNeeded;
            }
        }

        private static void AutoEnsureIfNeeded()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            bool lengkap =
                GameObject.Find("ImageTarget") != null &&
                GameObject.Find("ImageTarget_BukuCampus") != null &&
                GameObject.Find("ImageTarget_Kalkulator") != null &&
                GameObject.Find("ImageTarget_Penghapus") != null &&
                GameObject.Find("ImageTarget_Pulpen") != null &&
                GameObject.Find("Btn_GantiObjek") != null;

            if (!lengkap)
            {
                ExecuteSetup();
            }
        }

        [MenuItem("AR/Refresh and Reload Domain")]
        public static void ForceReload()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            Debug.Log("[SetupARHUD] AssetDatabase refresh & script reload requested!");
        }

        [MenuItem("AR/Setup HUD and Hand Tracking")]
        public static void ExecuteSetup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "Scene_AR")
            {
                string scenePath = "Assets/Scenes/Scene_AR.unity";
                if (System.IO.File.Exists(scenePath))
                {
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                }
                else
                {
                    return;
                }
            }

            // 0. Bangun ke-5 ImageTarget Vuforia dari DB_Kelompok4 & 3D Digital Twin masing-masing
            KatalogObjekSehariHariBuilder.BangunKatalogObjek();

            // 1. Dapatkan atau Buat Canvas_HUD (Screen Space Overlay)
            GameObject canvasHUDGO = GameObject.Find("Canvas_HUD");
            if (canvasHUDGO == null)
            {
                canvasHUDGO = new GameObject("Canvas_HUD");
                Canvas canvas = canvasHUDGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 10;

                CanvasScaler scaler = canvasHUDGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;

                canvasHUDGO.AddComponent<GraphicRaycaster>();
            }

            // 2. Panel_Status (Top Banner)
            Transform panelStatusT = canvasHUDGO.transform.Find("Panel_Status");
            GameObject panelStatusGO;
            if (panelStatusT == null)
            {
                panelStatusGO = new GameObject("Panel_Status", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                panelStatusGO.transform.SetParent(canvasHUDGO.transform, false);

                RectTransform rect = panelStatusGO.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -40f);
                rect.sizeDelta = new Vector2(-60f, 160f);

                UnityEngine.UI.Image img = panelStatusGO.GetComponent<UnityEngine.UI.Image>();
                img.color = new Color(0.05f, 0.08f, 0.15f, 0.85f);
            }
            else
            {
                panelStatusGO = panelStatusT.gameObject;
            }

            Transform txtStatusT = panelStatusGO.transform.Find("Txt_Status");
            TextMeshProUGUI txtStatus;
            if (txtStatusT == null)
            {
                GameObject txtGO = new GameObject("Txt_Status", typeof(RectTransform), typeof(TextMeshProUGUI));
                txtGO.transform.SetParent(panelStatusGO.transform, false);

                RectTransform rect = txtGO.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = new Vector2(20f, 10f);
                rect.offsetMax = new Vector2(-20f, -10f);

                txtStatus = txtGO.GetComponent<TextMeshProUGUI>();
                txtStatus.fontSize = 23;
                txtStatus.alignment = TextAlignmentOptions.Center;
                txtStatus.color = Color.white;
            }
            else
            {
                txtStatus = txtStatusT.GetComponent<TextMeshProUGUI>();
            }
            txtStatus.text = "AR DETEKSI 5 OBJEK NYATA + HAND TRACKING\n<size=19><color=#FFCC00>Scan Buku / Kalkulator / Penghapus / Pulpen (atau klik PREVIEW 3D)</color></size>";

            // 3. KerangkaTangan_Overlay (Hand Skeleton Renderer)
            Transform skeletonT = canvasHUDGO.transform.Find("KerangkaTangan_Overlay");
            KerangkaTanganUI kerangkaUI;
            if (skeletonT == null)
            {
                GameObject sGO = new GameObject("KerangkaTangan_Overlay", typeof(RectTransform), typeof(KerangkaTanganUI));
                sGO.transform.SetParent(canvasHUDGO.transform, false);
                RectTransform sRect = sGO.GetComponent<RectTransform>();
                sRect.anchorMin = Vector2.zero;
                sRect.anchorMax = Vector2.one;
                sRect.offsetMin = Vector2.zero;
                sRect.offsetMax = Vector2.zero;
                kerangkaUI = sGO.GetComponent<KerangkaTanganUI>();
            }
            else
            {
                kerangkaUI = skeletonT.GetComponent<KerangkaTanganUI>();
            }

            // 4. Reticle_HandTracking
            Transform reticleT = canvasHUDGO.transform.Find("Reticle_HandTracking");
            GameObject reticleGO;
            UnityEngine.UI.Image reticleImage;
            TextMeshProUGUI txtReticle;
            if (reticleT == null)
            {
                reticleGO = new GameObject("Reticle_HandTracking", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                reticleGO.transform.SetParent(canvasHUDGO.transform, false);

                RectTransform rect = reticleGO.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(80f, 80f);

                reticleImage = reticleGO.GetComponent<UnityEngine.UI.Image>();
                reticleImage.color = Color.clear;

                GameObject txtReticleGO = new GameObject("Txt_ReticleStatus", typeof(RectTransform), typeof(TextMeshProUGUI));
                txtReticleGO.transform.SetParent(reticleGO.transform, false);
                RectTransform textRect = txtReticleGO.GetComponent<RectTransform>();
                textRect.anchoredPosition = new Vector2(0f, -80f);
                textRect.sizeDelta = new Vector2(300f, 45f);

                txtReticle = txtReticleGO.GetComponent<TextMeshProUGUI>();
                txtReticle.fontSize = 15;
                txtReticle.fontStyle = FontStyles.Bold;
                txtReticle.alignment = TextAlignmentOptions.Center;
                txtReticle.color = Color.white;
                txtReticle.text = "[AKTIF] TANGAN TERLACAK";
            }
            else
            {
                reticleGO = reticleT.gameObject;
                reticleImage = reticleGO.GetComponent<UnityEngine.UI.Image>();
                reticleImage.color = Color.clear;
                Transform dot = reticleGO.transform.Find("CenterDot");
                if (dot != null) dot.gameObject.SetActive(false);
                txtReticle = reticleGO.GetComponentInChildren<TextMeshProUGUI>(true);
                if (txtReticle != null) txtReticle.text = "[AKTIF] TANGAN TERLACAK";
            }
            reticleGO.SetActive(false);

            // 5. Panel_BottomControls (PASTIKAN ke-3 tombol dibuat lengkap bahkan jika panel sudah ada sebelumnya!)
            Transform bottomControlsT = canvasHUDGO.transform.Find("Panel_BottomControls");
            GameObject bottomControlsGO;
            if (bottomControlsT == null)
            {
                bottomControlsGO = new GameObject("Panel_BottomControls", typeof(RectTransform));
                bottomControlsGO.transform.SetParent(canvasHUDGO.transform, false);

                RectTransform rect = bottomControlsGO.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(0f, 40f);
                rect.sizeDelta = new Vector2(720f, 80f);

                HorizontalLayoutGroup layout = bottomControlsGO.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 15f;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
            }
            else
            {
                bottomControlsGO = bottomControlsT.gameObject;
                bottomControlsGO.GetComponent<RectTransform>().sizeDelta = new Vector2(720f, 80f);
            }

            Button btnGantiObjek = PastikanTombol(bottomControlsGO.transform, "Btn_GantiObjek", "PREVIEW 3D (0/5)", new Color(0.92f, 0.45f, 0.10f, 0.95f), new Vector2(225f, 65f), true);
            Button btnReset = PastikanTombol(bottomControlsGO.transform, "Btn_Reset", "RESET POSISI", new Color(0.20f, 0.40f, 0.80f, 0.90f), new Vector2(210f, 65f), false);
            Button btnAutoPutar = PastikanTombol(bottomControlsGO.transform, "Btn_AutoPutar", "AUTO-PUTAR", new Color(0.15f, 0.65f, 0.40f, 0.90f), new Vector2(210f, 65f), false);
            TextMeshProUGUI txtGantiObjek = btnGantiObjek != null ? btnGantiObjek.GetComponentInChildren<TextMeshProUGUI>() : null;

            // 6. Hubungkan ke ManajerScene
            GameObject manajerGO = GameObject.Find("ManajerScene");
            if (manajerGO != null)
            {
                PelacakTanganKameraAR pelacak = manajerGO.GetComponent<PelacakTanganKameraAR>();
                if (pelacak == null) pelacak = manajerGO.AddComponent<PelacakTanganKameraAR>();
                pelacak.kerangkaTanganUI = kerangkaUI;

                SerializedObject soPelacak = new SerializedObject(pelacak);
                soPelacak.FindProperty("reticleHandUI").objectReferenceValue = reticleGO.GetComponent<RectTransform>();
                soPelacak.FindProperty("reticleImage").objectReferenceValue = reticleImage;
                soPelacak.FindProperty("txtReticleStatus").objectReferenceValue = txtReticle;
                soPelacak.ApplyModifiedPropertiesWithoutUndo();

                PengaturARObjectRecognition pengatur = manajerGO.GetComponent<PengaturARObjectRecognition>();
                if (pengatur != null)
                {
                    SerializedObject soPengatur = new SerializedObject(pengatur);
                    soPengatur.FindProperty("txtStatus").objectReferenceValue = txtStatus;
                    soPengatur.FindProperty("btnReset").objectReferenceValue = btnReset;
                    soPengatur.FindProperty("btnAutoPutar").objectReferenceValue = btnAutoPutar;
                    soPengatur.FindProperty("btnGantiObjek").objectReferenceValue = btnGantiObjek;
                    soPengatur.FindProperty("txtBtnGantiObjek").objectReferenceValue = txtGantiObjek;
                    soPengatur.FindProperty("bottomControlsPanel").objectReferenceValue = bottomControlsGO;
                    soPengatur.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[SetupARHUD] SUKSES! 5 Target Vuforia Nyata + 3D Digital Twins + HUD Hand Tracking telah aktif di Scene_AR!");
        }

        private static Button PastikanTombol(Transform parent, string btnName, string label, Color bgColor, Vector2 size, bool setFirstSibling)
        {
            Transform existing = parent.Find(btnName);
            GameObject btnGO;
            if (existing == null)
            {
                btnGO = new GameObject(btnName, typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(Button));
                btnGO.transform.SetParent(parent, false);
                if (setFirstSibling) btnGO.transform.SetAsFirstSibling();

                GameObject txtGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                txtGO.transform.SetParent(btnGO.transform, false);
                RectTransform tr = txtGO.GetComponent<RectTransform>();
                tr.anchorMin = Vector2.zero;
                tr.anchorMax = Vector2.one;
                tr.sizeDelta = Vector2.zero;
            }
            else
            {
                btnGO = existing.gameObject;
                if (setFirstSibling) btnGO.transform.SetAsFirstSibling();
            }

            btnGO.GetComponent<RectTransform>().sizeDelta = size;
            btnGO.GetComponent<UnityEngine.UI.Image>().color = bgColor;

            TextMeshProUGUI tmp = btnGO.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.fontSize = 16;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                tmp.text = label;
            }

            return btnGO.GetComponent<Button>();
        }
    }
}
#endif
