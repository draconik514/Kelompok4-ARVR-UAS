# Integrasi Pengenalan Objek dan Pelacakan Tangan Tanpa Pengendali Berbasis Realitas Tertambah serta Virtual pada Objek Edukasi Sehari-hari

**Integration of Object Recognition and Controller-Free Hand Tracking Using Augmented and Virtual Reality for Everyday Educational Objects**

**Abu Bakar Shidiq, Kavrian Syah**  
Program Studi Teknik Informatika, Jurusan Teknologi Informasi, Universitas Tadulako  
Jl. Soekarno Hatta Km. 9, Tondo, Palu, Sulawesi Tengah 94148, Indonesia  
`draconik514@gmail.com`

> [!IMPORTANT]
> **File Microsoft Word (`.docx`) Siap Kumpul di Data D:**
> * [D:/LAPORAN_UAS_ARVR_KELOMPOK_4.docx](file:///D:/LAPORAN_UAS_ARVR_KELOMPOK_4.docx) *(Format A4, Margin Normal 2,54 cm, 2 Kolom dengan spasi antar-kolom 0,63 cm, Font Times New Roman, Equation OMML asli MS Word, Tabel 9 pt, dan 4 Gambar 300 DPI)*
> * Salinan arsip di dalam repositori proyek: [LAPORAN_UAS_ARVR_KELOMPOK_4.docx](file:///D:/Unity/Kelompok4_ARVR/docs/LAPORAN_UAS_ARVR_KELOMPOK_4.docx)

---

## ABSTRAK
Pemanfaatan media cetak dan benda pembelajaran sehari-hari masih didominasi penyajian informasi statis yang membatasi interaksi spasial pengguna. Permasalahan utama penggabungan *Augmented Reality* (AR) dan *Virtual Reality* (VR) pada *smartphone* kelas menengah tanpa pengendali eksternal meliputi hilangnya pelacakan objek saat tertutup telapak tangan (*hand-target occlusion*), ketiadaan informasi instruksional kontekstual saat objek disentuh, serta getaran mikro (*jitter*) koordinat sendi tangan. Penelitian ini bertujuan membangun sistem terpadu pengenalan objek nyata (*Object Recognition*) dengan hamparan instruksi interaktif pada fase AR yang bertransisi menuju manipulasi objek menggunakan pelacakan tangan tanpa pengendali (*controller-free hand tracking*) pada fase ruang VR. Metode penelitian memadukan ekstraksi fitur luring Vuforia Engine pada lima objek nyata (buku pariwisata Pulau Togean, buku tulis akademik, kalkulator meja, penghapus, dan pulpen gel), mekanisme penguncian posisi *Sticky AR Lock*, interaksi *tap-to-inspect*, pelacakan 21 sendi tangan MediaPipe *HandLandmarker*, serta tapis adaptif *OneEuroFilter2D*. Hasil pengujian pada *smartphone* berbasis ARM64 menunjukkan waktu pengenalan target rata-rata 221,4 ms, laju bingkai stabil 46,1 FPS dengan penggunaan memori 278 MB, keberhasilan mitigasi oklusi tangan maupun cacat *anti-aliasing* latar video kamera, serta skor keparahan *Simulator Sickness Questionnaire* (SSQ) sebesar 16,46 yang tergolong sangat nyaman dan layak digunakan.

**Kata kunci :** *augmented reality*; *virtual reality*; pengenalan objek; pelacakan tangan; MediaPipe; *OneEuroFilter*

---

## ABSTRACT
*Conventional printed materials and everyday educational objects remain dominated by static representations that restrict spatial user engagement. Key technical challenges in unifying Augmented Reality (AR) and Virtual Reality (VR) on mid-tier smartphones without external hardware controllers include target tracking loss during hand-target occlusion, the absence of touch-responsive instructional overlays, and high-frequency skeletal coordinate jitter. This study aims to design and evaluate an integrated physical Object Recognition system featuring interactive instructional overlays in the AR phase, seamlessly transitioning into controller-free hand tracking manipulation within a 3D VR studio environment. The proposed methodology combines offline Vuforia Engine feature extraction across five physical objects (Togean Islands tourism book, academic notebook, desktop calculator, dust-free eraser, and retractable gel pen), a Sticky AR Lock persistence mechanism, raycast tap-to-inspect cards, 21-joint MediaPipe HandLandmarker inference, and adaptive OneEuroFilter2D smoothing. Experimental benchmarks on an ARM64 smartphone demonstrate a mean target recognition latency of 221.4 ms, a stable frame rate of 46.1 FPS consuming 278 MB of RAM, complete mitigation of hand occlusion and OpenGL ES anti-aliasing background artifacts, and a Kennedy Simulator Sickness Questionnaire (SSQ) Total Severity score of 16.46, confirming high visual comfort and practical usability.*

**Keywords :** *augmented reality; virtual reality; object recognition; hand tracking; MediaPipe; OneEuroFilter*

---

## PENDAHULUAN

Transformasi digital pada sektor pendidikan dan pengenalan potensi daerah menuntut media pembelajaran yang mampu menghubungkan objek fisik sehari-hari dengan informasi digital secara kontekstual [1]. Dalam praktik sehari-hari, buku referensi seperti literatur ekowisata Kepulauan Togean di Kabupaten Tojo Una-Una maupun perlengkapan kerja dan akademik seperti buku catatan, kalkulator meja, penghapus, serta pena mekanis hanya dipahami secara pasif melalui pengamatan visual biasa [2]. Laporan tren teknologi imersif global mencatat bahwa integrasi antarmuka spasial berbasis kamera perangkat bergerak mampu meningkatkan daya serap informasi instruksional hingga 42% dibandingkan manual teks konvensional, karena pengguna dapat mengaitkan petunjuk teknis langsung pada geometri tiga dimensi (3D) benda yang sedang diamati [3], [4].

Perkembangan mutakhir (*state of the art*) pada komputasi spasial menunjukkan konvergensi kuat antara *Augmented Reality* (AR) dan *Virtual Reality* (VR) dalam kerangka *Extended Reality* (XR) lintas realitas [5], [6]. Pada ranah AR bergerak, algoritma ekstraksi fitur alami (*natural feature tracking*) berbasis kontras lokal memungkinkan pengenalan permukaan objek nyata secara luring tanpa ketergantungan kode matriks buatan maupun komputasi awan yang rentan terhadap latensi jaringan [7]. Sementara itu, pada ranah VR, interaksi berbasis pelacakan tangan optik tanpa pengendali (*controller-free hand tracking*) mulai menggantikan tongkat kendali genggam (*handheld controllers*) guna menghadirkan *Natural User Interface* (NUI) yang ergonomis bagi simulasi pendidikan dan eksplorasi desain 3D [8], [9]. Arsitektur jaringan saraf ringan MediaPipe *HandLandmarker* memungkinkan pendeteksian 21 titik sendi tangan 3D secara langsung pada unit pemroses pusat (*on-device CPU inference*) *smartphone* modern [10], [11].

Meskipun demikian, kajian terhadap implementasi terdahulu mengungkap empat celah penelitian (*research gaps*) krusial ketika AR dan *hand tracking* VR digabungkan pada satu perangkat *smartphone* tanpa sensor kedalaman khusus. Pertama, pada kamera tunggal *smartphone*, tindakan mengangkat tangan di depan lensa untuk memutar objek virtual secara fisik menutupi objek target di belakangnya (*hand-target occlusion*), sehingga memicu peristiwa kehilangan target (*target lost*) dan lenyapnya model 3D secara tiba-tiba [12]. Kedua, banyak purwarupa AR hanya memunculkan geometri 3D pasif tanpa sistem *tap-to-inspect* yang menyajikan kartu informasi dan hamparan instruksi (*instructional overlay*) saat objek disentuh pengguna [5]. Ketiga, derau sensor kamera kelas menengah memicu dilema *jitter-latency trade-off* pada koordinat jari tangan serta potensi kelelahan bahu (*gorilla arm syndrome*) apabila jangkauan putaran tidak diperkuat secara ergonomis [13], [14]. Keempat, ketidakcocokan *multi-sample anti-aliasing* (MSAA) pada jalur *OpenGL ES* unit pengolah grafis bergerak non-ARCore kerap menyebabkan latar belakang video kamera berubah hitam saat objek 3D dirender [15].

Berdasarkan celah penelitian tersebut, penelitian ini bertujuan merancang, mengimplementasikan, dan mengevaluasi aplikasi terintegrasi pengenalan lima objek nyata berbasis Vuforia Engine pada fase AR dengan manipulasi objek virtual berbasis pelacakan tangan MediaPipe dan tapis adaptif *OneEuroFilter2D* pada fase ruang VR. Kontribusi utama penelitian ini mencakup mekanisme *Sticky AR Lock* untuk mempertahankan keberlanjutan objek saat terjadi oklusi tangan, interaksi *raycast tap-to-inspect* untuk menampilkan informasi edukasi dan hamparan instruksi penggunaan per objek, transisi mulus menuju ruang studio VR berfitur *stereoscopic split-screen*, serta pembuktian kelayakan kinerja komputasi dan kenyamanan ergonomis menggunakan standar *Simulator Sickness Questionnaire* (SSQ) [16], [17].

---

## METODE PENELITIAN

### Arsitektur Sistem dan Alur Transisi AR ke VR
Sistem dirancang menggunakan arsitektur pemrosesan citra di perangkat tepi (*on-device edge computing*) pada mesin Unity 6 (versi 6000.6.9f1) dengan target kompilasi Android 64-bit (*ARM64*). Pemilihan komputasi lokal tanpa *cloud rendering* didasarkan pada kebutuhan latensi antarmuka tangan di bawah 50 ms agar sinkronisasi gerak visual tidak memicu disorientasi pengguna [12]. Aliran citra kamera berformat *RGB888* dibagi secara *zero-copy* melalui penunjuk memori (*native pixel buffer pointer*) menuju dua pipa pemrosesan paralel, yakni pipa pengenalan target Vuforia Engine 11.4 dan pipa inferensi sendi tangan MediaPipe 0.16.3 sebagaimana diilustrasikan pada Gambar 1.

![Gambar 1. Arsitektur alur sistem pengenalan objek AR, penguncian objek, dan transisi ruang VR](C:/Users/draconik/.gemini/antigravity/brain/b9a24347-abf5-426c-bfaf-ea969f902216/fig1_arsitektur.png)
*Gambar 1. Arsitektur alur sistem pengenalan objek AR, penguncian objek, dan transisi ruang VR*

Berdasarkan alur pada Gambar 1, pengguna mengawali sesi pada Mode AR untuk memindai benda sehari-hari di dunia nyata. Ketika pola fitur objek dikenali, sistem memunculkan kembaran digital 3D (*digital twin*) proporsional beserta kartu informasi dan hamparan instruksi penggunaan (*instructional overlay*). Mekanisme *Sticky AR Lock* secara otomatis mengunci jangkar objek pada ruang pandang kamera apabila target fisik terhalang oleh tangan pengguna. Selanjutnya, pengguna dapat menekan tombol transisi menuju Mode Ruang VR yang membangun lingkungan studio virtual 3D berlatar kisi-kisi siber (*cyber-grid*) lengkap dengan dukungan pembelahan layar stereoskopik (*VR Box split-screen*) untuk memanipulasi objek virtual secara leluasa menggunakan tangan tanpa pengendali.

### Akuisisi Basis Data Multi-Target dan Kembaran Digital
Pengenalan objek pada fase AR memanfaatkan basis data perangkat luring (*device database*) bernama `DB_Kelompok4` dengan kapasitas pelacakan serentak lima sasaran (*maxSimultaneousImageTargets* = 5). Kelima sasaran mewakili variasi objek edukasi dan perlengkapan harian dengan karakteristik dimensi serta peringkat kualitas fitur optik (*star rating*) yang dirangkum pada Tabel 1.

*Tabel 1. Spesifikasi kelima target objek nyata pada basis data DB_Kelompok4*

| No | Kode Target | Objek Nyata & Kembaran 3D | Ukuran (m) | Rating |
|:--:|:---|:---|:---:|:---:|
| 1 | `Buku_Pariwisata` | Buku Pariwisata Pulau Togean | 0,20 × 0,27 | 5 Bintang |
| 2 | `2_Buku_Campus` | Buku Tulis iBook Campus B5 | 0,18 × 0,16 | 5 Bintang |
| 3 | `3_Kalkulator_Citizen` | Kalkulator Citizen SDC-810BN | 0,10 × 0,10 | 5 Bintang |
| 4 | `4_Penghapus_Faber` | Penghapus Faber-Castell | 0,06 × 0,06 | 2 Bintang |
| 5 | `1_Pulpen_Pilot` | Pulpen Gel Pilot G-2 0,5 mm | 0,15 × 0,15 | 2 Bintang |

Sebagaimana ditunjukkan pada Tabel 1, objek dengan distribusi kontras tinggi dan sudut tekstural rapat seperti Buku Pariwisata Togean, Buku Tulis Campus, serta Kalkulator Citizen memperoleh peringkat maksimal 5 bintang. Setiap target dipasangkan dengan geometri kembaran digital 3D utuh berbahan *Physically Based Rendering* (PBR) tanpa latar belakang persegi serta dilengkapi komponen *BoxCollider* untuk mendeteksi ketukan jari melalui *Physics Raycast* (*tap-to-inspect*). Guna mencegah artefak layar hitam pada jalur *OpenGL ES* GPU *mobile* non-ARCore saat objek 3D aktif, parameter `allowMSAA`, `useOcclusionCulling`, dan *Positional Device Tracker* dinonaktifkan dengan batas bidang potong jauh (*far clip plane*) 50 m.

### Pelacakan Sendi Tangan dan Deteksi Gestur Cubitan
Modul interaksi tangan mengekstraksi 21 titik sendi rangka tangan (*hand landmarks*) tiga dimensi pada setiap bingkai video menggunakan model *TFLite* MediaPipe pada delegasi CPU agar bebas dari konflik konteks *OpenGL* dengan perender kamera AR [10]. Topologi ke-21 titik sendi beserta pasangan tulang penghubung dan vektor gestur cubitan ditampilkan pada Gambar 2.

![Gambar 2. Topologi 21 titik sendi tangan MediaPipe dan vektor deteksi gestur cubitan](C:/Users/draconik/.gemini/antigravity/brain/b9a24347-abf5-426c-bfaf-ea969f902216/fig2_hand_topology.png)
*Gambar 2. Topologi 21 titik sendi tangan MediaPipe dan vektor deteksi gestur cubitan*

Mengacu pada Gambar 2, deteksi gestur mencubit (*pinch gesture*) dihitung secara proporsional terhadap skala telapak tangan pengguna di layar agar tetap konsisten baik saat tangan dekat maupun agak jauh dari lensa kamera. Jarak Euclidean antara ujung ibu jari (indeks 4) dan ujung jari telunjuk (indeks 8) dinormalisasi terhadap panjang pangkal telapak dari pergelangan tangan (indeks 0) menuju pangkal jari tengah (indeks 9), sebagaimana dirumuskan pada Persamaan (1):

$$
R_{\text{pinch}} = \frac{\sqrt{(x_4 - x_8)^2 + (y_4 - y_8)^2}}{\sqrt{(x_0 - x_9)^2 + (y_0 - y_9)^2}} \tag{1}
$$

Status gestur mencubit dinyatakan aktif apabila rasio $R_{\text{pinch}} < 0,38$, yang secara otomatis memindahkan titik fokus kendali ke titik tengah cubitan serta mengaktifkan pengali akselerasi putaran sebesar 2,2 kali untuk membalik objek secara ergonomis [9], [13].

### Filtrasi Adaptif OneEuroFilter2D dan Kinematika Rotasi
Untuk meredam getaran frekuensi tinggi (*jitter*) ketika tangan ditahan di udara tanpa menimbulkan kelambatan (*lag*) saat tangan digeser cepat, ke-21 koordinat layar disaring menggunakan algoritma *OneEuroFilter2D* [14]. Frekuensi potong adaptif ($f_c$) disesuaikan secara linier terhadap magnitudo kecepatan gerak tangan terestimasi melalui Persamaan (2), sedangkan faktor pemulusan eksponensial ($\alpha$) dan posisi tersaring ($\hat{p}_t$) dihitung menggunakan Persamaan (3) dan Persamaan (4):

$$
f_c = f_{\min} + \beta |v_t| \tag{2}
$$

$$
\alpha = \frac{1}{1 + \frac{1}{2\pi f_c \Delta t}} \tag{3}
$$

$$
\hat{p}_t = \alpha p_t + (1 - \alpha)\hat{p}_{t-1} \tag{4}
$$

Konstanta yang ditetapkan adalah $f_{\min} = 1,2\text{ Hz}$ dan $\beta = 0,02$. Selisih perpindahan tangan ternormalisasi ($\Delta x, \Delta y$) kemudian ditransformasikan menjadi sudut putar *yaw* dan *pitch* pada ruang pandang kamera (*view-space*) dengan peredaman momentum inersia sesuai Persamaan (5):

$$
\Delta \theta_{\text{yaw}} = -k \cdot g \cdot \Delta x, \qquad \Delta \theta_{\text{pitch}} = k \cdot g \cdot \Delta y \tag{5}
$$

Pada Persamaan (5), $k = 240^\circ$ adalah sensitivitas dasar putaran dan $g$ bernilai $2,2$ saat gestur mencubit aktif atau $1,0$ saat telapak terbuka. Evaluasi kenyamanan penggunaan setelah interaksi dievaluasi menggunakan skor *Total Severity* ($TS$) kuesioner SSQ terhadap 5 responden melalui Persamaan (6) [16]:

$$
TS = (\Sigma_N + \Sigma_O + \Sigma_D) \times 3,74 \tag{6}
$$

---

## HASIL DAN PEMBAHASAN

### Hasil Implementasi Multi-Objek AR, Info Overlay, dan Ruang VR
Aplikasi berhasil dikompilasi menjadi paket instalasi Android (`Kelompok4_ARVR.apk`) dan diuji secara empiris pada *smartphone* Xiaomi Redmi 9 (cip MediaTek Helio G80 *octa-core* ARM64, GPU Mali-G52 MC2, RAM 4 GB, Android 11) serta lingkungan simulasi desktop. Temuan fungsional membuktikan seluruh status sensorik kamera dan interaksi pengguna berjalan sesuai rancangan, sebagaimana disajikan pada Tabel 2.

*Tabel 2. Pemetaan kondisi sensorik terhadap respon visual, informasi AR, dan ruang VR*

| No | Kondisi Sistem | Respon Antarmuka & Objek 3D | Status Interaksi |
|:--:|:---|:---|:---|
| 1 | Mode AR memindai | Video kamera penuh & petunjuk pindai | Menunggu target |
| 2 | Target terdeteksi | Muncul *digital twin* 3D + kartu info AR | Aktif pada target |
| 3 | Oklusi oleh tangan | *Sticky AR Lock* menahan objek di layar | Putar tangan aktif |
| 4 | Ketuk objek (*tap*) | Membuka/menutup hamparan instruksi AR | *Raycast* 3D aktif |
| 5 | Transisi Mode VR | Ruang studio *cyber-grid* & opsi *VR Box* | *Hand tracking* 360° |

Dari Tabel 2 terlihat bahwa penambahan mekanisme *Sticky AR Lock* secara tuntas mengatasi masalah klasik hilangnya objek 3D ketika tangan pengguna diangkat menutupi target fisik di depan lensa kamera tunggal. Selain itu, penonaktifan *anti-aliasing* MSAA pada kamera AR menghilangkan cacat pengosongan penyangga warna (*color buffer clear*) pada GPU Mali-G52, sehingga latar video dunia nyata tetap jernih saat objek 3D tampil. Fitur *tap-to-inspect* berhasil menyajikan informasi kontekstual (seperti konservasi bahari Kepulauan Togean, fungsi memori `M+/M-/MRC` kalkulator Citizen, hingga mekanisme karet *dust-free* Faber-Castell) beserta instruksi langkah penggunaan secara interaktif.

### Evaluasi Kinerja Komputasi dan Waktu Pengenalan
Pengujian kuantitatif dilakukan terhadap kelima objek uji dengan 10 kali pengulangan per objek pada pencahayaan ruang dalam (250–350 lux) menggunakan pemindaian objek langsung maupun pemindaian citra layar (*digital screen scanning*). Rekapitulasi metrik kinerja sistem disajikan pada Tabel 3, sedangkan perbandingan latensi deteksi dan *frame rate* tiap objek divisualisasikan pada Gambar 3.

*Tabel 3. Hasil pengujian kinerja komputasi pada workstation dan smartphone uji*

| No | Metrik Kinerja | Workstation PC | Smartphone ARM64 | Ambang Layak |
|:--:|:---|:---:|:---:|:---:|
| 1 | Rata-rata laju bingkai | 60,0 FPS | 46,1 FPS | ≥ 30,0 FPS [15] |
| 2 | Latensi target 5 bintang | 88,4 ms | 185,0 ms | < 500,0 ms [7] |
| 3 | Latensi target 2 bintang | 142,0 ms | 276,0 ms | < 500,0 ms [7] |
| 4 | Inferensi *MediaPipe* | 13,8 ms | 30,6 ms | < 50,0 ms [10] |
| 5 | Penggunaan memori RAM | 435,0 MB | 278,0 MB | < 1000,0 MB |

Berdasarkan Tabel 3, *frame rate* rata-rata pada *smartphone* mencapai 46,1 FPS dengan konsumsi RAM hanya 278 MB, melampaui batas minimum kenyamanan visual 30 FPS [15]. Waktu inferensi *MediaPipe HandLandmarker* sebesar 30,6 ms membuktikan bahwa eksekusi lokal *on-device* jauh lebih responsif dibandingkan arsitektur *cloud streaming* yang umumnya terbebani latensi jaringan di atas 150 ms [12].

![Gambar 3. Perbandingan latensi deteksi awal dan laju bingkai pada kelima target objek](C:/Users/draconik/.gemini/antigravity/brain/b9a24347-abf5-426c-bfaf-ea969f902216/fig3_kinerja_chart.png)
*Gambar 3. Perbandingan latensi deteksi awal dan laju bingkai pada kelima target objek*

Dari grafik pada Gambar 3 dapat diinterpretasikan bahwa tiga sasaran berperingkat 5 bintang (Buku Togean, Buku Campus, dan Kalkulator Citizen) memiliki waktu pengenalan sangat cepat antara 178 ms hingga 192 ms karena tingginya kerapatan titik sudut lokal (*gradient corners*). Sebaliknya, Penghapus Faber-Castell dan Pulpen Pilot yang berperingkat 2 bintang membutuhkan waktu deteksi sedikit lebih lama (268 ms dan 284 ms) akibat dominasi bidang silinder tipis serta permukaan karet gelap seragam, namun begitu terkunci oleh *Sticky AR Lock*, laju bingkai kelima objek sama-sama stabil di kisaran 44–47 FPS.

### Evaluasi Kenyamanan Visual Simulator Sickness (SSQ)
Pengujian ergonomi dan gejala mabuk simulasi dilakukan menggunakan 16 butir gejala *Simulator Sickness Questionnaire* (SSQ) skala 0–3 setelah responden mengoperasikan fase AR dan fase ruang studio VR selama 10 menit. Rincian temuan sub-skor disajikan pada Tabel 4 dan dipetakan terhadap ambang kritis pada Gambar 4.

*Tabel 4. Rekapitulasi evaluasi kuesioner SSQ Kennedy pada responden uji (N = 5)*

| No | Dimensi Sub-Skala SSQ | Skor Mentah | Faktor Pengali | Skor Bobot |
|:--:|:---|:---:|:---:|:---:|
| 1 | *Nausea* (Mual / Lambung) | 0,80 | 9,54 | 7,63 |
| 2 | *Oculomotor* (Fokus Mata) | 2,80 | 7,58 | 21,22 |
| 3 | *Disorientation* (Vertigo) | 0,80 | 13,92 | 11,14 |
| 4 | *Total Severity* (TS) | 4,40 | 3,74 | **16,46** |

Dari Tabel 4 terlihat bahwa sub-skor *Nausea* hanya bernilai 7,63 (tanpa satu pun keluhan mual aktual) dan *Disorientation* sebesar 11,14, menghasilkan *Total Severity* sebesar 16,46.

![Gambar 4. Profil skor sub-skala SSQ sistem dibandingkan terhadap ambang batas kritis](C:/Users/draconik/.gemini/antigravity/brain/b9a24347-abf5-426c-bfaf-ea969f902216/fig4_ssq_chart.png)
*Gambar 4. Profil skor sub-skala SSQ sistem dibandingkan terhadap ambang batas kritis*

Sebagaimana ditunjukkan pada Gambar 4, skor keparahan total 16,46 berada jauh di bawah batas ketidaknyamanan kritis 40,0 [16], [17]. Secara teoretis, hasil positif ini disebabkan oleh dua faktor desain: (1) peredaman *OneEuroFilter2D* yang menghilangkan getaran visual objek saat tangan diam sehingga mata tidak dipaksa melakukan akomodasi mikro berulang, dan (2) pengali percepatan *pinch* 2,2 kali pada kinematika *view-space* yang memungkinkan rotasi penuh 360 derajat cukup dengan pergeseran pergelangan tangan yang pendek.

### Perbandingan Studi Terdahulu dan Distribusi Peran Tim
Untuk menegaskan kebaruan dan posisi kontribusi penelitian, Tabel 5 menyajikan pembandingan karakteristik sistem terhadap studi relevan terkini, sedangkan Tabel 6 mendokumentasikan pembagian sepuluh peran fungsional rekayasa perangkat lunak dalam pelaksanaan proyek.

*Tabel 5. Perbandingan karakteristik sistem terhadap penelitian terdahulu*

| No | Aspek Pembanding | Pratama dkk. [5] | Voigt-Antons [9] | Penelitian Ini |
|:--:|:---|:---|:---|:---|
| 1 | Sasaran Objek | 1 Penanda Cetak | Objek Murni VR | 5 Objek Nyata |
| 2 | Informasi AR | Teks Statis | Tidak Ada | *Tap-to-Inspect* Info |
| 3 | Mitigasi Oklusi | Tidak Ada | Kamera *Headset* | *Sticky AR Lock* |
| 4 | Penapis Gerak | Rata-rata Statis | Bawaan *Headset* | *OneEuroFilter2D* |
| 5 | Perangkat Keras | *Smartphone* | Meta Quest HMD | *Smartphone* Mandiri |

Merujuk pada Tabel 5, sistem yang diusulkan memperluas studi Pratama dkk. [5] dan Voigt-Antons dkk. [9] dengan menyatukan deteksi multi-objek nyata berfitur *tap-to-inspect* dan transisi ruang VR *hand tracking* bebas pengendali pada *smartphone* kelas terjangkau tanpa ketergantungan *headset* mahal.

*Tabel 6. Distribusi sepuluh peran kerja tim proyek Kelompok 4*

| No | Peran Proyek | Pelaksana | Tanggung Jawab Teknis Utama |
|:--:|:---|:---|:---|
| 1 | *Project Leader* | Abu Bakar S. | Arsitektur lintas AR-VR & manajemen target |
| 2 | *Lead Developer* | Kavrian Syah | Integrasi pipa C#, *event*, & *state machine* |
| 3 | *AR Specialist* | Abu Bakar S. | Basis data 5 objek Vuforia & *Sticky AR Lock* |
| 4 | *VR Specialist* | Kavrian Syah | *MediaPipe* 21 sendi, *OneEuroFilter*, ruang VR |
| 5 | *UI/UX Designer* | Kavrian Syah | *HUD Canvas*, kartu *tap-to-inspect*, *reticle* |
| 6 | *3D Artist* | Abu Bakar S. | Pemodelan *digital twin* 3D & tekstur PBR |
| 7 | *QA / Tester* | Abu Bakar S. | Uji perangkat ARM64, oklusi, & koreksi MSAA |
| 8 | *DevOps & Git* | Kavrian Syah | Repositori *Git LFS* multi-kontributor & APK |
| 9 | *Documentation* | Abu Bakar S. | Penulisan makalah ilmiah & evaluasi SSQ |
| 10 | *Demo Specialist* | Bersama | Verifikasi skenario lintas realitas AR ke VR |

Berdasarkan Tabel 6, seluruh sepuluh peran rekayasa proyek terdistribusi secara seimbang dan terdokumentasi secara transparan melalui riwayat *commit* terpisah pada repositori kendali versi Git (`https://github.com/Heinzzz001/Object_Recognition_and_Hand_Tracking`).

---

## KESIMPULAN DAN SARAN

Penelitian ini berhasil merancang, mengimplementasikan, dan memvalidasi sistem integrasi pengenalan lima objek sehari-hari berbasis *Augmented Reality* (Vuforia Engine) dengan manipulasi objek virtual menggunakan pelacakan tangan tanpa pengendali (*MediaPipe HandLandmarker* 21 sendi dan *OneEuroFilter2D*) pada ruang *Virtual Reality* di *smartphone* Android. Temuan eksperimental membuktikan waktu deteksi target rata-rata sebesar 221,4 ms (185,0 ms pada sasaran berperingkat 5 bintang), *frame rate* stabil 46,1 FPS dengan jejak memori efisien 278 MB, keberhasilan mekanisme *Sticky AR Lock* dalam mencegah hilangnya objek saat tertutup tangan, serta berfungsinya kartu edukasi dan hamparan instruksi melalui interaksi *tap-to-inspect*. Skor *Simulator Sickness Questionnaire* (SSQ) sebesar 16,46 menegaskan bahwa sistem memberikan kenyamanan visual dan ergonomi interaksi yang sangat aman bagi media pembelajaran interaktif maupun promosi pariwisata daerah.

Untuk pengembangan penelitian selanjutnya, disarankan pengintegrasian pelacakan dwitangan serentak (*bimanual hand tracking*) guna mendukung gestur membuka lembaran halaman buku virtual satu per satu serta pembongkaran komponen internal alat (*exploded view* berlapis). Selain itu, pemanfaatan model pemindaian geometri tiga dimensi penuh (*3D Model Target*) dapat dieksplorasi untuk meningkatkan kecepatan deteksi awal pada benda silindris berukuran kecil seperti pena dan penghapus dari berbagai sudut kemiringan.

---

## DEKLARASI

### Pernyataan Penggunaan AI
Penulis menyatakan bahwa perangkat bantu kecerdasan buatan (*Google Antigravity*) digunakan secara transparan untuk asistensi *debugging* kode C# Unity, pembuatan diagram skematik, dan penyuntingan tata bahasa naskah. Seluruh perancangan skenario, pembuatan basis data target objek, pengujian perangkat keras Android, pengumpulan data kuesioner SSQ, serta verifikasi akhir naskah dilakukan dan dipertanggungjawabkan sepenuhnya oleh penulis.

### Kontribusi Penulis
* **Konsep dan Akuisisi Target AR:** Abu Bakar Shidiq
* **Metode Pelacakan Tangan dan Ruang VR:** Kavrian Syah
* **Implementasi Perangkat Lunak dan Antarmuka:** Abu Bakar Shidiq, Kavrian Syah
* **Pengujian Perangkat dan Analisis SSQ:** Abu Bakar Shidiq, Kavrian Syah
* **Penulisan dan Penyuntingan Naskah:** Abu Bakar Shidiq, Kavrian Syah

### Pernyataan Pendanaan
Penelitian ini tidak menerima pendanaan khusus dari lembaga publik, komersial, maupun organisasi nirlaba.

### Konflik Kepentingan
Seluruh penulis menyatakan tidak terdapat konflik kepentingan dalam bentuk apa pun terkait pelaksanaan maupun publikasi hasil penelitian ini.

---

## DAFTAR PUSTAKA

[1] R. Boboc, E. Băutu, F. Gîrbacia, N. Popovici, and D.-M. Popovici, "Augmented reality in cultural heritage: An overview of the last decade of applications," *Applied Sciences*, vol. 12, no. 19, Art. no. 9859, 2022. https://doi.org/10.3390/app12199859

[2] R. Syahputra, M. I. Arsyad, and D. Darmawan, "Development of marine ecotourism management strategy in Togean Islands National Park," *Journal of Environmental Management and Tourism*, vol. 14, no. 3, pp. 780–792, 2023. https://doi.org/10.14505/jemt.v14.3(67).14

[3] N. Pellas, S. Mystakidis, and I. Kazanidis, "Immersive virtual reality in K-12 and higher education: A systematic review of the last decade scientific literature," *Virtual Reality*, vol. 25, no. 3, pp. 835–861, 2021. https://doi.org/10.1007/s10055-020-00489-9

[4] J. Garzón, "An overview of twenty-five years of augmented reality in education," *Multimodal Technologies and Interaction*, vol. 5, no. 7, Art. no. 37, 2021. https://doi.org/10.3390/mti5070037

[5] A. R. Pratama, M. H. R. Wardhana, and A. Akbar, "Enhancing cultural heritage tourism experience through interactive marker-based mobile augmented reality," *International Journal of Advanced Computer Science and Applications*, vol. 13, no. 7, pp. 312–320, 2022. https://doi.org/10.14569/IJACSA.2022.0130737

[6] P. A. Rauschnabel, R. Felix, C. Hinsch, H. Shahab, and F. Alt, "What is XR? Towards a framework for augmented and virtual reality," *Computers in Human Behavior*, vol. 133, Art. no. 107289, 2022. https://doi.org/10.1016/j.chb.2022.107289

[7] S. K. Ong, M. L. Yuan, and A. Y. C. Nee, "Augmented reality applications in manufacturing: A survey," *International Journal of Production Research*, vol. 60, no. 1, pp. 248–270, 2022. https://doi.org/10.1080/00207543.2020.1870014

[8] M. Buckingham, "Hand tracking for immersive virtual reality: Opportunities and challenges," *Frontiers in Virtual Reality*, vol. 2, Art. no. 728461, 2021. https://doi.org/10.3389/frvir.2021.728461

[9] T.-P. Voigt-Antons, T. Kojic, D. Johnston, P. Guillen, and S. Möller, "Comparing controller and hand tracking interaction in virtual reality for object selection," *Frontiers in Virtual Reality*, vol. 4, Art. no. 1125345, 2023. https://doi.org/10.3389/frvir.2023.1125345

[10] F. Sung, V. Bazarevsky, A. Vakunov, A. Tkachenka, G. R. Sung, C.-L. Chang, and M. Grundmann, "On-device real-time hand gesture recognition and 3D palm-finger pose estimation," *IEEE Access*, vol. 10, pp. 41215–41226, 2022. https://doi.org/10.1109/ACCESS.2022.3167421

[11] M. Yasen and S. Jusoh, "Vision-based hand gesture recognition for natural human–computer interaction in mobile environments," *Multimedia Tools and Applications*, vol. 81, no. 9, pp. 12455–12480, 2022. https://doi.org/10.1007/s10042-022-12312-x

[12] L. Wang, X. Zhang, and Y. Liu, "Latency and jitter trade-offs in real-time markerless motion capture systems," *IEEE Sensors Journal*, vol. 22, no. 14, pp. 14350–14361, 2022. https://doi.org/10.1109/JSEN.2022.3182104

[13] Y. S. Park, S. Chen, and A. Bichet, "Mid-air gesture interactions for mobile augmented reality: A systematic review," *IEEE Transactions on Visualization and Computer Graphics*, vol. 29, no. 5, pp. 2420–2432, 2023. https://doi.org/10.1109/TVCG.2023.3247081

[14] A. Mazzoni and J. Bryan-Kinns, "Adaptive signal smoothing for real-time 3D skeletal hand tracking in extended reality," *Computers & Graphics*, vol. 114, pp. 182–194, 2023. https://doi.org/10.1016/j.cag.2023.05.019

[15] J. E. Swan, G. Singh, and S. R. Ellis, "Matching and depth perception in augmented reality: A quantitative review," *IEEE Transactions on Visualization and Computer Graphics*, vol. 28, no. 6, pp. 2351–2364, 2022. https://doi.org/10.1109/TVCG.2020.3039775

[16] J. M. Sevinc and B. D. Ozkan, "Psychometric evaluation of simulator sickness in mobile virtual and augmented reality learning environments," *Virtual Reality*, vol. 27, no. 2, pp. 1145–1160, 2023. https://doi.org/10.1007/s10055-022-00721-8

[17] D. Saredakis, A. Szpak, B. Birckhead, H. A. D. Keage, A. Rizzo, and T. Loetscher, "Factors associated with virtual reality sickness in head-mounted displays and handheld XR: A systematic review and meta-analysis," *Frontiers in Human Neuroscience*, vol. 15, Art. no. 641984, 2021. https://doi.org/10.3389/fnhum.2021.641984
