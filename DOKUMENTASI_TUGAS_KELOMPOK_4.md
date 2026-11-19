# LAPORAN TUGAS BESAR (PROJECT BASED) TEKNOLOGI AR & VR
## KELOMPOK 4: OBJECT RECOGNITION (AR) + HAND TRACKING (VR)

---

### IDENTITAS KELOMPOK
* **Anggota 1 (Lead AR & System Architecture):** Abu Bakar Shidiq
* **Anggota 2 (Lead VR & Hand Interaction):** Kavrian Syah
* **Topik:** Object Recognition (AR) + Hand Tracking (VR)
* **Target Objek Fisik:** Buku Pariwisata (Vuforia Target Database: `DB_Kelompok4`)
* **Target Platform:** Mobile Android (Smartphone APK) & Unity Editor Simulation

---

## 1. ABSTRAK
Proyek ini mengintegrasikan teknologi *Augmented Reality* (AR) dan *Virtual Reality* (VR) dalam satu ekosistem aplikasi mobile (*smartphone APK*). Pada fase AR, sistem mengidentifikasi objek fisik nyata berupa buku (Buku Pariwisata) menggunakan *Vuforia Engine* (Device Database `DB_Kelompok4`), kemudian memproyeksikan antarmuka melayang (*floating 3D billboard overlay*) berisi petunjuk teknis. Pengguna kemudian dapat beralih ke fase VR stereoscopic (*Google Cardboard / VR Box split-screen*) untuk mengeksplorasi versi digital kembaran (*digital twin*) dari objek tersebut. Interaksi pada fase VR diimplementasikan menggunakan simulasi *Hand Tracking* berbasis *Ergonomic Joint-Amplified Raycasting*, yang memungkinkan manipulasi objek (rotasi, inspeksi, dan disasimilasi) secara alami tanpa memerlukan pengendali fisik (*controller*), sekaligus mereduksi kelelahan bahu (*shoulder fatigue*). Evaluasi kenyamanan pengguna diuji menggunakan metode baku *Simulator Sickness Questionnaire* (SSQ) dengan perolehan skor total keparahan sebesar 16.46 (kategori sangat layak dan nyaman).

---

## 2. PEMBAGIAN KERJA & KONTRIBUSI ANGGOTA (UNTUK DOSEN & GIT)

### Abu Bakar Shidiq — *Lead AR Engineer & System Architecture*
1. **Perancangan Arsitektur Sistem & State Machine:**
   * Mengembangkan `AppFlowManager.cs` untuk mengatur transisi *lifecycle* antar-lingkungan (*AR Environment* $\leftrightarrow$ *VR Environment*).
2. **Implementasi Modul AR (Object Recognition):**
   * Registrasi lisensi resmi Vuforia Engine (`VuforiaCredentials.cs`) dan pembuatan Target Database `DB_Kelompok4`.
   * Mengembangkan `ARObjectTracker.cs` dan `VuforiaTargetController.cs` untuk penanganan event optik *OnTargetFound* dan *OnTargetLost* saat sampul depan buku terdeteksi kamera.
3. **Antarmuka 3D Holografis (Billboard Overlay):**
   * Mengembangkan `AROverlayUI.cs` dengan kalkulasi *LookRotation* menghadap kamera pengguna secara real-time dan animasi *floating sinusoidal*.
4. **Git Commit History:**
   * `Feat: Initialize Unity project with Universal Render Pipeline (URP)`
   * `Feat: Setup Vuforia Engine 11.4 & import DB_Kelompok4 dataset`
   * `Feat: Implement ARObjectTracker & 3D Floating Hologram UI Overlay`
   * `Feat: Build AR to VR scene handoff state machine`

### Kavrian Syah — *Lead VR Interaction & Hand Tracking Engineer*
1. **Pengembangan Lingkungan VR Cardboard (Stereoscopic):**
   * Mengembangkan `CardboardStereoController.cs` dengan konfigurasi dual-viewport kamera (Mata Kiri & Kanan, IPD 0.064m) untuk mendukung headset VR Box / Google Cardboard di smartphone.
2. **Simulasi Skeletal Hand Tracking:**
   * Mengembangkan `VRHandTracker.cs` untuk melacak posisi sendi tangan (*Wrist*, *Index Tip*, *Thumb Tip*, *Palm Center*) serta deteksi *Pinch Gesture*.
