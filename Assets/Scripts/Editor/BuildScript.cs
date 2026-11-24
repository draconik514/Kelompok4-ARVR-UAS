#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Kelompok4.AR.Editor
{
    public static class BuildScript
    {
        [MenuItem("AR/Build APK Android (5 Objek Nyata + Hand Tracking)")]
        public static void BuildAndroid()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                EditorApplication.delayCall += BuildAndroid;
                Debug.Log("[BuildScript] Menghentikan Play Mode terlebih dahulu sebelum Build APK...");
                return;
            }

            Debug.Log("[BuildScript] 1/3 Menyiapkan & menyimpan 5 ImageTarget Vuforia + 3D Digital Twins ke Scene_AR...");
            SetupARHUDHandTracking.ExecuteSetup();

            string buildDir = Path.Combine(Directory.GetCurrentDirectory(), "Builds");
            if (!Directory.Exists(buildDir))
            {
                Directory.CreateDirectory(buildDir);
            }

            string apkPath = Path.Combine(buildDir, "Kelompok4_ARVR.apk");

            // Pastikan Scene_AR masuk build
            string scenePath = "Assets/Scenes/Scene_AR.unity";
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };

            // Set Player Settings untuk Android ARM64
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            PlayerSettings.companyName = "Kelompok4";
            PlayerSettings.productName = "Kelompok4_ARVR";

            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.Kelompok4.ARVR");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new string[] { scenePath },
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            Debug.Log($"[BuildScript] 2/3 Mengompilasi APK Android ke: {apkPath} ...");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                double mb = Math.Round(summary.totalSize / (1024.0 * 1024.0), 2);
                Debug.Log($"[BuildScript] 3/3 BUILD APK SUKSES! Ukuran: {mb} MB. Lokasi: {apkPath}");
                EditorUtility.RevealInFinder(apkPath);
            }
            else
            {
                Debug.LogError($"[BuildScript] BUILD GAGAL! Status: {summary.result}, Total Error: {summary.totalErrors}");
                foreach (var step in report.steps)
                {
                    foreach (var msg in step.messages)
                    {
                        if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        {
                            Debug.LogError($"[BuildScript Detail] {msg.content}");
                        }
                    }
                }
            }
        }
    }
}
#endif
