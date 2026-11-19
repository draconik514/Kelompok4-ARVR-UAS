# PROMPT LOG PENGEMBANGAN SISTEM AR-VR (KELOMPOK 4)
**Mata Kuliah:** Proyek AR & VR  
**Kelompok 4:** Abu Bakar Shidiq & Kavrian Syah  

Dokumen ini mendokumentasikan log interaksi dan prompt rekayasa perangkat lunak yang digunakan dalam pengembangan sistem ini sebagai bentuk transparansi akademik.

---

### Sesi 1: Perancangan Arsitektur & State Machine (Abu Bakar Shidiq)
* **Prompt:**  
  *"Rancang arsitektur perangkat lunak berbasis State Machine di Unity C# untuk menghubungkan dua mode: AR Object Recognition dan VR Environment. Buat kelas AppFlowManager dengan event OnModeChanged dan transisi scene yang bersih menggunakan URP."*
* **Hasil:**  
  Script `Assets/Scripts/Core/AppFlowManager.cs`.

---

### Sesi 2: Modul Deteksi Objek AR & Floating Billboard (Abu Bakar Shidiq)
* **Prompt:**  
  *"Buat modul ARObjectTracker.cs untuk menangani tracking state pengenalan objek fisik (buku/perangkat teknik) dengan UnityEvent OnTargetFound/OnTargetLost, serta script AROverlayUI.cs dengan efek billboard (selalu menghadap kamera pengguna) dan animasi floating sinusoidal."*
* **Hasil:**  
  Script `Assets/Scripts/AR/ARObjectTracker.cs` dan `Assets/Scripts/AR/AROverlayUI.cs`.

---

### Sesi 3: Hand Tracking & Ergonomic Joint-Amplified Raycasting (Kavrian Syah)
* **Prompt:**  
  *"Sesuai materi kuliah tentang Ergonomic Alternative VR, buat script ErgonomicRaycaster.cs yang membandingkan Traditional Raycasting vs Joint-Amplified Raycasting dengan amplifikasi rotasi pergelangan tangan (wrist joint) untuk mengurangi kelelahan bahu (shoulder fatigue). Lengkapi dengan deteksi Pinch Gesture pada VRHandTracker.cs."*
* **Hasil:**  
  Script `Assets/Scripts/VR/ErgonomicRaycaster.cs` dan `Assets/Scripts/VR/VRHandTracker.cs`.

---

### Sesi 4: Manipulasi Fisika Objek 3D Virtual (Kavrian Syah)
* **Prompt:**  
  *"Buat script VirtualObjectManipulator.cs untuk objek 3D virtual di VR agar dapat dipegang (grab), dirotasi interaktif, diberi highlight warna saat terkena raycast hover, serta memiliki fitur exploded view (bongkar pasang komponen)."*
* **Hasil:**  
  Script `Assets/Scripts/VR/VirtualObjectManipulator.cs`.

---

### Sesi 5: Tool Otomasi Editor & Scene Builder
* **Prompt:**  
  *"Buat Unity Editor Script (Kelompok4SceneSetup.cs) dengan MenuItem untuk menyusun hierarki GameObject, Rig Hand Tracking, Canvas UI TextMeshPro, dan menghubungkan seluruh dependensi script secara otomatis."*
* **Hasil:**  
  Script `Assets/Scripts/Editor/Kelompok4SceneSetup.cs`.
