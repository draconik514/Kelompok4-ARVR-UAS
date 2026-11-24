# Kelompok4-ARVR-UAS: Object Recognition (AR) + Hand Tracking (VR)

Repositori Proyek Besar (*Project-Based Learning*) UAS Mata Kuliah Teknologi Augmented Reality & Virtual Reality — Program Studi Teknik Informatika, Universitas Tadulako.

---

## Identitas Tim Pengembang (Kelompok 4)
* **Abu Bakar Shidiq** — *Project Leader, AR Specialist, 3D Artist, QA Tester & Documentation Lead*
* **Kavrian Syah** — *Lead Developer, VR & Hand Tracking Specialist, UI/UX Designer & DevOps*
* **Engine & SDK:** Unity 6 (`6000.6.9f1`), PTC Vuforia Engine `11.4.4`, MediaPipe Unity `0.16.3` (*21-Joint HandLandmarker* + *OneEuroFilter2D*)
* **Target Platform:** Android Smartphone (`ARM64`, API Level 29+) & Unity Editor Simulation

---

## Fitur Utama Sistem

### 1. Fase AR: Multi-Object Recognition, Sticky Lock & Tap-to-Inspect Info Overlay
* Mendeteksi **5 Objek Sehari-hari Nyata** secara luring menggunakan basis data `DB_Kelompok4`:
  1. `Buku_Pariwisata` — Buku Kebijakan Pengembangan Pariwisata Pulau Togean (Digital Twin 3D)
  2. `2_Buku_Campus` — Buku Tulis Akademik iBook Campus B5
  3. `3_Kalkulator_Citizen` — Kalkulator Meja Citizen SDC-810BN 10-Digit
  4. `4_Penghapus_Faber` — Penghapus Faber-Castell Dust-Free
  5. `1_Pulpen_Pilot` — Pulpen Mekanis Gel Pilot G-2 0.5 mm
* **Tap-to-Inspect & Overlay Instruksi AR:** Mengetuk objek 3D menampilkan kartu informasi edukasi kontekstual serta langkah instruksi penggunaan objek.
* **Sticky AR Lock:** Mempertahankan posisi kembaran 3D di layar saat sasaran fisik tertutup oleh telapak tangan pengguna (*hand-target occlusion mitigation*).

### 2. Fase Ruang VR: Controller-Free Hand Tracking & VR Box Split-Screen
* Tombol transisi **`MASUK MODE VR >>`** memindahkan objek yang dipindai menuju **Ruang Studio Virtual 3D** (*Cyber-Grid Environment*).
* **Pelacakan Tangan Tanpa Pengendali:** Melacak 21 titik sendi jari dan telapak tangan secara *real-time* melalui kamera *smartphone* yang disaring dengan algoritma adaptif **`OneEuroFilter2D`** (rotasi 360° *view-space* & akselerasi gestur mencubit/*pinch* 2,2×).
* **Mode Stereoskopik:** Mendukung pengalihan tampilan 1 layar penuh maupun pembelahan 2 lensa mata kiri-kanan (**`VR BOX: 2 LAYAR`**) dengan sensor giroskop.

---

## Struktur Repositori
* `Assets/Scenes/Scene_AR.unity` — Scene utama terintegrasi lintas mode AR & Ruang Studio VR.
* `Assets/Scripts/` — Kode sumber C# (`PengaturARObjectRecognition.cs`, `PelacakTanganKameraAR.cs`, `InteraksiBukuAR.cs`, `KerangkaTanganUI.cs`, `DataInformasiObjekAR.cs`, serta Editor Builder & Build Script).
* `Assets/StreamingAssets/Vuforia/` — Basis data luring 5 target (`DB_Kelompok4.xml` & `DB_Kelompok4.dat`).
* `Assets/Textures/` & `Assets/Materials/` — Tekstur sampul objek dan material PBR 3D Digital Twin.
* `docs/` — Dokumen Laporan Akhir format Jurnal JATI / IEEE (`.docx` & `.pdf`).
* `KUESIONER_SSQ_KELOMPOK_4.md` & `PROMPT_LOG.md` — Evaluasi kenyamanan SSQ Kennedy ($TS = 16,46$) dan log transparansi penggunaan AI.

---

## Cara Menjalankan & Build APK Android
1. Clone repositori beserta aset Git LFS:
   ```bash
   git clone https://github.com/draconik514/Kelompok4-ARVR-UAS.git
   cd Kelompok4-ARVR-UAS
   git lfs pull
   ```
2. Buka folder proyek melalui **Unity Hub** menggunakan **Unity 6 (`6000.6.9f1`)** (pastikan modul *Android Build Support*, *SDK/NDK*, dan *OpenJDK* terpasang).
3. Buka scene `Assets/Scenes/Scene_AR.unity`:
   * Untuk pengujian langsung di Editor: klik **Play (▶)**, arahkan objek/gambar ke webcam atau klik tombol **`PILIH OBJEK`** / tekan **`SPASI`**.
   * Untuk membangun paket instalasi Android (`.apk`): klik menu atas **`AR` ➔ `Build APK Android (5 Objek Nyata + Hand Tracking)`**; berkas akan dihasilkan pada `Builds/Kelompok4_ARVR.apk`.
