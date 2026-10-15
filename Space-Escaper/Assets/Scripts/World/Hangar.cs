using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Shows the hangar while it is open: the building with the shown ship on its stand, seen through the hangar
    /// camera instead of the main camera.
    /// </summary>
    public class Hangar : MonoBehaviour
    {
        [SerializeField] private GameObject m_mainCamera;
        [SerializeField] private GameObject m_shopCamera;
        [SerializeField] private GameObject m_building;
        [Tooltip("The model of the shown ship is created here.")]
        [SerializeField] private Transform m_stand;

        private GameObject m_model;

        private void Awake()
        {
            Close();
        }

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

        /// <summary>
        /// Puts the model of this ship on the stand in place of the old one.
        /// </summary>
        public void ShowShip(ShipData ship)
        {
            if (m_model != null)
            {
                Destroy(m_model);
            }

            // The model prefab carries its place on the flown ship. The hangar
            // shows it on its stand instead, at a size of its own.
            m_model = Instantiate(ship.Model, m_stand);
            m_model.transform.localPosition = Vector3.zero;
            m_model.transform.localScale = ship.HangarScale;
        }

        private void GameManager_HangarOpened()
        {
            Open();
        }

        private void GameManager_HangarClosed()
        {
            Close();
        }

        private void Open()
        {
            m_mainCamera.SetActive(false);
            m_shopCamera.SetActive(true);
            m_building.SetActive(true);
        }

        private void Close()
        {
            m_mainCamera.SetActive(true);
            m_shopCamera.SetActive(false);
            m_building.SetActive(false);
        }
    }
}
