using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// The ship being flown: which one it is, and its model and engine flame on this object. The flame burns while the
    /// ship flies, model and flame go out with the crash and come back with a revive.
    /// </summary>
    public class PlayerShip : MonoBehaviour
    {
        [SerializeField] private ShipCatalog m_shipCatalog;

        private ShipData m_ship;
        private GameObject m_model;
        private GameObject m_engineFlame;

        public ShipData Ship => m_ship;

        private void Awake()
        {
            SaveData saveData = SaveSystem.Data;
            saveData.RepairShips(m_shipCatalog.StarterShip.Id, m_shipCatalog.GetShipIds());
            ChangeShip(m_shipCatalog.FindShip(saveData.CurrentShipId));
        }

        private void OnEnable()
        {
            GameManager.RunStarted += GameManager_RunStarted;
            GameManager.ShipCrashed += GameManager_ShipCrashed;
            GameManager.ShipRevived += GameManager_ShipRevived;
        }

        private void OnDisable()
        {
            GameManager.RunStarted -= GameManager_RunStarted;
            GameManager.ShipCrashed -= GameManager_ShipCrashed;
            GameManager.ShipRevived -= GameManager_ShipRevived;
        }

        /// <summary>
        /// Puts the model and the engine flame of this ship on in place of the old ones. The flame stays off until the
        /// run starts.
        /// </summary>
        public void ChangeShip(ShipData ship)
        {
            if (m_model != null)
            {
                Destroy(m_model);
                Destroy(m_engineFlame);
            }

            m_ship = ship;
            m_model = Instantiate(ship.Model, transform);
            m_engineFlame = Instantiate(ship.EngineFlame, transform);
            m_engineFlame.SetActive(false);
        }

        private void GameManager_RunStarted()
        {
            m_engineFlame.SetActive(true);
        }

        private void GameManager_ShipCrashed()
        {
            m_model.SetActive(false);
            m_engineFlame.SetActive(false);
        }

        private void GameManager_ShipRevived()
        {
            m_model.SetActive(true);
            m_engineFlame.SetActive(true);
        }
    }
}
