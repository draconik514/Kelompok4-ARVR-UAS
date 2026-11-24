using System.Collections.Generic;
using UnityEngine;

namespace Kelompok4.AR
{
    [System.Serializable]
    public class InfoEdukasiObjek
    {
        public string targetId;
        public string judul;
        public string kategori;
        public string deskripsi;
        public string instruksiOverlay;
        public string spesifikasiSingkat;
    }

    /// <summary>
    /// Menyediakan basis data informasi edukasi AR & overlay instruksi untuk ke-5 objek nyata,
    /// serta membangun Ruang Studio Virtual 3D (Mode VR) + dukungan Split-Screen VR Box & Gyroscope.
    /// </summary>
    public static class DataInformasiObjekAR
    {
        private static readonly Dictionary<string, InfoEdukasiObjek> Database = new Dictionary<string, InfoEdukasiObjek>
        {
            {
                "Buku_Pariwisata",
                new InfoEdukasiObjek
                {
                    targetId = "Buku_Pariwisata",
                    judul = "Buku Pariwisata Pulau Togean",
                    kategori = "Literatur Ekowisata & Konservasi Bahari",
                    spesifikasiSingkat = "Penulis: Dr. Suaib Djafar | Lokasi: Kab. Tojo Una-Una, Sulawesi Tengah",
                    deskripsi = "Buku referensi kebijakan pengembangan ekowisata Taman Nasional Kepulauan Togean di Teluk Tomini. Mengulas kekayaan jantung segitiga terumbu karang dunia (Coral Triangle), habitat ubur-ubur tak menyengat Danau Mariona, serta pemberdayaan wisata bahari berkelanjutan.",
                    instruksiOverlay = "1. Ketuk objek buku 3D untuk membuka/menutup panel informasi AR.\n2. Angkat 1 tangan di depan kamera untuk memutar buku 360 derajat.\n3. Tekan tombol [MASUK MODE VR] untuk mengeksplorasi buku di Ruang Virtual."
                }
            },
            {
                "2_Buku_Campus",
                new InfoEdukasiObjek
                {
                    targetId = "2_Buku_Campus",
                    judul = "Buku Tulis iBook Campus (B5)",
                    kategori = "Perlengkapan Akademik Sehari-hari",
                    spesifikasiSingkat = "Ukuran: B5 (18 x 25 cm) | Kertas: HVS 70 gsm | Sampul: Tartan Merah",
                    deskripsi = "Buku catatan perkuliahan berformat B5 yang memberikan ruang tulis lebih luas dibanding buku kuarto standar. Dilengkapi sampul karton tebal berlaminasi corak tartan merah serta penjilidan punggung hitam yang kuat untuk penggunaan harian mahasiswa.",
                    instruksiOverlay = "1. Gunakan pada alas meja datar agar jilid buku tetap awet.\n2. Geser tangan kiri/kanan di depan kamera untuk melihat sisi depan & belakang.\n3. Tekan [MASUK MODE VR] untuk inspeksi detail 3D di Ruang Virtual."
                }
            },
            {
                "3_Kalkulator_Citizen",
                new InfoEdukasiObjek
                {
                    targetId = "3_Kalkulator_Citizen",
                    judul = "Kalkulator Citizen SDC-810BN",
                    kategori = "Alat Komputasi & Finansial Meja",
                    spesifikasiSingkat = "Layar: LCD 10-Digit Miring 18° | Daya: Dual Power (Solar + Baterai)",
                    deskripsi = "Kalkulator meja kompak 10-digit dengan sistem Dual Power yang menggabungkan sel surya (solar panel) untuk pencahayaan ruang terang dan baterai cadangan otomatis. Kepala layar dimiringkan 18 derajat untuk mencegah pantulan cahaya lampu.",
                    instruksiOverlay = "1. Tekan tombol oranye [ON/CE] untuk menyalakan atau menghapus hitungan.\n2. Gunakan [M+], [M-], dan [MRC] untuk menyimpan subtotal memori.\n3. Cubit jari (Pinch) di udara untuk membalik & melihat struktur bodi 3D."
                }
            },
            {
                "4_Penghapus_Faber",
                new InfoEdukasiObjek
                {
                    targetId = "4_Penghapus_Faber",
                    judul = "Penghapus Faber-Castell Dust-Free",
                    kategori = "Alat Tulis Koreksi Presisi",
                    spesifikasiSingkat = "Dimensi: 6.2 x 2.4 cm | Material: PVC Bebas Phthalate | Fitur: Dust-Free",
                    deskripsi = "Penghapus karet hitam berkualitas tinggi dengan formula Dust-Free: sisa karet hasil hapusan langsung menggumpal menjadi satu gulungan rapi sehingga meja dan kertas tetap bersih tanpa serpihan debu, serta lembut tidak merusak serat kertas.",
                    instruksiOverlay = "1. Pegang pada bagian selongsong karton pelindung agar tidak mudah patah.\n2. Gosok searah secara perlahan di atas goresan pensil grafit.\n3. Tekan [MASUK MODE VR] untuk memutar bentuk 3D dengan Hand Tracking."
                }
            },
            {
                "1_Pulpen_Pilot",
                new InfoEdukasiObjek
                {
                    targetId = "1_Pulpen_Pilot",
                    judul = "Pulpen Gel Pilot G-2 (0.5 mm)",
                    kategori = "Instrumen Tulis Mekanis (Retractable Gel Pen)",
                    spesifikasiSingkat = "Panjang: 14.3 cm | Mata Pena: Tungsten Carbide 0.5 mm | Tinta: Gel Pigmen",
                    deskripsi = "Pena mekanis tekan (retractable) dengan tinta gel berviskositas dinamis yang mengalir sangat halus tanpa macet. Dilengkapi bantalan pegangan karet berkontur (ergonomic rubber grip) di bagian depan untuk meredam kelelahan jari saat menulis lama.",
                    instruksiOverlay = "1. Tekan tombol atas (clicker) hingga berbunyi klik untuk mengeluarkan mata pena.\n2. Masukkan kembali mata pena setelah selesai agar tinta tidak mengering.\n3. Masuk ke [MODE VR] untuk mengamati silinder bodi, klip, dan ujung logam."
                }
            }
        };

