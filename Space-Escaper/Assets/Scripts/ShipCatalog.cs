using System.Collections.Generic;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// All ships of the game, in the order of the hangar.
    /// </summary>
    [CreateAssetMenu(fileName = "ShipCatalog", menuName = "Space Escaper/Ship Catalog")]
    public class ShipCatalog : ScriptableObject
    {
        [Tooltip("Grouped by family, in the order of the hangar.")]
        [SerializeField] private List<ShipData> m_ships;
        [Tooltip("Owned from the start and flown by a new save.")]
        [SerializeField] private ShipData m_starterShip;

        public IReadOnlyList<ShipData> Ships => m_ships;
        public ShipData StarterShip => m_starterShip;

        /// <summary>
        /// The ship with this ID, or null if there is none.
        /// </summary>
        public ShipData FindShip(string id)
        {
            foreach (ShipData ship in m_ships)
            {
                if (ship.Id == id)
                {
                    return ship;
                }
            }

            return null;
        }

        /// <summary>
        /// The ship the hangar shows when the player switches to a family.
        /// </summary>
        public ShipData GetFirstShipOfFamily(ShipFamily family)
        {
            foreach (ShipData ship in m_ships)
            {
                if (ship.Family == family)
                {
                    return ship;
                }
            }

            return null;
        }

        public List<string> GetShipIds()
        {
            List<string> ids = new List<string>(m_ships.Count);
            foreach (ShipData ship in m_ships)
            {
                ids.Add(ship.Id);
            }

            return ids;
        }
    }
}
