# LAPORAN TUGAS BESAR (PROJECT-BASED)
# MATA KULIAH TEKNOLOGI AUGMENTED REALITY & VIRTUAL REALITY
## TEMA: OBJECT RECOGNITION (AR) + HAND TRACKING (VR)
### "Pengembangan Aplikasi Edukasi Buku Pariwisata 3D Berbasis Deteksi Objek Optik dan Manipulasi Virtual Hand Tracking Ergonomis"

---

### IDENTITAS KELOMPOK (KELOMPOK 4)

| No | Nama Mahasiswa | NIM | Peran Utama & Tanggung Jawab |
|:--:|:---|:---:|:---|
| 1 | **Abu Bakar Shidiq** | *(Isi NIM)* | **Project Leader, AR Specialist, 3D Artist, Documentation Lead, QA Tester** |
| 2 | **Kavrian Syah** | *(Isi NIM)* | **Lead Developer, VR & Tracking Specialist, UI/UX Designer, DevOps/Git, Demo Lead** |

#### Distribusi 10 Peran Kerja Kelompok (Sesuai Aturan Silabus UAS):
1. **Project Leader:** Abu Bakar Shidiq *(Perancangan roadmap, manajemen integrasi modul, dan kepatuhan rubrik)*
2. **Lead Developer:** Kavrian Syah *(Penyusunan arsitektur skrip C#, integrasi pipeline tracking, dan event handling)*
3. **AR Specialist:** Abu Bakar Shidiq *(Registrasi lisensi Vuforia Engine 11.4, Target Database DB_Kelompok4, dan tracking plane)*
4. **VR Specialist:** Kavrian Syah *(Pengembangan skeletal landmark tracking, orientasi view-space, dan gesture detection)*
5. **UI/UX Designer:** Kavrian Syah *(Perancangan HUD Canvas, reticle pointer dinamis, dan layout tombol kontrol layar)*
6. **3D Artist:** Abu Bakar Shidiq *(Pembuatan Digital Twin 3D Buku Pariwisata, texturing sampul resolusi tinggi, dan orientasi model)*
7. **QA/Tester:** Abu Bakar Shidiq *(Pengujian deteksi kamera fisik, robustness target under poor lighting, dan screen scanning)*
8. **DevOps & Cloud:** Kavrian Syah *(Version control GitHub LFS, pipeline multi-commit, dan deployment APK Android)*
9. **Documentation:** Abu Bakar Shidiq *(Penyusunan laporan komprehensif, evaluasi SSQ Kennedy, dan pencatatan Prompt Log)*
10. **Presenter/Demo:** Bersama (Abu Bakar Shidiq & Kavrian Syah) *(Uji fungsi langsung di hadapan dosen penguji)*

---

## 1. ABSTRAK
Proyek berbasis proyek (*Project-Based Learning*) ini mengembangkan aplikasi integrasi *Augmented Reality* (AR) dan *Virtual Reality* (VR) pada platform mobile Android. Pada modul AR, sistem melakukan pengenalan objek optik (*Object Recognition*) terhadap sampul fisik "Buku Pariwisata Togean" menggunakan *Vuforia Engine* dengan basis data lokal (*Device Database*), yang memproyeksikan kembaran digital 3D (*Digital Twin*) melayang beserta antarmuka instruksi adaptif. Pada modul VR dan interaksi, sistem memanfaatkan *Optical Hand Tracking* berbasis *MediaPipe* (21 titik landmark sendi tangan) yang disaring menggunakan algoritma *OneEuroFilter2D* adaptif untuk mengeliminasi getaran (*jitter*). Pengguna dapat memutar objek 360 derajat di udara, membalikkan objek secara instan menggunakan gestur mencubit (*pinch gesture*), maupun mengaktifkan auto-rotasi dan multi-objek katalog wisata tanpa memerlukan alat kendali fisik (*controller-free*). Pengujian kenyamanan pengguna diukur menggunakan *Simulator Sickness Questionnaire* (SSQ) dengan perolehan skor total keparahan sebesar 16.46 (kategori sangat nyaman dan aman), serta performa stabil pada kisaran 30–60 FPS pada pengujian perangkat mobile Android.

**Kata Kunci:** *Augmented Reality, Virtual Reality, Object Recognition, Vuforia Engine, MediaPipe Hand Tracking, OneEuroFilter, Digital Twin.*

---

## 2. TUJUAN & RUANG LINGKUP (SCOPE)

### 2.1 Tujuan Proyek
1. Mengembangkan sistem pengenalan objek optik (*Object Recognition*) yang presisi dan cepat pada media buku fisik nyata tanpa ketergantungan koneksi internet (*offline device database*).
2. Mengimplementasikan manipulasi objek 3D interaktif di ruang virtual menggunakan pelacakan tangan alami (*Hand Tracking*) bebas pengendali fisik.
3. Mengurangi kelelahan fisik (*shoulder fatigue*) dan getaran pelacakan (*noise jitter*) melalui penerapan algoritma filtrasi sinyal adaptif (*OneEuroFilter2D*).
4. Menyediakan antarmuka pengguna (HUD) yang informatif dengan umpan balik visual terhadap status deteksi objek dan status pelacakan sendi tangan.
5. Memenuhi seluruh rubrik penilaian Ujian Akhir Semester (UAS) mata kuliah Teknologi AR & VR.

### 2.2 Ruang Lingkup Proyek
* **Target Fisik:** Buku "Kebijakan Pengembangan Pariwisata Pulau Togean Kabupaten Tojo Una-Una Provinsi Sulawesi Tengah" (rasio dimensi cetak 0.2 m x 0.266 m).
* **Target Pengguna:** Mahasiswa, edukator, dan wisatawan yang ingin mengeksplorasi informasi pariwisata interaktif.
* **Perangkat Keras Target:** Smartphone Android (diuji pada Redmi 9 / Helio G80 ARM64, Android 10/11) dan laptop workstation pengembang.
* **Lingkungan Pengembangan:** Unity 6 (versi 6000.6.9f1 Long Term Support).

---

## 3. METODE & ALAT PENGEMBANGAN (TEKNIS)

### 3.1 Perangkat Lunak dan SDK
1. **Game Engine:** Unity 6 (6000.6.9f1) dengan Universal Render Pipeline (URP) dan Canvas Overlay System.
2. **AR SDK:** PTC Vuforia Engine versi 11.4.4 (menggunakan fitur *Image Target Feature Extraction* & *Offline Device Database*).
3. **Tracking Framework:** MediaPipe Unity Plugin versi 0.16.3 (berbasis model deep learning *HandLandmarker* 21 keypoints 3D).
4. **Algoritma Filter:** *1€ Filter (OneEuroFilter2D)* — filter lolos rendah (*low-pass filter*) dengan frekuensi cutoff adaptif berdasarkan kecepatan gerak.
5. **Version Control:** Git & Git LFS (Large File Storage) yang di-hosting pada GitHub.
6. **Compiler & Build Tools:** OpenJDK 17, Android NDK r23b/r25, Android SDK Platform 34/36/37, arsitektur target ARM64 (`arm64-v8a`).

### 3.2 Spesifikasi Perangkat Keras Pengujian
* **Perangkat Pengembang 1 (Abu):** PC Windows 11, RAM 8 GB, Integrated Graphics, HD Webcam.
* **Perangkat Pengembang 2 (Kavri):** Laptop Windows 11, Dedicated GPU, Android Build Pipeline.
* **Smartphone Target:** Xiaomi Redmi 9 (MediaTek Helio G80 Octa-Core ARM64, RAM 4 GB, Kamera 13 MP, OS Android 10/11, Hardware Gyroscope & Accelerometer).

---

## 4. ARSITEKTUR SISTEM & DIAGRAM ALUR (FLOW)

### 4.1 Diagram Alur Kerja Sistem (Pipeline Flow)

```
       [ KAMERA SMARTPHONE AKTIF ]
                    │
                    ▼
       ┌─────────────────────────┐
       │   VUFORIA ENGINE 11.4   │
       │ (Deteksi Feature Points)│
       └────────────┬────────────┘
                    │
           Target Terdeteksi?
          ├── TIDAK ──> Tampilkan Status: "Arahkan Kamera ke Buku Pariwisata"
          │
          └── YA (Target: Buku_Pariwisata)
                    │
                    ▼
       ┌─────────────────────────┐
       │   SPAWN 3D DIGITAL TWIN │
       │  (Buku Pariwisata 3D)   │
       └────────────┬────────────┘
                    │
                    ▼
       ┌─────────────────────────┐
       │ MEDIAPIPE HAND TRACKING │
       │  (21 Keypoints Sendi)   │
       └────────────┬────────────┘
                    │
           Tangan Terdeteksi?
          ├── TIDAK ──> Mode Touch/Auto-Putar: Pengguna dapat swipe layar
          │
          └── YA ──> [ Algoritma OneEuroFilter2D (Anti-Jitter) ]
                    │
                    ├── Gerak Kiri / Kanan  ──> Rotasi Horizontal (Yaw)
                    ├── Gerak Atas / Bawah  ──> Rotasi Vertikal / Flip (Pitch)
                    └── Gestur Mencubit     ──> Pengali Kecepatan Putar (Fast Flip)
```

### 4.2 Arsitektur Komponen Kode C#
Sistem dibangun secara modular dengan memisahkan logika pengenalan citra, pemrosesan sinyal pelacakan, dan interaksi visual:
1. **`PengaturARObjectRecognition.cs`:** *Central Coordinator* yang menangani event Vuforia (`OnTargetFound` / `OnTargetLost`), siklus hidup UI Canvas HUD, dan katalog pergantian objek.
2. **`PelacakTanganKameraAR.cs`:** *Vision Processor* yang menangani ekstraksi frame kamera, inferensi MediaPipe HandLandmarker, perhitungan gestur cubitan (*pinch detection*), serta normalisasi koordinat delta.
3. **`InteraksiBukuAR.cs`:** *Kinematic Manipulator* yang menerima input vektor gerak dan memanipulasi rotasi objek 3D secara *View-Space Relative* dengan peredaman inersia (*damping momentum*).
4. **`KerangkaTanganUI.cs`:** *Visual Feedback Renderer* yang menggambarkan visualisasi kerangka tulang sendi (21 titik landmark) pada Canvas HUD.

---

## 5. IMPLEMENTASI & HIGHLIGHT ALGORITMA

### 5.1 Algoritma OneEuroFilter2D (Anti-Jitter & Responsif Cepat)
Salah satu tantangan terbesar pelacakan tangan optik menggunakan kamera ponsel adalah *noise jitter* (titik sendi bergetar saat tangan diam) dan *lag latensi* (respon lambat saat tangan digerakkan cepat). Proyek ini mengimplementasikan algoritma filter industri **OneEuroFilter**:

```csharp
public class OneEuroFilter2D
{
    private float minCutoff; // Mematikan jitter pada kecepatan rendah
    private float beta;      // Meningkatkan responsivitas pada akselerasi cepat
    private float dCutoff;
    private Vector2 xPrev, dxPrev;
    private float tPrev;

    public Vector2 Filter(Vector2 x, float rate = 30f)
    {
        // 1. Hitung turunan perubahan gerak (derivatives)
        Vector2 dx = (x - xPrev) * rate;
        Vector2 edx = LowPass(dx, dxPrev, Alpha(rate, dCutoff));
        dxPrev = edx;

        // 2. Cutoff dinamis: Kecepatan tinggi -> cutoff membesar -> zero lag
        float cutoff = minCutoff + beta * edx.magnitude;
        Vector2 result = LowPass(x, xPrev, Alpha(rate, cutoff));
        xPrev = result;
        return result;
    }
}
```
*Hasil Implementasi:* Objek buku 3D tidak lagi mengalami getaran mikro saat telapak tangan berada diam di depan kamera, namun langsung bergerak mulus saat tangan digeser secara cepat.

### 5.2 Interaksi Relatif Terhadap Bidang Pandang Kamera (View-Space Rotation)
Untuk memastikan rotasi objek selalu selaras dengan pandangan pengguna di layar smartphone (bukan mengacu pada orientasi global dunia atau orientasi buku yang miring di meja), rotasi dihitung berdasarkan vektor kamera:
```csharp
Vector3 axisYaw = mainCamera.transform.up;      // Sumbu vertikal layar HP
Vector3 axisPitch = mainCamera.transform.right; // Sumbu horizontal layar HP

float rotY = -delta.x * speed; // Tangan ke kiri/kanan memutar buku secara horizontal
float rotX = delta.y * speed;  // Tangan ke atas/bawah membalikkan buku secara vertikal

transform.Rotate(axisYaw, rotY, Space.World);
transform.Rotate(axisPitch, rotX, Space.World);
```

### 5.3 Deteksi Gestur Cubitan (Pinch Gesture)
Gestur cubitan dideteksi dengan mengukur jarak Euclidean normalisasi antara ujung ibu jari (*Thumb Tip - Landmark 4*) dan ujung jari telunjuk (*Index Tip - Landmark 8*):
$$\text{Dist}_{\text{pinch}} = \sqrt{(x_4 - x_8)^2 + (y_4 - y_8)^2 + (z_4 - z_8)^2}$$
Jika $\text{Dist}_{\text{pinch}} < 0.055$, status `isPinching` bernilai aktif dan melipatgandakan kecepatan putaran hingga $2.2\times$ untuk menghasilkan efek membalik buku secara cepat (*fast page/book flip*).

---

## 6. PENGUJIAN & ANALISIS HASIL

### 6.1 Pengujian Simulator Sickness Questionnaire (SSQ)
Pengujian kenyamanan interaksi virtual dilakukan mengacu pada standar kuesioner **Kennedy SSQ (1993)** terhadap 5 responden setelah menggunakan aplikasi selama 10 menit.

| No | Kategori Gejala | Rata-Rata Skor Gejala | Bobot Pengali Kennedy | Skor Tertimbang |
|:--:|:---|:---:|:---:|:---:|
| 1 | **Nausea (Mual & Gangguan Lambung)** | 0.80 | $\times 9.54$ | **7.63** |
| 2 | **Oculomotor (Ketegangan Mata & Fokus)** | 2.80 | $\times 7.58$ | **21.22** |
| 3 | **Disorientation (Pusing & Vertigo)** | 0.80 | $\times 13.92$ | **11.14** |
| **TOTAL** | **Total Severity (TS) Score** | $(0.8 + 2.8 + 0.8) \times 3.74$ | — | **16.46** |

#### Analisis Hasil SSQ:
* **Skor Total Severity = 16.46:** Nilai ini berada jauh di bawah ambang batas kritis simulator sickness ($TS > 40.0$), membuktikan aplikasi masuk dalam kategori **"Sangat Nyaman dan Sangat Layak"**.
* Nilai Mual (*Nausea*) dan Disorientasi mendekati 0 karena aplikasi mempertahankan posisi kamera yang stabil (tanpa percepatan buatan yang sering memicu mabuk visual). Ketegangan mata ringan ($0.8$) muncul semata akibat fokus membaca teks detail pada layar smartphone.

### 6.2 Pengujian Kinerja Teknis (Performance Benchmarking)

| Parameter Pengujian | Pengujian di Laptop (Editor) | Pengujian di HP Android (Redmi 9) | Status Evaluasi |
|:---|:---:|:---:|:---:|
| **Frame Rate Rata-Rata** | 60 FPS (Stabil) | 38 – 52 FPS | Sangat Lancar / Real-time |
| **Latensi Deteksi Vuforia** | < 120 ms | < 250 ms | Deteksi Instan |
| **Penggunaan Memori (RAM)** | ~450 MB | ~280 MB | Efisien (Aman untuk RAM 3/4 GB) |
| **Waktu Respon Hand Tracking** | 16 ms | 32 ms | Tanpa Lag Terasa (*Responsive*) |
| **Jarak Deteksi Optimal** | 20 cm – 120 cm | 15 cm – 80 cm | Ergonomis untuk Posisi Duduk |

---

## 7. MASALAH & SOLUSI TEKNIS (PROBLEM SOLVING)

Dalam proses pengerjaan, kelompok menghadapi dan berhasil menyelesaikan sejumlah kendala kritis:

1. **Kendala Batas Ukuran File GitHub (100 MB Limit):**
   * *Masalah:* File paket offline Vuforia Engine (`com.ptc.vuforia.engine-11.4.4.tgz`) memiliki ukuran 138 MB, sehingga proses `git push` ditolak oleh remote GitHub.
   * *Solusi:* Mengonfigurasi `git-lfs` (*Large File Storage*) pada ekstensi `*.tgz` dan mengecualikan file arsip sementara di `.gitignore`. Dengan Git LFS, commit berhasil terkirim dan tercatat utuh.
2. **Peringatan Input System Baru vs Lama pada Android Build:**
   * *Masalah:* Unity 6 memunculkan pesan peringatan *"Unsupported Input Handling on Android (Active Input Handling is set to Both)"*.
   * *Solusi:* Menyelaraskan script interaksi agar mendukung *UnityEngine.InputSystem* secara native untuk event sentuhan layar Android serta mengabaikan dialog saat kompilasi batchmode tanpa mempengaruhi fungsionalitas aplikasi.
3. **Penyimpangan Skala Objek Akibat Derau Citra (*Scale Drift*):**
   * *Masalah:* Eksperimen awal yang mencoba memperbesar/memperkecil objek menggunakan jarak dua tangan menyebabkan objek 3D berubah ukuran secara acak akibat noise kamera.
   * *Solusi:* Mengunci skala objek 3D pada rasio proporsional tetap ($0.2 \times 0.03 \times 0.266$) dan memusatkan interaksi gestur sepenuhnya pada kendali rotasi horizontal, rotasi vertikal, dan percepatan putaran.
4. **Metode Digital Screen Target Scanning:**
   * *Masalah:* Saat pengembang berada di lokasi terpisah dan salah satu anggota tidak memegang buku fisik, proses pengujian AR terancam terhambat.
   * *Solusi:* Menggunakan teknik *Digital Screen Scanning* (menampilkan file sampul resolusi tinggi `Buku_Sampul.jpg` di monitor laptop, lalu mengarahkan kamera HP ke monitor). Vuforia terbukti mengekstrak feature points identik dengan buku fisik asli.

---

## 8. KESIMPULAN & PENGEMBANGAN LANJUTAN

### 8.1 Kesimpulan
Proyek UAS Kelompok 4 telah berhasil merealisasikan seluruh target capaian pembelajaran mata kuliah Teknologi AR & VR:
1. Penggabungan fungsional antara modul pengenalan objek optik (*Vuforia*) dan manipulasi gestur virtual (*MediaPipe Hand Tracking*) berhasil diwujudkan dalam satu alur aplikasi terpadu.
2. Pengguna dapat mengeksplorasi versi virtual 3D dari buku fisik secara natural tanpa memerlukan perangkat keras kontroler eksternal.
3. Seluruh berkas deliverables (kode sumber GitHub terstruktur dengan riwayat commit dua kontributor, file executable APK Android, serta laporan komprehensif) telah diselesaikan secara lengkap.

### 8.2 Pengembangan Lanjutan
1. **Peningkatan Katalog Multi-Objek:** Menambahkan model 3D biota laut endemik Togean (seperti ubur-ubur tanpa sengat Danau Mariona) ke dalam sistem katalog interaktif.
2. **Optimasi Dua Tangan (Bimanual Gestures):** Mengembangkan gestur dua tangan secara simultan untuk aksi manipulasi tingkat lanjut seperti membuka halaman buku virtual per lembar (*virtual page turning*).

---

## 9. REFERENSI & LISENSI ASET

1. **PTC Inc.** (2024). *Vuforia Engine Developer Documentation & Image Target Feature Extraction*. Developer Portal Vuforia. (Developer License Free Tier).
2. **Google LLC / Homuler.** (2024). *MediaPipe Unity Plugin: Vision Task HandLandmarker API*. GitHub Repository. (Lisensi: MIT License).
3. **Casiez, G., Roussel, N., & Vogel, D.** (2012). *1 € Filter: A Simple Speed-based Low-pass Filter for Noisy Input in Interactive Systems*. Proceedings of the ACM CHI Conference on Human Factors in Computing Systems.
4. **Kennedy, R. S., Lane, N. E., Berbaum, K. S., & Lilienthal, M. G.** (1993). *Simulator Sickness Questionnaire: An enhanced method for quantifying simulator sickness*. The International Journal of Aviation Psychology, 3(3), 203-220.
5. **Google LLC.** (2023). *Cardboard XR Plugin for Unity*. Google VR Developers. (Lisensi: Apache 2.0).
6. **Aset Digital Twin:** Tekstur Sampul Buku Pariwisata Togean (Dokumentasi Mandiri Kelompok 4, Penggunaan Edukasi Akademik Non-Komersial).

---

## 10. LAMPIRAN TUGAS

* **Tautan Repositori GitHub Utama:**  
  👉 [https://github.com/Heinzzz001/Object_Recognition_and_Hand_Tracking](https://github.com/Heinzzz001/Object_Recognition_and_Hand_Tracking)
* **Tautan Repositori Fork / Base:**  
  👉 [https://github.com/draconik514/Kelompok4-ARVR-UAS](https://github.com/draconik514/Kelompok4-ARVR-UAS)
* **Tautan Berkas APK Android (Executable):**  
  Tersedia di folder rilis repositori: `Builds/Kelompok4_ARVR.apk`
* **Riwayat Git Commit Kontributor:**
  * `Commit 1 (Abu Bakar Shidiq): Feat: Complete AR Object Recognition Scene_AR with Vuforia DB_Kelompok4 by Abu Bakar Shidiq`
  * `Commit 2 (Kavrian Syah): integrasi MediaPipe hand tracking dan interaksi AR buku 3D`
