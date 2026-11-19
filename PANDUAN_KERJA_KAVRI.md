# PANDUAN KERJA KAVRIAN SYAH (TAHAP 3: MODE VR & HAND TRACKING)

Dokumen ini adalah panduan kerja teknis untuk **Kavrian Syah** dalam menyelesaikan implementasi **Mode VR & Hand Tracking** pada project UAS Kelompok 4.

---

### RINGKASAN PROGRESS SAAT INI
Bagian **Mode AR (Scene_AR)** sudah 100% selesai dikerjakan oleh **Abu Bakar Shidiq**:
* Kamera Vuforia mendeteksi sampul fisik Buku Pariwisata.
* Objek Digital Twin 3D (`Buku_Virtual_3D`) melayang dan berputar di atas buku fisik.
* Tombol UI `MASUK MODE VR` sudah terhubung ke script `PengaturScene.KeModeVR()`.

---

### LANGKAH KERJA KAVRIAN SYAH DI `Scene_VR`:

#### 1. Buka Scene VR di Unity
* Buka project `Kelompok4_ARVR` di Unity Editor.
* Di panel bawah (`Assets > Scenes`), klik 2x pada **`Scene_VR`**.

#### 2. Buat Objek Buku Virtual 3D
* Di Hierarchy, klik kanan ➔ **3D Object > Cube** ➔ beri nama **`Buku_Virtual_3D`**.
* Atur Transform di Inspector:
  * **Position:** `X: 0`, `Y: 0`, `Z: 2` (pas di depan mata kamera).
  * **Scale:** `X: 0.8`, `Y: 0.08`, `Z: 1.0` (proporsi buku tebal 3D).
  * **Rotation:** `X: 20`, `Y: -15`, `Z: 0`.
* Pasang material sampul: Tarik **`Assets/Materials/Mat_BukuSampul3D.mat`** ke objek tersebut.
* Pasang script interaksi: Tarik script **`Assets/Scripts/InteraksiObjekVR.cs`** ke objek `Buku_Virtual_3D` di Inspector.

#### 3. Pasang MediaPipe Hand Tracking
* Impor package MediaPipe dari folder `Bahan_Kelompok4`: `MediaPipeUnity.0.16.3.unitypackage`.
* Buka folder `Assets > MediaPipeUnity > Samples > Scenes > Hand Tracking`.
* Tarik prefab **`HandTrackingSolution`** ke dalam Hierarchy di `Scene_VR`.

#### 4. Setup Cardboard VR (Split-Screen di HP)
* Buka **Edit ➔ Project Settings ➔ XR Plug-in Management** ➔ tab Android ➔ pastikan **`Cardboard XR Plugin`** tercentang.
* Klik **`Main Camera`** di Hierarchy ➔ di Inspector klik **Add Component** ➔ tambahkan **`Tracked Pose Driver`** (agar kamera bergerak mengikuti tolehan kepala).

#### 5. Buat Tombol Kembali ke AR
* Klik kanan di Hierarchy ➔ **UI > Canvas** ➔ buat tombol **Button - TextMeshPro** bertuliskan `KEMBALI KE AR`.
* Buat Empty GameObject bernama `ManajerScene`, pasang script `PengaturScene.cs`, lalu hubungkan tombol ke fungsi `PengaturScene.KeModeAR()`.
* Tekan **Ctrl + S** untuk save.

---

### TAHAP AKHIR: BUILD APK
1. Buka **File ➔ Build Settings**.
2. Daftarkan kedua scene ke *Scenes in Build*:
   * Indeks 0: `Assets/Scenes/Scene_AR.unity`
   * Indeks 1: `Assets/Scenes/Scene_VR.unity`
3. Sambungkan HP Android via kabel USB (USB Debugging aktif).
4. Klik **Build And Run** ➔ simpan dengan nama **`Kelompok4_ARVR.apk`**.
