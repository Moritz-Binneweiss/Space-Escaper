using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// Briefly enlarges and tints a UI graphic, then brings it back to rest. The coin
    /// icon of the game menu pops this way for every coin collected.
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public class PopEffect : MonoBehaviour
    {
        [Tooltip("How long one pop takes, in seconds.")]
        [SerializeField] private float m_duration = 0.25f;
        [Tooltip("Scale at the height of the pop, relative to the size at rest.")]
        [SerializeField] private float m_peakScale = 1.15f;
        [Tooltip("Tint at the height of the pop.")]
        [SerializeField] private Color m_peakColor = new Color(1f, 1f, 0f);

        private Graphic m_graphic;
        private Color m_restColor;
        private Coroutine m_pop;

        private void Awake()
        {
            m_graphic = GetComponent<Graphic>();
            m_restColor = m_graphic.color;
        }

        // Switching the screen off stops the pop wherever it is, so it comes back at rest.
        private void OnDisable()
        {
            m_pop = null;
            ShowRest();
        }

        public void Play()
        {
            // Coroutines cannot start on a switched-off screen.
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (m_pop != null)
            {
                StopCoroutine(m_pop);
            }

            m_pop = StartCoroutine(Pop());
        }

        private IEnumerator Pop()
        {
            for (float time = 0f; time < m_duration; time += Time.deltaTime)
            {
                // Up and back down in one smooth arc.
                float height = Mathf.Sin(time / m_duration * Mathf.PI);
                transform.localScale = Vector3.one * Mathf.Lerp(1f, m_peakScale, height);
                m_graphic.color = Color.Lerp(m_restColor, m_peakColor, height);
                yield return null;
            }

            ShowRest();
            m_pop = null;
        }

        private void ShowRest()
        {
            transform.localScale = Vector3.one;
            m_graphic.color = m_restColor;
        }
    }
}
