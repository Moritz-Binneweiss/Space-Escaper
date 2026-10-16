using System;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// A skin button in the hangar. It stands for one ship and names it with every tap.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ShipButton : MonoBehaviour
    {
        [SerializeField] private ShipData m_ship;

        private Button m_button;

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

        private void OnClicked()
        {
            Clicked?.Invoke(m_ship);
        }
    }
}