        public static InfoEdukasiObjek AmbilInfo(string targetName)
        {
            if (!string.IsNullOrEmpty(targetName) && Database.TryGetValue(targetName, out InfoEdukasiObjek info))
            {
                return info;
            }

            return new InfoEdukasiObjek
            {
                targetId = targetName,
                judul = string.IsNullOrEmpty(targetName) ? "Objek 3D AR/VR" : targetName.Replace("_", " "),
                kategori = "Objek Sehari-hari Terlacak",
                spesifikasiSingkat = "Target Vuforia Aktif | Manipulasi 3D Hand Tracking",
                deskripsi = "Objek fisik berhasil dikenali secara optik oleh sistem Vuforia Engine dan direkonstruksi sebagai kembaran digital 3D interaktif.",
                instruksiOverlay = "1. Ketuk objek 3D untuk membuka detail informasi.\n2. Gerakkan tangan di depan kamera atau masuk ke Mode VR untuk memutar objek."
            };
        }

        /// <summary>
        /// Membangun Ruang Studio Virtual 3D (kubah ruang virtual + grid neon + podium hologram)
        /// yang mengelilingi kamera saat pengguna masuk ke Mode VR.
        /// </summary>
        public static GameObject BangunRuangStudioVR(Transform camTransform)
        {
            GameObject vrRoot = new GameObject("VR_Studio_Environment");
            vrRoot.transform.SetParent(camTransform, false);
            vrRoot.transform.localPosition = Vector3.zero;
            vrRoot.transform.localRotation = Quaternion.identity;

            // Muat material dari Resources (100% aman dari shader stripping pada APK Android)
            Shader fallbackShader = Shader.Find("Standard");
            Material matRoomWall = Resources.Load<Material>("Mat_VRRoomWall") ?? new Material(fallbackShader) { color = new Color(0.05f, 0.09f, 0.18f) };
            Material matFloor = Resources.Load<Material>("Mat_VRRoomFloor") ?? new Material(fallbackShader) { color = new Color(0.03f, 0.05f, 0.11f) };
            Material matGrid = Resources.Load<Material>("Mat_VRNeonGrid") ?? new Material(fallbackShader) { color = new Color(0.0f, 0.78f, 1.0f) };
            Material matPodium = Resources.Load<Material>("Mat_VRPodium") ?? new Material(fallbackShader) { color = new Color(0.09f, 0.24f, 0.42f) };

            BuatDindingRuang(vrRoot.transform, "BackWall", new Vector3(0f, 0f, 4.5f), new Vector3(12f, 8f, 0.1f), matRoomWall);
            BuatDindingRuang(vrRoot.transform, "LeftWall", new Vector3(-4.5f, 0f, 1.5f), new Vector3(0.1f, 8f, 10f), matRoomWall);
            BuatDindingRuang(vrRoot.transform, "RightWall", new Vector3(4.5f, 0f, 1.5f), new Vector3(0.1f, 8f, 10f), matRoomWall);
            BuatDindingRuang(vrRoot.transform, "Ceiling", new Vector3(0f, 3.2f, 1.5f), new Vector3(12f, 0.1f, 10f), matRoomWall);
            BuatDindingRuang(vrRoot.transform, "Floor", new Vector3(0f, -0.55f, 1.5f), new Vector3(12f, 0.05f, 10f), matFloor);

            // 2. Garis-Garis Cyber Grid Holografik di Lantai Ruang VR
            for (int i = -6; i <= 6; i++)
            {
                BuatDindingRuang(vrRoot.transform, $"GridZ_{i}", new Vector3(i * 0.28f, -0.52f, 1.6f), new Vector3(0.006f, 0.003f, 5.5f), matGrid);
            }
            for (int j = 1; j <= 14; j++)
            {
                BuatDindingRuang(vrRoot.transform, $"GridX_{j}", new Vector3(0f, -0.52f, j * 0.30f), new Vector3(3.6f, 0.003f, 0.006f), matGrid);
            }

            // 3. Podium Hologram Melingkar di Bawah Objek Virtual
            GameObject podium = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            podium.name = "Podium_VR";
            Object.Destroy(podium.GetComponent<Collider>());
            podium.transform.SetParent(vrRoot.transform, false);
            podium.transform.localPosition = new Vector3(0f, -0.17f, 0.42f);
            podium.transform.localScale = new Vector3(0.26f, 0.008f, 0.26f);
            podium.GetComponent<Renderer>().sharedMaterial = matPodium;

            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "NeonRing_VR";
            Object.Destroy(ring.GetComponent<Collider>());
            ring.transform.SetParent(vrRoot.transform, false);
            ring.transform.localPosition = new Vector3(0f, -0.162f, 0.42f);
            ring.transform.localScale = new Vector3(0.28f, 0.003f, 0.28f);
            ring.GetComponent<Renderer>().sharedMaterial = matGrid;

            // 4. Lampu Studio VR agar seluruh sisi objek 3D tampak terang & tajam
            GameObject lightGO = new GameObject("VR_StudioLight");
            lightGO.transform.SetParent(vrRoot.transform, false);
            lightGO.transform.localPosition = new Vector3(0.2f, 0.4f, 0.1f);
            lightGO.transform.localRotation = Quaternion.Euler(35f, -20f, 0f);
            Light l = lightGO.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = 4.0f;
            l.intensity = 1.4f;
            l.color = new Color(0.92f, 0.97f, 1.0f);

            vrRoot.SetActive(false);
            return vrRoot;
        }

        private static void BuatDindingRuang(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
        }
    }
}