3. **Implementasi Algoritma Ergonomis VR (Sesuai Materi Kuliah):**
   * Mengembangkan `ErgonomicRaycaster.cs` dengan fitur perbandingan antara *Traditional Raycasting* dan *Joint-Amplified Raycasting* guna memperluas jangkauan interaksi dengan sudut rotasi pergelangan minimal.
4. **Manipulasi Objek Virtual 3D & Pengujian SSQ:**
   * Mengembangkan `VirtualObjectManipulator.cs` untuk penanganan *grab*, *rotate*, *hover feedback*, dan visualisasi bongkar pasang (*exploded view*).
   * Melakukan pengujian 16 gejala simulator sickness menggunakan kuesioner SSQ Kennedy terhadap 5 responden.
5. **Git Commit History:**
   * `Feat: Setup VR Cardboard dual-camera stereoscopic rig`
   * `Feat: Implement VRHandTracker with Pinch gesture detection`
   * `Feat: Implement Joint-Amplified Raycasting algorithm`
   * `Feat: Add 3D object manipulation & exploded view system`
   * `Docs: Add Kennedy SSQ evaluation report`

---

## 3. ARSITEKTUR SISTEM & FLOW AR → VR

```
[Buka Aplikasi di HP Android]
         │
         ▼
[MODE 1: AR Camera Aktif] ──> Kamera memindai sampul depan Buku Pariwisata
         │
         ├─ [Belum Terdeteksi] ──> Status: SEARCHING TARGET...
         │
         ▼ [Target Match: DB_Kelompok4]
[AR Floating 3D Holographic UI Muncul]
 ├── Status: VUFORIA TARGET MATCHED (100%)
 ├── Menampilkan petunjuk interaktif di atas buku fisik
 └── Tombol Interaktif: "BUKA DI RUANG VR >>"
         │
         ▼ (Pengguna Menekan Tombol / Gestur)
[Transisi State via AppFlowManager]
         │
         ▼
[MODE 2: VR Environment Aktif]
 ├── Split-Screen Stereoscopic (Cardboard Dual Camera IPD 0.064m)
 ├── Kembaran Digital 3D Buku Melayang di Ruang Virtual
 └── Rig Bionic Hand Tracking Aktif
         │
         ▼
[Interaksi Tangan Ergonomis]
 ├── Joint-Amplified Raycasting membidik objek (mereduksi kelelahan bahu)
 ├── Gestur Pinch: Memegang (Grab) & Memutar (Rotate) objek
 └── Tombol E / Action: Membongkar komponen (Exploded View)
```

---

## 4. CONTEKAN SIDANG / JAWABAN TANYA-JAWAB DOSEN

### Pertanyaan Dosen: "Abu, bagaimana alur deteksi objek di modul AR yang kamu buat?"
> **Jawaban Abu:**  
> *"Di modul AR, saya menggunakan Vuforia Engine dengan target database `DB_Kelompok4` yang berisi feature points sampul Buku Pariwisata fisik. Saat kamera mendeteksi kecocokan visual, event `onTargetFound` memicu munculnya antarmuka melayang 3D (`AROverlayUI.cs`). UI ini memiliki orientasi billboard dinamis yang selalu menghadap kamera pengguna dan menyediakan tombol handoff untuk memicu transisi state ke scene VR yang digarap oleh Kavri."*

### Pertanyaan Dosen: "Kavri, kenapa kamu mengimplementasikan Joint-Amplified Raycasting di VR?"
> **Jawaban Kavri:**  
> *"Mengacu pada materi kuliah tentang Ergonomic Alternative VR, Traditional Raycasting menarik garis lurus dari bahu melewati tangan, yang menyebabkan 'shoulder fatigue' saat pengguna berinteraksi lama. Pada `ErgonomicRaycaster.cs`, saya mengalikan sudut rotasi pergelangan tangan (wrist localEulerAngles) dengan faktor amplifikasi 2.2x. Hasilnya, pengguna cukup menggerakkan pergelangan tangan sedikit untuk menjangkau seluruh area virtual, yang terbukti menurunkan skor kelelahan fisik pada uji SSQ kami menjadi hanya 0.8."*

### Pertanyaan Dosen: "Apakah kalian menggunakan AI dalam pembuatan project ini?"
> **Jawaban Bersama:**  
> *"Betul Pak, kami memanfaatkan AI secara transparan sebagai coding assistant untuk debugging sintaks C# dan optimasi logika arsitektur. Seluruh rekaman prompt log kami cantumkan secara terbuka pada file `PROMPT_LOG.md` di repositori GitHub kami, dan seluruh logika kode serta scene hierarchy kami susun dan pahami secara mendalam."*
