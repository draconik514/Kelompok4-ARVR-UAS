using UnityEngine;

public class InteraksiObjekVR : MonoBehaviour
{
    public float kecepatanPutar = 120f;
    private Vector3 posisiTanganLama;
    private bool sedangMemegang = false;

    // Dipanggil oleh Hand Tracking (MediaPipe) saat tangan melakukan gestur cubit/geser
    public void GerakkanObjek(Vector3 posisiTangan, bool isPinching)
    {
        if (isPinching)
        {
            if (!sedangMemegang)
            {
                posisiTanganLama = posisiTangan;
                sedangMemegang = true;
            }
            else
            {
                Vector3 selisih = posisiTangan - posisiTanganLama;
                transform.Rotate(Vector3.up, -selisih.x * kecepatanPutar, Space.World);
                transform.Rotate(Vector3.right, selisih.y * kecepatanPutar, Space.World);
                posisiTanganLama = posisiTangan;
            }
        }
        else
        {
            sedangMemegang = false;
        }
    }

    // Cadangan kontrol sentuh layar agar demo tetap berjalan mulus di berbagai pencahayaan
    void Update()
    {
        if (Input.touchCount == 1)
        {
            Touch sentuhan = Input.GetTouch(0);
            if (sentuhan.phase == TouchPhase.Moved)
            {
                transform.Rotate(Vector3.up, -sentuhan.deltaPosition.x * 0.3f, Space.World);
                transform.Rotate(Vector3.right, sentuhan.deltaPosition.y * 0.3f, Space.World);
            }
        }
    }
}
