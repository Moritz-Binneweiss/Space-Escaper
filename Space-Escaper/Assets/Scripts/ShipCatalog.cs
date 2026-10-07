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
        [Tooltip("Grouped by family. The save stores a ship as its place in this list, starting at 1, "
            + "so the starter ship comes first and the order must stay.")]
        [SerializeField] private List<ShipData> m_ships;

        public IReadOnlyList<ShipData> Ships => m_ships;

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
    }
}
