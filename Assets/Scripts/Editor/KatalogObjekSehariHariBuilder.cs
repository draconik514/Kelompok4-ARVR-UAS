#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Vuforia;
using Kelompok4.AR;

namespace Kelompok4.AR.Editor
{
    /// <summary>
    /// KatalogObjekSehariHariBuilder:
    /// Mengonfigurasi ARCamera anti-layar-hitam + ke-5 ImageTarget Vuforia dari DB_Kelompok4
    /// beserta 3D Digital Twin proporsional & BoxCollider Tap-to-Inspect.
    /// </summary>
    public static class KatalogObjekSehariHariBuilder
    {
        [MenuItem("AR/Setup Semua 5 Target Vuforia & Objek 3D Nyata")]
        public static void BangunKatalogObjek()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "Scene_AR")
            {
                if (File.Exists("Assets/Scenes/Scene_AR.unity"))
                {
                    scene = EditorSceneManager.OpenScene("Assets/Scenes/Scene_AR.unity", OpenSceneMode.Single);
                }
                else
                {
                    return;
                }
            }

            // 0. Konfigurasi ARCamera agar bebas layar hitam di Android OpenGL ES (Mali/Adreno) & non-ARCore
            GameObject arCamGO = GameObject.Find("ARCamera");
            if (arCamGO != null)
            {
                Camera cam = arCamGO.GetComponent<Camera>();
                if (cam != null)
                {
                    cam.allowMSAA = false;
                    cam.allowHDR = false;
                    cam.useOcclusionCulling = false;
                    cam.nearClipPlane = 0.015f;
                    cam.farClipPlane = 50f;
                    EditorUtility.SetDirty(cam);
                }

                VuforiaBehaviour vb = arCamGO.GetComponent<VuforiaBehaviour>();
                if (vb != null)
                {
                    SerializedObject sovb = new SerializedObject(vb);
                    var propWcm = sovb.FindProperty("mWorldCenterMode");
                    if (propWcm != null) propWcm.intValue = 1; // FIRST_TARGET (optimal tanpa ARCore)
                    sovb.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            string[] texPaths =
            {
                "Assets/StreamingAssets/Vuforia/DB_Kelompok4.xml",
                "Assets/StreamingAssets/Vuforia/DB_Kelompok4.dat",
                "Assets/Textures/Tex_Sampul_BukuCampus.jpg",
                "Assets/Textures/Tex_Kalkulator_LCD.jpg",
                "Assets/Textures/Tex_Penghapus_Sleeve.jpg"
            };
            foreach (string tp in texPaths)
            {
                if (File.Exists(tp))
                {
                    AssetDatabase.ImportAsset(tp, ImportAssetOptions.ForceSynchronousImport);
                }
            }

            GameObject baseTarget = GameObject.Find("ImageTarget");
            if (baseTarget == null)
            {
                Debug.LogError("[KatalogBuilder] ImageTarget utama tidak ditemukan di Scene_AR!");
                return;
            }

            // Bersihkan objek prosedural lama di bawah ImageTarget utama
            string[] oldNames = { "Pulpen_3D", "Botol_Minum_3D", "Smartphone_3D" };
            foreach (string old in oldNames)
            {
                Transform oldT = baseTarget.transform.Find(old);
                if (oldT != null)
                {
                    Object.DestroyImmediate(oldT.gameObject);
                }
            }

            Transform oldCanvas = baseTarget.transform.Find("Canvas");
            if (oldCanvas != null)
            {
                oldCanvas.gameObject.SetActive(false);
            }

            // 1. TARGET 1: Buku_Pariwisata (0.20 x 0.266667)
            KonfigurasiVuforiaTarget(baseTarget, "Buku_Pariwisata", 0.20f, 0.266667f, Vector3.zero);
            Transform bukuUtama = baseTarget.transform.Find("Buku_Virtual_3D");
            if (bukuUtama != null)
            {
                bukuUtama.gameObject.SetActive(true);
                bukuUtama.localPosition = new Vector3(0f, 0.045f, 0f);
                bukuUtama.localScale = new Vector3(0.16f, 0.024f, 0.21f);
                if (bukuUtama.GetComponent<BoxCollider>() == null)
                {
                    bukuUtama.gameObject.AddComponent<BoxCollider>();
                }
                if (bukuUtama.GetComponent<InteraksiBukuAR>() == null)
                {
                    bukuUtama.gameObject.AddComponent<InteraksiBukuAR>();
                }
            }

            // 2. Siapkan Material PBR
            string matDir = "Assets/Materials";
            if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);
            Shader shaderLit = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");

            Material matBukuCampus = BuatAtauMuatMaterial("Assets/Materials/Mat_BukuCampus.mat", shaderLit,
                Color.white, 0.45f, 0.05f, "Assets/Textures/Tex_Sampul_BukuCampus.jpg");
            Material matKertas = BuatAtauMuatMaterial("Assets/Materials/Mat_KertasPutih.mat", shaderLit,
                new Color(0.96f, 0.95f, 0.92f), 0.15f, 0.0f, null);
            Material matSpine = BuatAtauMuatMaterial("Assets/Materials/Mat_SpineHitam.mat", shaderLit,
                new Color(0.10f, 0.10f, 0.12f), 0.30f, 0.10f, null);

            Material matCalcBody = BuatAtauMuatMaterial("Assets/Materials/Mat_KalkulatorBody.mat", shaderLit,
                new Color(0.16f, 0.18f, 0.21f), 0.45f, 0.15f, null);
            Material matCalcKey = BuatAtauMuatMaterial("Assets/Materials/Mat_KalkulatorKey.mat", shaderLit,
                new Color(0.28f, 0.31f, 0.35f), 0.50f, 0.10f, null);
            Material matCalcOrange = BuatAtauMuatMaterial("Assets/Materials/Mat_KalkulatorOrange.mat", shaderLit,
                new Color(0.90f, 0.38f, 0.15f), 0.55f, 0.10f, null);
            Material matCalcLCD = BuatAtauMuatMaterial("Assets/Materials/Mat_KalkulatorLCD.mat", shaderLit,
                Color.white, 0.82f, 0.05f, "Assets/Textures/Tex_Kalkulator_LCD.jpg");

            Material matEraserRubber = BuatAtauMuatMaterial("Assets/Materials/Mat_PenghapusKaret.mat", shaderLit,
                new Color(0.14f, 0.15f, 0.17f), 0.25f, 0.0f, null);
            Material matEraserSleeve = BuatAtauMuatMaterial("Assets/Materials/Mat_PenghapusSleeve.mat", shaderLit,
                Color.white, 0.35f, 0.0f, "Assets/Textures/Tex_Penghapus_Sleeve.jpg");

            Material matPenBarrel = BuatAtauMuatMaterial("Assets/Materials/Mat_PenBody.mat", shaderLit,
                new Color(0.20f, 0.23f, 0.27f), 0.90f, 0.35f, null);
            Material matPenGrip = BuatAtauMuatMaterial("Assets/Materials/Mat_PenGrip.mat", shaderLit,
                new Color(0.11f, 0.12f, 0.14f), 0.25f, 0.05f, null);
            Material matSilver = BuatAtauMuatMaterial("Assets/Materials/Mat_SilverMetal.mat", shaderLit,
                new Color(0.86f, 0.89f, 0.93f), 0.92f, 0.95f, null);

            // Material khusus Ruang Studio VR disimpan di Assets/Resources agar 100% tidak ter-strip saat Build APK Android
            if (!Directory.Exists("Assets/Resources")) Directory.CreateDirectory("Assets/Resources");
            Shader shaderUnlit = Shader.Find("Unlit/Color") ?? shaderLit;
            BuatAtauMuatMaterial("Assets/Resources/Mat_VRRoomWall.mat", shaderUnlit, new Color(0.05f, 0.09f, 0.18f), 0.1f, 0f, null);
            BuatAtauMuatMaterial("Assets/Resources/Mat_VRRoomFloor.mat", shaderUnlit, new Color(0.03f, 0.05f, 0.11f), 0.1f, 0f, null);
            BuatAtauMuatMaterial("Assets/Resources/Mat_VRNeonGrid.mat", shaderUnlit, new Color(0.0f, 0.78f, 1.0f), 0.5f, 0f, null);
            BuatAtauMuatMaterial("Assets/Resources/Mat_VRPodium.mat", shaderLit, new Color(0.09f, 0.24f, 0.42f), 0.7f, 0.4f, null);

            // 3. Bangun 4 ImageTarget Lainnya dengan Skala Proporsional (agar kamera di sekeliling benda tetap luas terlihat!)
            GameObject itCampus = DapatkanAtauKlonTarget(baseTarget, "ImageTarget_BukuCampus", "2_Buku_Campus", 0.18f, 0.161716f, new Vector3(0.28f, 0f, 0f));
            BangunModelBukuCampus(itCampus.transform, matBukuCampus, matKertas, matSpine);

            GameObject itCalc = DapatkanAtauKlonTarget(baseTarget, "ImageTarget_Kalkulator", "3_Kalkulator_Citizen", 0.10f, 0.10f, new Vector3(0.50f, 0f, 0f));
            BangunModelKalkulatorCitizen(itCalc.transform, matCalcBody, matCalcKey, matCalcOrange, matCalcLCD);

            GameObject itEraser = DapatkanAtauKlonTarget(baseTarget, "ImageTarget_Penghapus", "4_Penghapus_Faber", 0.06f, 0.06f, new Vector3(-0.22f, 0f, 0f));
            BangunModelPenghapusFaber(itEraser.transform, matEraserRubber, matEraserSleeve);

            GameObject itPen = DapatkanAtauKlonTarget(baseTarget, "ImageTarget_Pulpen", "1_Pulpen_Pilot", 0.15f, 0.15f, new Vector3(-0.44f, 0f, 0f));
            BangunModelPulpenPilot(itPen.transform, matPenBarrel, matPenGrip, matSilver);

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[KatalogBuilder] SUKSES! ARCamera anti-hitam + 5 Target Vuforia & 3D Digital Twins proporsional siap digunakan!");
        }

