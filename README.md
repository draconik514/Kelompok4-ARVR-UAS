# Kelompok4-ARVR-UAS: Object Recognition (AR) + Hand Tracking (VR)

Repositori Proyek Besar (Project Based) UAS Mata Kuliah Teknologi AR & VR.

---

### IDENTITAS KELOMPOK 4
* **Abu Bakar Shidiq** — *Lead AR Engineer & System Architecture*
* **Kavrian Syah** — *Lead VR Interaction & Hand Tracking Engineer*
* **Target Objek Fisik:** Buku Pariwisata (Database: `DB_Kelompok4`)
* **Target Platform:** Android Smartphone (APK) & Unity Editor Simulation

---

### ARSITEKTUR & PEMBAGIAN KERJA

#### 1. Modul AR: Object Recognition (Abu Bakar Shidiq)
* Mengintegrasikan Vuforia Engine SDK dan Device Target Database (`DB_Kelompok4`) untuk mendeteksi sampul fisik Buku Pariwisata.
* Mengembangkan `Scene_AR` dengan Floating 3D Holographic UI Overlay dan Digital Twin 3D dari buku tersebut.
* Mengimplementasikan State Machine perpindahan scene (`PengaturScene.cs`) dari mode AR menuju ruang VR.

#### 2. Modul VR: Hand Tracking & Stereoscopic Cardboard (Kavrian Syah)
* Mengembangkan `Scene_VR` dengan dukungan Cardboard XR Plugin (Dual-Eye Stereoscopic Viewport IPD 0.064m).
* Mengintegrasikan MediaPipe Hand Tracking untuk melacak sendi jari tangan pengguna secara real-time via kamera HP.
* Mengimplementasikan rotasi dan manipulasi 3D objek buku (`InteraksiObjekVR.cs`) menggunakan gestur tangan tanpa controller fisik.
* Melakukan evaluasi uji kenyamanan pengguna dengan Simulator Sickness Questionnaire (SSQ).

---

### STRUKTUR REPOSITORI
* `Assets/` — Source code C#, Scene AR & VR, Textures, Materials, dan Model 3D.
* `Packages/` — Konfigurasi package manager (Vuforia Engine, Cardboard XR Plugin, URP).
* `ProjectSettings/` — Pengaturan build Android (API Level 26, ARM64, IL2CPP).
* `DOKUMENTASI_TUGAS_KELOMPOK_4.md` — Laporan lengkap format UAS (Abstrak, Arsitektur, Flowchart).
* `PANDUAN_KERJA_KAVRI.md` — Panduan teknis pengerjaan Scene VR untuk Kavrian Syah.
* `KUESIONER_SSQ_KELOMPOK_4.md` — Hasil evaluasi Kennedy SSQ kenyamanan pengguna.
* `PROMPT_LOG.md` — Rekaman transparan prompt log coding AI.
