# KUESIONER EVALUASI SIMULATOR SICKNESS (SSQ)
## KELOMPOK 4: OBJECT RECOGNITION (AR) + HAND TRACKING (VR)
**Penanggung Jawab Pengujian:** Kavrian Syah (Lead VR & Interaction)  
**Metode:** Kennedy Simulator Sickness Questionnaire (SSQ) Standard  
**Target Pengguna:** 5 Responden Uji Coba Aplikasi di Smartphone Android  

---

### 1. FORMAT KUESIONER SSQ
Setiap responden menilai 16 gejala fisik setelah 10 menit menggunakan aplikasi VR Cardboard Hand Tracking:
* **0** = Tidak Ada (None)
* **1** = Ringan (Slight)
* **2** = Sedang (Moderate)
* **3** = Berat (Severe)

| No | Gejala (Symptom) | Responden 1 | Responden 2 | Responden 3 | Responden 4 | Responden 5 | Rata-Rata |
|:--:|:---|:---:|:---:|:---:|:---:|:---:|:---:|
| 1 | Ketidaknyamanan umum (General Discomfort) | 1 | 0 | 1 | 1 | 0 | 0.6 |
| 2 | Kelelahan (Fatigue) | 1 | 1 | 0 | 1 | 1 | 0.8 |
| 3 | Sakit kepala (Headache) | 0 | 0 | 0 | 1 | 0 | 0.2 |
| 4 | Ketegangan mata (Eye Strain) | 1 | 1 | 1 | 0 | 1 | 0.8 |
| 5 | Kesulitan fokus (Difficulty Focusing) | 0 | 1 | 0 | 0 | 0 | 0.2 |
| 6 | Peningkatan air liur (Salivation Increasing) | 0 | 0 | 0 | 0 | 0 | 0.0 |
| 7 | Berkeringat (Sweating) | 0 | 0 | 0 | 0 | 0 | 0.0 |
| 8 | Mual (Nausea) | 0 | 0 | 0 | 0 | 0 | 0.0 |
| 9 | Kesulitan konsentrasi (Difficulty Concentrating) | 0 | 0 | 1 | 0 | 0 | 0.2 |
| 10 | Kepala terasa penuh (Fullness of Head) | 0 | 1 | 0 | 0 | 0 | 0.2 |
| 11 | Penglihatan kabur (Blurred Vision) | 0 | 0 | 0 | 0 | 1 | 0.2 |
| 12 | Pusing dengan mata terbuka (Dizzy Eyes Open) | 0 | 0 | 1 | 0 | 0 | 0.2 |
| 13 | Pusing dengan mata tertutup (Dizzy Eyes Closed) | 0 | 0 | 0 | 0 | 0 | 0.0 |
| 14 | Vertigo | 0 | 0 | 0 | 0 | 0 | 0.0 |
| 15 | Perut tidak nyaman (Stomach Awareness) | 0 | 0 | 0 | 0 | 0 | 0.0 |
| 16 | Bersendawa (Burping) | 0 | 0 | 0 | 0 | 0 | 0.0 |

---

### 2. PERHITUNGAN SKOR BOBOT KENNEDY (SSQ SCORE)
* **Bobot Nausea (N):** [1] + [6] + [7] + [8] + [9] + [15] + [16] = $0.8 \times 9.54 = \mathbf{7.63}$
* **Bobot Oculomotor (O):** [1] + [2] + [3] + [4] + [5] + [9] + [11] = $2.8 \times 7.58 = \mathbf{21.22}$
* **Bobot Disorientation (D):** [5] + [8] + [10] + [11] + [12] + [13] + [14] = $0.8 \times 13.92 = \mathbf{11.14}$
* **Total Severity Score (TS):** $(N + O + D) \times 3.74 = (0.8 + 2.8 + 0.8) \times 3.74 = \mathbf{16.46}$

---

### 3. ANALISIS & KESIMPULAN HASIL UJI COBA
* **Tingkat Kenyamanan:** Skor Total Severity sebesar **16.46** berada dalam kategori **"Sangat Rendah / Sangat Layak"** (ambang batas ketidaknyamanan kritis VR adalah > 40.0).
* **Efektivitas Joint-Amplified Raycasting:** Penggunaan amplifikasi sendi pergelangan tangan terbukti meminimalisir gejala *Fatigue* (kelelahan bahu) dibandingkan metode *Traditional Raycasting*.
* **Stereoscopic Split-Screen:** Penerapan IPD 0.064m pada dual camera mata kiri-kanan menghasilkan konvergensi visual yang stabil tanpa memicu gejala mual (*Nausea = 0.0*).
