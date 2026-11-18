using System;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// A skin button in the hangar. It stands for one ship, names it with every tap and shows whether the ship is the
    /// one on the stand, whether it is owned and whether it is the one being flown.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ShipButton : MonoBehaviour
    {
        [SerializeField] private ShipData m_ship;
        [Tooltip("Corners around the button while its ship is on the stand.")]
        [SerializeField] private GameObject m_shownFrame;
        [Tooltip("The grey check under the button while the ship is owned but not flown.")]
        [SerializeField] private GameObject m_ownedBadge;
        [Tooltip("The green check under the button while the ship is the one being flown.")]
        [SerializeField] private GameObject m_selectedBadge;

        private Button m_button;

        public ShipData Ship => m_ship;

        public event Action<ShipData> Clicked;

        private void Awake()
        {
            m_button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            m_button.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            m_button.onClick.RemoveListener(OnClicked);
        }

        public void ShowState(bool isShown, bool isOwned, bool isSelected)
        {
            m_shownFrame.SetActive(isShown);
            m_ownedBadge.SetActive(isOwned && !isSelected);
            m_selectedBadge.SetActive(isSelected);
        }

        private void OnClicked()
        {
            Clicked?.Invoke(m_ship);
        }
    }
}
