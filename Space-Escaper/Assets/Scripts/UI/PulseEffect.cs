using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Fades a graphic in and out again and again, like the glow around the start button.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class PulseEffect : MonoBehaviour
    {
        [Tooltip("Seconds for one pulse, from invisible to full and back.")]
        [SerializeField] private float m_period = 1.8f;

        private CanvasRenderer m_renderer;

        private void Awake()
        {
            m_renderer = GetComponent<CanvasRenderer>();
        }

        // Eases in and out like the mockup: invisible at the start and the end of a pulse, full in the middle.
        // The alpha of the renderer leaves the color of the graphic as it is.
        private void Update()
        {
            float phase = Mathf.Repeat(Time.time / m_period, 1f);
            m_renderer.SetAlpha(0.5f - 0.5f * Mathf.Cos(phase * 2f * Mathf.PI));
        }
    }
}
