using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Knows the colorway the UI shows, the one chosen in the save, and tells the UI when another one is chosen. It
    /// wakes before the rest of the scene, so every screen finds the colorway when it comes on.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class ColorwayManager : MonoBehaviour
    {
        [SerializeField] private ColorwayCatalog m_catalog;

        /// <summary>
        /// The colorway the UI shows, from the save as the scene wakes.
        /// </summary>
        public static ColorwayData Current { get; private set; }

        public IReadOnlyList<ColorwayData> Colorways => m_catalog.Colorways;

        /// <summary>
        /// Raised when another colorway is chosen, once <see cref="Current"/> is that one.
        /// </summary>
        public static event Action ColorwayChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Current = null;
            ColorwayChanged = null;
        }

        private void Awake()
        {
            Show(m_catalog.Find(SaveSystem.Data.ColorwayId));
        }

        /// <summary>
        /// Shows a colorway everywhere and keeps it in the save.
        /// </summary>
        public void Choose(ColorwayData colorway)
        {
            SaveSystem.Data.ColorwayId = colorway.Id;
            SaveSystem.Save();
            Show(colorway);
        }

        private static void Show(ColorwayData colorway)
        {
            if (colorway == Current)
            {
                return;
            }

            Current = colorway;
            OnColorwayChanged();
        }

        private static void OnColorwayChanged()
        {
            ColorwayChanged?.Invoke();
        }
    }
}
