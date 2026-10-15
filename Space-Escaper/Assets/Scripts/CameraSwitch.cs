using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Switches to the hangar camera while the hangar is open, and to the main camera otherwise.
    /// </summary>
    public class CameraSwitch : MonoBehaviour
    {
        [SerializeField] private GameObject m_mainCamera;
        [SerializeField] private GameObject m_shopCamera;

        private void OnEnable()
        {
            GameManager.HangarOpened += GameManager_HangarOpened;
            GameManager.HangarClosed += GameManager_HangarClosed;
        }

        private void OnDisable()
        {
            GameManager.HangarOpened -= GameManager_HangarOpened;
            GameManager.HangarClosed -= GameManager_HangarClosed;
        }

        private void Start()
        {
            SwitchToMainCamera();
        }

        private void GameManager_HangarOpened()
        {
            SwitchToShopCamera();
        }

        private void GameManager_HangarClosed()
        {
            SwitchToMainCamera();
        }

        private void SwitchToShopCamera()
        {
            m_mainCamera.SetActive(false);
            m_shopCamera.SetActive(true);
        }

        private void SwitchToMainCamera()
        {
            m_mainCamera.SetActive(true);
            m_shopCamera.SetActive(false);
        }
    }
}
