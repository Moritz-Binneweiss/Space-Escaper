using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// What the game knows about one ship. One asset per ship, all of them listed
    /// in the <see cref="ShipCatalog"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "ShipData", menuName = "Space Escaper/Ship Data")]
    public class ShipData : ScriptableObject
    {
        [SerializeField] private ShipFamily m_family;
        [Tooltip("Coins it costs in the hangar. 0 for the starter ship.")]
        [SerializeField] private int m_price;
        [Tooltip("Shown in the hangar until the ship is bought. Empty for the starter ship.")]
        [SerializeField] private Sprite m_priceTag;
        [Tooltip("Placed on the flown ship as it is, offset and scale included.")]
        [SerializeField] private GameObject m_model;
        [Tooltip("Scale of the model on its stand in the hangar.")]
        [SerializeField] private Vector3 m_hangarScale = Vector3.one;
        [Tooltip("Placed next to the model, burns while the ship flies. Shared by the skins of a family.")]
        [SerializeField] private GameObject m_engineFlame;

        public ShipFamily Family => m_family;
        public int Price => m_price;
        public Sprite PriceTag => m_priceTag;
        public GameObject Model => m_model;
        public Vector3 HangarScale => m_hangarScale;
        public GameObject EngineFlame => m_engineFlame;
    }
}
