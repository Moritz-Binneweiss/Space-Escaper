using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Switches between the main camera and the hangar camera.
    /// </summary>
    public class CameraSwitch : MonoBehaviour
    {
        [SerializeField] private GameObject m_mainCamera;
        [SerializeField] private GameObject m_shopCamera;

        private void Start()
        {
            SwitchToMainCamera();
        }

        public void SwitchToShopCamera()
        {
            m_mainCamera.SetActive(false);
            m_shopCamera.SetActive(true);
        }

        public void SwitchToMainCamera()
        {
            m_mainCamera.SetActive(true);
            m_shopCamera.SetActive(false);
        }
    }
}