        private static Material BuatAtauMuatMaterial(string path, Shader shader, Color color, float smoothness, float metallic, string texPath)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.shader = shader;
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);

            if (!string.IsNullOrEmpty(texPath))
            {
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                if (tex != null)
                {
                    mat.mainTexture = tex;
                    if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                }
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static GameObject DapatkanAtauKlonTarget(GameObject templateTarget, string goName, string trackableName, float width, float height, Vector3 editorPos)
        {
            GameObject existing = GameObject.Find(goName);
            if (existing == null)
            {
                existing = Object.Instantiate(templateTarget);
                existing.name = goName;
            }

            for (int i = existing.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(existing.transform.GetChild(i).gameObject);
            }

            KonfigurasiVuforiaTarget(existing, trackableName, width, height, editorPos);
            return existing;
        }

        private static void KonfigurasiVuforiaTarget(GameObject targetGO, string trackableName, float width, float height, Vector3 editorPos)
        {
            targetGO.transform.SetParent(null);
            targetGO.transform.localPosition = editorPos;
            targetGO.transform.localRotation = Quaternion.identity;
            targetGO.transform.localScale = Vector3.one;

            ImageTargetBehaviour itb = targetGO.GetComponent<ImageTargetBehaviour>();
            if (itb != null)
            {
                SerializedObject so = new SerializedObject(itb);
                SetPropString(so, "mTrackableName", trackableName);
                SetPropString(so, "mDataSetPath", "Vuforia/DB_Kelompok4.xml");
                SetPropBool(so, "mInitializedInEditor", true);
                SetPropInt(so, "mImageTargetType", 0);
                SetPropFloat(so, "mWidth", width);
                SetPropFloat(so, "mHeight", height);
                SetPropFloat(so, "mAspectRatio", width > 0.0001f ? (height / width) : 1f);
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            Component[] comps = targetGO.GetComponents<Component>();
            foreach (Component c in comps)
            {
                if (c != null && c.GetType().Name == "ImageTargetPreview")
                {
                    SerializedObject sop = new SerializedObject(c);
                    SetPropString(sop, "mTargetName", trackableName);
                    SetPropString(sop, "mDatasetName", "Vuforia/DB_Kelompok4.xml");
                    sop.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            DefaultObserverEventHandler handler = targetGO.GetComponent<DefaultObserverEventHandler>();
            if (handler != null)
            {
                handler.StatusFilter = DefaultObserverEventHandler.TrackingStatusFilter.Tracked_ExtendedTracked;
                SerializedObject soh = new SerializedObject(handler);
                SerializedProperty foundCalls = soh.FindProperty("OnTargetFound.m_PersistentCalls.m_Calls");
                if (foundCalls != null) foundCalls.ClearArray();
                SerializedProperty lostCalls = soh.FindProperty("OnTargetLost.m_PersistentCalls.m_Calls");
                if (lostCalls != null) lostCalls.ClearArray();
                soh.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetPropString(SerializedObject so, string name, string val)
        {
            var p = so.FindProperty(name);
            if (p != null) p.stringValue = val;
        }

        private static void SetPropBool(SerializedObject so, string name, bool val)
        {
            var p = so.FindProperty(name);
            if (p != null) p.boolValue = val;
        }

        private static void SetPropInt(SerializedObject so, string name, int val)
        {
            var p = so.FindProperty(name);
            if (p != null) p.intValue = val;
        }

        private static void SetPropFloat(SerializedObject so, string name, float val)
        {
            var p = so.FindProperty(name);
            if (p != null) p.floatValue = val;
        }

        private static GameObject BuatPart(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 euler, Vector3 scale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        private static void TambahTapCollider(GameObject root, Vector3 size)
        {
            BoxCollider bc = root.GetComponent<BoxCollider>();
            if (bc == null) bc = root.AddComponent<BoxCollider>();
            bc.center = Vector3.zero;
            bc.size = size;
        }

        private static void BangunModelBukuCampus(Transform parent, Material matCover, Material matPages, Material matSpine)
        {
            GameObject root = new GameObject("Buku_Campus_3D");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(0f, 0.04f, 0f);
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one * 0.82f;

            BuatPart(PrimitiveType.Cube, "SampulDepan", root.transform, new Vector3(0f, 0.0075f, 0f), new Vector3(0f, 180f, 0f), new Vector3(0.135f, 0.0022f, 0.175f), matCover);
            BuatPart(PrimitiveType.Cube, "SampulBelakang", root.transform, new Vector3(0f, -0.0075f, 0f), Vector3.zero, new Vector3(0.135f, 0.0022f, 0.175f), matCover);
            BuatPart(PrimitiveType.Cube, "LembarKertas", root.transform, new Vector3(0.002f, 0f, 0f), Vector3.zero, new Vector3(0.129f, 0.013f, 0.169f), matPages);
            BuatPart(PrimitiveType.Cube, "JilidHitam", root.transform, new Vector3(-0.0655f, 0f, 0f), Vector3.zero, new Vector3(0.006f, 0.0172f, 0.1755f), matSpine);

            TambahTapCollider(root, new Vector3(0.16f, 0.04f, 0.20f));
            root.AddComponent<InteraksiBukuAR>();
        }

        private static void BangunModelKalkulatorCitizen(Transform parent, Material matBody, Material matKey, Material matOrange, Material matLCD)
        {
            GameObject root = new GameObject("Kalkulator_Citizen_3D");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(0f, 0.032f, 0f);
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one * 0.68f;

            BuatPart(PrimitiveType.Cube, "BodiUtama", root.transform, new Vector3(0f, 0f, -0.016f), Vector3.zero, new Vector3(0.092f, 0.011f, 0.088f), matBody);

            GameObject head = BuatPart(PrimitiveType.Cube, "KepalaLayarMiring", root.transform, new Vector3(0f, 0.0065f, 0.043f), new Vector3(-18f, 0f, 0f), new Vector3(0.092f, 0.011f, 0.042f), matBody);
            BuatPart(PrimitiveType.Cube, "PanelLCD", head.transform, new Vector3(0f, 0.52f, 0f), new Vector3(0f, 180f, 0f), new Vector3(0.91f, 0.12f, 0.86f), matLCD);

            float startX = -0.0315f;
            float stepX = 0.021f;
            float startZ = 0.016f;
            float stepZ = -0.016f;

            for (int r = 0; r < 5; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (r == 4 && c == 2) continue;
                    bool isPlusTall = (r == 3 && c == 2);
                    bool isOrangeOn = (r == 0 && c == 3);

                    float posZ = startZ + r * stepZ + (isPlusTall ? (stepZ * 0.5f) : 0f);
                    float sizeZ = isPlusTall ? 0.027f : 0.0115f;
                    Material keyMat = isOrangeOn ? matOrange : matKey;

                    BuatPart(PrimitiveType.Cube, $"Key_{r}_{c}", root.transform,
                        new Vector3(startX + c * stepX, 0.007f, posZ),
                        Vector3.zero,
                        new Vector3(0.0165f, 0.0045f, sizeZ),
                        keyMat);
                }
            }

            TambahTapCollider(root, new Vector3(0.11f, 0.04f, 0.15f));
            root.AddComponent<InteraksiBukuAR>();
        }

        private static void BangunModelPenghapusFaber(Transform parent, Material matRubber, Material matSleeve)
        {
            GameObject root = new GameObject("Penghapus_Faber_3D");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(0f, 0.022f, 0f);
            root.transform.localRotation = Quaternion.Euler(0f, -25f, 0f);
            root.transform.localScale = Vector3.one * 0.72f;

            BuatPart(PrimitiveType.Cube, "KaretHitam", root.transform, Vector3.zero, Vector3.zero, new Vector3(0.062f, 0.0135f, 0.025f), matRubber);
            BuatPart(PrimitiveType.Cube, "SelongsongFaber", root.transform, new Vector3(0.008f, 0f, 0f), new Vector3(0f, 180f, 0f), new Vector3(0.040f, 0.015f, 0.0265f), matSleeve);

            TambahTapCollider(root, new Vector3(0.08f, 0.035f, 0.045f));
            root.AddComponent<InteraksiBukuAR>();
        }

        private static void BangunModelPulpenPilot(Transform parent, Material matBarrel, Material matGrip, Material matSilver)
        {
            GameObject root = new GameObject("Pulpen_Pilot_3D");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(0f, 0.03f, 0f);
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one * 0.85f;

            Vector3 rotHorizontal = new Vector3(0f, 0f, 90f);

            BuatPart(PrimitiveType.Cylinder, "TabungUtama", root.transform, new Vector3(0.012f, 0f, 0f), rotHorizontal, new Vector3(0.010f, 0.046f, 0.010f), matBarrel);
            BuatPart(PrimitiveType.Cylinder, "RubberGrip", root.transform, new Vector3(-0.033f, 0f, 0f), rotHorizontal, new Vector3(0.0116f, 0.020f, 0.0116f), matGrip);
            BuatPart(PrimitiveType.Cylinder, "ConeTip", root.transform, new Vector3(-0.058f, 0f, 0f), rotHorizontal, new Vector3(0.0072f, 0.008f, 0.0072f), matSilver);
            BuatPart(PrimitiveType.Cylinder, "MataPena", root.transform, new Vector3(-0.068f, 0f, 0f), rotHorizontal, new Vector3(0.0026f, 0.0045f, 0.0026f), matSilver);
            BuatPart(PrimitiveType.Cylinder, "BodiBelakang", root.transform, new Vector3(0.060f, 0f, 0f), rotHorizontal, new Vector3(0.0106f, 0.009f, 0.0106f), matGrip);
            BuatPart(PrimitiveType.Cylinder, "TombolTekan", root.transform, new Vector3(0.072f, 0f, 0f), rotHorizontal, new Vector3(0.0074f, 0.0055f, 0.0074f), matGrip);
            BuatPart(PrimitiveType.Cube, "KlipSaku", root.transform, new Vector3(0.044f, 0.0075f, 0f), Vector3.zero, new Vector3(0.036f, 0.0025f, 0.0042f), matGrip);

            TambahTapCollider(root, new Vector3(0.16f, 0.045f, 0.045f));
            root.AddComponent<InteraksiBukuAR>();
        }
    }
}
#endif
