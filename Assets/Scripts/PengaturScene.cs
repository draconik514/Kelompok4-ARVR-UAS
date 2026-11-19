using UnityEngine;
using UnityEngine.SceneManagement;

public class PengaturScene : MonoBehaviour
{
    public void KeModeVR()
    {
        SceneManager.LoadScene("Scene_VR");
    }

    public void KeModeAR()
    {
        SceneManager.LoadScene("Scene_AR");
    }
}
