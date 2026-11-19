#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Kelompok4.AR;

namespace Kelompok4.Editor
{
    public static class Buku3DBuilder
    {
        [MenuItem("Kelompok 4/Pasang Buku 3D Otomatis ke ImageTarget", false, 10)]
        public static void SetupBuku3D()
        {
            // Cari ImageTarget di Scene
            GameObject imageTarget = GameObject.Find("ImageTarget");
            if (imageTarget == null)
            {
                EditorUtility.DisplayDialog("Error", "ImageTarget tidak ditemukan di Hierarchy! Pastikan Scene_AR terbuka.", "OK");
                return;
            }

            // Hapus model lama (seperti Pohon atau Model_Objek3D) jika ada
            Transform oldTree = imageTarget.transform.Find("Pohon_Pariwisata");
            if (oldTree != null) Object.DestroyImmediate(oldTree.gameObject);

            Transform oldModel = imageTarget.transform.Find("Model_Objek3D");
            if (oldModel != null) Object.DestroyImmediate(oldModel.gameObject);

            Transform oldBook = imageTarget.transform.Find("Buku_Virtual_3D");
            if (oldBook != null) Object.DestroyImmediate(oldBook.gameObject);

            // Buat Objek Buku 3D Fisik Kembaran (Digital Twin)
            GameObject book3D = GameObject.CreatePrimitive(PrimitiveType.Cube);
            book3D.name = "Buku_Virtual_3D";
            book3D.transform.SetParent(imageTarget.transform);
            
            // Posisi melayang sedikit di atas marker dan berukuran proporsional dengan buku asli (20 x 26.6 cm, tebal 2 cm)
            book3D.transform.localPosition = new Vector3(0, 0.05f, 0);
            book3D.transform.localRotation = Quaternion.identity;
            book3D.transform.localScale = new Vector3(0.2f, 0.02f, 0.266f);

            // Pasang Tekstur Sampul Buku Pariwisata
            Texture2D bookCover = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Buku_Sampul.jpg");
            if (bookCover == null)
            {
                bookCover = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Vuforia/ImageTargetTextures/DB_Kelompok4/Buku_Pariwisata_scaled.jpg");
            }

            Material bookMat = new Material(Shader.Find("Standard"));
            if (bookCover != null)
            {
                bookMat.mainTexture = bookCover;
            }
            bookMat.color = Color.white;

            // Simpan material ke project
            AssetDatabase.CreateAsset(bookMat, "Assets/Materials/Mat_BukuSampul3D.mat");
            book3D.GetComponent<Renderer>().sharedMaterial = bookMat;

            // Tambahkan komponen efek rotasi berputar halus
            book3D.AddComponent<RotasiHologram>();

            // Sesuaikan posisi Canvas tombol MASUK MODE VR agar berada di atas buku 3D
            Transform canvas = imageTarget.transform.Find("Canvas");
            if (canvas != null)
            {
                canvas.localPosition = new Vector3(0, 0.15f, 0);
                canvas.localRotation = Quaternion.Euler(90, 0, 0);
            }

            EditorUtility.SetDirty(imageTarget);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Sukses!", 
                "Buku Pariwisata versi 3D Hologram berhasil dipasang tepat di atas ImageTarget!\n\nTekan PLAY untuk melihat kembaran 3D buku Anda melayang dan berputar di atas buku fisik.", "Keren!");
        }
    }
}
#endif
