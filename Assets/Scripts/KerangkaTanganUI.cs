using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kelompok4.AR
{
    /// <summary>
    /// KerangkaTanganUI:
    /// Merender visualisasi kerangka tangan MediaPipe 21 Landmark (23 tulang & 21 sendi) secara real-time.
    /// Mengikuti arsitektur kinematis Google MediaPipe Hand Landmark dengan 1 draw call via VertexHelper.
    /// Dilengkapi indikator cincin cubitan dinamis di antara ujung jempol dan telunjuk.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class KerangkaTanganUI : Graphic
    {
        [Header("Pengaturan Tampilan")]
        public float ketebalanGaris = 5.0f;
        public float radiusSendi = 6.0f;
        public Color warnaTulangNormal = new Color(0.0f, 0.95f, 1.0f, 0.90f); // Cyan neon
        public Color warnaSendiNormal = new Color(1.0f, 1.0f, 1.0f, 0.95f);  // Putih
        public Color warnaTulangPinch = new Color(1.0f, 0.85f, 0.0f, 0.95f); // Emas / Kuning
        public Color warnaSendiPinch = new Color(1.0f, 0.95f, 0.4f, 1.0f);  // Kuning terang

        private bool isHandDetected = false;
        private bool isPinching = false;
        private float pinchLerp = 0f;

        // 21 Titik Sendi MediaPipe dalam koordinat lokal Canvas:
        // 0: Wrist
        // 1..4: Thumb (CMC, MCP, IP, Tip)
        // 5..8: Index (MCP, PIP, DIP, Tip)
        // 9..12: Middle (MCP, PIP, DIP, Tip)
        // 13..16: Ring (MCP, PIP, DIP, Tip)
        // 17..20: Pinky (MCP, PIP, DIP, Tip)
        private readonly Vector2[] keypoints = new Vector2[21];

        // 23 Pasang Tulang MediaPipe Resmi
        private static readonly int[,] TulangKoneksi = new int[,]
        {
            // Pergelangan ke Pangkal Jari
            {0, 1}, {0, 5}, {0, 9}, {0, 13}, {0, 17},
            // Garis Lengkung Pangkal Jari (Knuckle Arch)
            {5, 9}, {9, 13}, {13, 17},
            // Ibu Jari (Thumb)
            {1, 2}, {2, 3}, {3, 4},
            // Jari Telunjuk (Index)
            {5, 6}, {6, 7}, {7, 8},
            // Jari Tengah (Middle)
            {9, 10}, {10, 11}, {11, 12},
            // Jari Manis (Ring)
            {13, 14}, {14, 15}, {15, 16},
            // Jari Kelingking (Pinky)
            {17, 18}, {18, 19}, {19, 20}
        };

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
        }

        /// <summary>
        /// Menerima 21 koordinat layar MediaPipe yang telah disaring halus dengan OneEuroFilter
        /// </summary>
        public void PerbaruiKerangkaMediaPipe(Vector2[] screenLandmarks21, bool pinching, bool detected)
        {
            isHandDetected = detected;
            isPinching = pinching;

            if (!isHandDetected || screenLandmarks21 == null || screenLandmarks21.Length < 21)
            {
                SetVerticesDirty();
                return;
            }

            float targetPinch = isPinching ? 1.0f : 0.0f;
            pinchLerp = Mathf.Lerp(pinchLerp, targetPinch, 15f * Time.deltaTime);

            Camera cam = canvas != null ? canvas.worldCamera : null;

            // Konversi 21 koordinat layar ke koordinat lokal Canvas
            for (int i = 0; i < 21; i++)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenLandmarks21[i], cam, out Vector2 localPt);
                keypoints[i] = localPt;
            }

            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (!isHandDetected) return;

            Color warnaTulang = Color.Lerp(warnaTulangNormal, warnaTulangPinch, pinchLerp);
            Color warnaSendi = Color.Lerp(warnaSendiNormal, warnaSendiPinch, pinchLerp);
            float tebal = Mathf.Lerp(ketebalanGaris, ketebalanGaris * 1.3f, pinchLerp);

            // 1. Gambar 23 Segmen Tulang MediaPipe
            int boneCount = TulangKoneksi.GetLength(0);
            for (int b = 0; b < boneCount; b++)
            {
                int idxA = TulangKoneksi[b, 0];
                int idxB = TulangKoneksi[b, 1];
                GambarGaris(vh, keypoints[idxA], keypoints[idxB], tebal, warnaTulang);
            }

            // 2. Gambar 21 Titik Sendi MediaPipe
            for (int i = 0; i < 21; i++)
            {
                bool isTip = (i == 4 || i == 8 || i == 12 || i == 16 || i == 20);
                float r = isTip ? (radiusSendi * 1.35f) : radiusSendi;
                GambarTitikSendi(vh, keypoints[i], r, warnaSendi);
            }

            // 3. Lingkaran Indikator Cubitan di antara Ujung Jempol (4) dan Ujung Telunjuk (8)
            if (pinchLerp > 0.05f)
            {
                Vector2 pinchCenter = (keypoints[4] + keypoints[8]) * 0.5f;
                float ringR = 15f + Mathf.PingPong(Time.time * 25f, 6f);
                GambarCincin(vh, pinchCenter, ringR, 3.5f, new Color(1f, 0.85f, 0.1f, 0.95f * pinchLerp));
            }
        }

        private void GambarGaris(VertexHelper vh, Vector2 start, Vector2 end, float tebal, Color col)
        {
            Vector2 dir = (end - start);
            float len = dir.magnitude;
            if (len < 0.001f) return;
            dir /= len;

            Vector2 normal = new Vector2(-dir.y, dir.x) * (tebal * 0.5f);

            int startVert = vh.currentVertCount;
            vh.AddVert(start - normal, col, Vector2.zero);
            vh.AddVert(start + normal, col, Vector2.zero);
            vh.AddVert(end + normal, col, Vector2.zero);
            vh.AddVert(end - normal, col, Vector2.zero);

            vh.AddTriangle(startVert, startVert + 1, startVert + 2);
            vh.AddTriangle(startVert, startVert + 2, startVert + 3);
        }

        private void GambarTitikSendi(VertexHelper vh, Vector2 pos, float radius, Color col)
        {
            int startVert = vh.currentVertCount;
            vh.AddVert(pos, col, Vector2.zero);

            for (int i = 0; i < 8; i++)
            {
                float angle = i * (Mathf.PI * 2f / 8f);
                Vector2 p = pos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                vh.AddVert(p, col, Vector2.zero);
            }

            for (int i = 1; i <= 8; i++)
            {
                int next = (i == 8) ? 1 : (i + 1);
                vh.AddTriangle(startVert, startVert + i, startVert + next);
            }
        }

        private void GambarCincin(VertexHelper vh, Vector2 pos, float radius, float tebal, Color col)
        {
            float rInner = radius - (tebal * 0.5f);
            float rOuter = radius + (tebal * 0.5f);
            int segmen = 12;

            int startVert = vh.currentVertCount;
            for (int i = 0; i < segmen; i++)
            {
                float angle = i * (Mathf.PI * 2f / segmen);
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                vh.AddVert(pos + dir * rInner, col, Vector2.zero);
                vh.AddVert(pos + dir * rOuter, col, Vector2.zero);
            }

            for (int i = 0; i < segmen; i++)
            {
                int next = (i + 1) % segmen;
                int iInner = startVert + i * 2;
                int iOuter = iInner + 1;
                int nextInner = startVert + next * 2;
                int nextOuter = nextInner + 1;

                vh.AddTriangle(iInner, nextInner, nextOuter);
                vh.AddTriangle(iInner, nextOuter, iOuter);
            }
        }
    }
}
