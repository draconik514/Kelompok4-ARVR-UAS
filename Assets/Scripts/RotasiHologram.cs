using UnityEngine;

namespace Kelompok4.AR
{
    /// <summary>
    /// Memberikan efek rotasi putar halus pada objek 3D hologram yang melayang di atas buku AR.
    /// </summary>
    public class RotasiHologram : MonoBehaviour
    {
        [Tooltip("Kecepatan rotasi derajat per detik")]
        [SerializeField] private float kecepatanRotasi = 35f;

        private void Update()
        {
            // Berputar perlahan mengelilingi sumbu vertikal (Y)
            transform.Rotate(Vector3.up, kecepatanRotasi * Time.deltaTime, Space.Self);
        }
    }
}
