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
            EditorApplication.delayCall += ExecuteSetup;
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
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "Scene_AR")
            {
                string scenePath = "Assets/Scenes/Scene_AR.unity";
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }

            Debug.Log("[SetupARHUD] Memulai konfigurasi Canvas HUD & Pelacak Tangan...");

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

            // 2. Dapatkan atau Buat Panel_Status (Top Banner)
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

            // Txt_Status
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
                txtStatus.fontSize = 24;
                txtStatus.alignment = TextAlignmentOptions.Center;
                txtStatus.color = Color.white;
                txtStatus.text = "AR BUKU PARIWISATA 3D\n<size=20><color=#FFCC00>Arahkan kamera ke gambar sampul Buku Pariwisata</color></size>";
            }
            else
            {
                txtStatus = txtStatusT.GetComponent<TextMeshProUGUI>();
            }

            // 3. Dapatkan atau Buat KerangkaTangan_Overlay (Hand Skeleton Renderer)
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

            // 4. Dapatkan atau Buat Reticle_HandTracking (Label status di bawah tangan)
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
                reticleImage.color = Color.clear; // Transparan agar kotak pointer lama tidak muncul

                // Child Teks Status
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
                txtReticle.text = "✋ TANGAN TERLACAK";
            }
            else
            {
                reticleGO = reticleT.gameObject;
                reticleImage = reticleGO.GetComponent<UnityEngine.UI.Image>();
                reticleImage.color = Color.clear;
                Transform dot = reticleGO.transform.Find("CenterDot");
                if (dot != null) dot.gameObject.SetActive(false);
                txtReticle = reticleGO.GetComponentInChildren<TextMeshProUGUI>(true);
            }
            reticleGO.SetActive(false);

            // 5. Dapatkan atau Buat Panel_BottomControls (Balikkan, Reset & Auto Putar)
            Transform bottomControlsT = canvasHUDGO.transform.Find("Panel_BottomControls");
            GameObject bottomControlsGO;
            Button btnReset = null;
            Button btnAutoPutar = null;
            if (bottomControlsT == null)
            {
                bottomControlsGO = new GameObject("Panel_BottomControls", typeof(RectTransform));
                bottomControlsGO.transform.SetParent(canvasHUDGO.transform, false);

                RectTransform rect = bottomControlsGO.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(0f, 40f);
                rect.sizeDelta = new Vector2(480f, 80f);

                HorizontalLayoutGroup layout = bottomControlsGO.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 20f;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = false;
                layout.childControlHeight = false;

                // Btn_Reset
                GameObject btnResetGO = new GameObject("Btn_Reset", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(Button));
                btnResetGO.transform.SetParent(bottomControlsGO.transform, false);
                btnResetGO.GetComponent<RectTransform>().sizeDelta = new Vector2(210f, 65f);
                btnResetGO.GetComponent<UnityEngine.UI.Image>().color = new Color(0.2f, 0.4f, 0.8f, 0.9f);
                btnReset = btnResetGO.GetComponent<Button>();

                GameObject txtResetGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                txtResetGO.transform.SetParent(btnResetGO.transform, false);
                RectTransform tr = txtResetGO.GetComponent<RectTransform>();
                tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.sizeDelta = Vector2.zero;
                TextMeshProUGUI txtR = txtResetGO.GetComponent<TextMeshProUGUI>();
                txtR.fontSize = 17; txtR.fontStyle = FontStyles.Bold; txtR.alignment = TextAlignmentOptions.Center;
                txtR.text = "RESET POSISI"; txtR.color = Color.white;

                // Btn_AutoPutar
                GameObject btnAutoGO = new GameObject("Btn_AutoPutar", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(Button));
                btnAutoGO.transform.SetParent(bottomControlsGO.transform, false);
                btnAutoGO.GetComponent<RectTransform>().sizeDelta = new Vector2(210f, 65f);
                btnAutoGO.GetComponent<UnityEngine.UI.Image>().color = new Color(0.15f, 0.65f, 0.4f, 0.9f);
                btnAutoPutar = btnAutoGO.GetComponent<Button>();

                GameObject txtAutoGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                txtAutoGO.transform.SetParent(btnAutoGO.transform, false);
                RectTransform ta = txtAutoGO.GetComponent<RectTransform>();
                ta.anchorMin = Vector2.zero; ta.anchorMax = Vector2.one; ta.sizeDelta = Vector2.zero;
                TextMeshProUGUI txtA = txtAutoGO.GetComponent<TextMeshProUGUI>();
                txtA.fontSize = 17; txtA.fontStyle = FontStyles.Bold; txtA.alignment = TextAlignmentOptions.Center;
                txtA.text = "AUTO-PUTAR"; txtA.color = Color.white;
            }
            else
            {
                bottomControlsGO = bottomControlsT.gameObject;
                btnReset = bottomControlsGO.transform.Find("Btn_Reset")?.GetComponent<Button>();
                btnAutoPutar = bottomControlsGO.transform.Find("Btn_AutoPutar")?.GetComponent<Button>();
            }

            // 6. Hubungkan ke ManajerScene
            GameObject manajerGO = GameObject.Find("ManajerScene");
            if (manajerGO != null)
            {
                PelacakTanganKameraAR pelacak = manajerGO.GetComponent<PelacakTanganKameraAR>();
                if (pelacak == null) pelacak = manajerGO.AddComponent<PelacakTanganKameraAR>();
                pelacak.kerangkaTanganUI = kerangkaUI;

                // Set field via SerializedObject
                SerializedObject soPelacak = new SerializedObject(pelacak);
                soPelacak.FindProperty("reticleHandUI").objectReferenceValue = reticleGO.GetComponent<RectTransform>();
                soPelacak.FindProperty("reticleImage").objectReferenceValue = reticleImage;
                soPelacak.FindProperty("txtReticleStatus").objectReferenceValue = txtReticle;
                soPelacak.ApplyModifiedProperties();

                PengaturARObjectRecognition pengatur = manajerGO.GetComponent<PengaturARObjectRecognition>();
                if (pengatur != null)
                {
                    SerializedObject soPengatur = new SerializedObject(pengatur);
                    soPengatur.FindProperty("txtStatus").objectReferenceValue = txtStatus;
                    soPengatur.FindProperty("btnReset").objectReferenceValue = btnReset;
                    soPengatur.FindProperty("btnAutoPutar").objectReferenceValue = btnAutoPutar;
                    soPengatur.FindProperty("bottomControlsPanel").objectReferenceValue = bottomControlsGO;
                    soPengatur.ApplyModifiedProperties();
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[SetupARHUD] SUKSES! Canvas HUD & Kerangka Tangan selesai dikonfigurasi dan disimpan ke Scene_AR.");
        }
    }
}
#endif
