using System.Collections.Generic;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// All colorways of the UI, in the order of the colorways screen. The first is the one of a new installation.
    /// </summary>
    [CreateAssetMenu(fileName = "ColorwayCatalog", menuName = "Space Escaper/Colorway Catalog")]
    public class ColorwayCatalog : ScriptableObject
    {
        [SerializeField] private List<ColorwayData> m_colorways;

        public IReadOnlyList<ColorwayData> Colorways => m_colorways;
        public ColorwayData Default => m_colorways[0];

        /// <summary>
        /// The colorway with an ID, or the default one for an ID no colorway has, like the empty one of a save from
        /// before the colorways.
        /// </summary>
        public ColorwayData Find(string id)
        {
            foreach (ColorwayData colorway in m_colorways)
            {
                if (colorway.Id == id)
                {
                    return colorway;
                }
            }

            return Default;
        }
    }
}
