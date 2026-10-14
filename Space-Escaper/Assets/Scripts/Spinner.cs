using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Turns an object evenly, like the asteroids and coins on the track. The SpinManager
    /// in the scene turns all spinners in one Update, which costs far less than an
    /// Animator or an Update on each of them.
    /// </summary>
    public class Spinner : MonoBehaviour
    {
        private const float k_FullTurn = 360f;

        [Tooltip("Degrees per second around the x, y and z axes of the parent. Several axes make the object tumble.")]
        [SerializeField] private Vector3 m_degreesPerSecond;

        private Quaternion m_startRotation;
        private Vector3 m_angles;

        private void Awake()
        {
            m_startRotation = transform.localRotation;
        }

        private void OnEnable()
        {
            SpinManager.Add(this);
        }

        private void OnDisable()
        {
            SpinManager.Remove(this);
        }

        /// <summary>
        /// Turns on by the time since the last frame. Called by the SpinManager.
        /// </summary>
        public void Turn(float deltaTime)
        {
            // Each angle wraps on its own, so they stay precise however long a run lasts.
            m_angles.x = Mathf.Repeat(m_angles.x + m_degreesPerSecond.x * deltaTime, k_FullTurn);
            m_angles.y = Mathf.Repeat(m_angles.y + m_degreesPerSecond.y * deltaTime, k_FullTurn);
            m_angles.z = Mathf.Repeat(m_angles.z + m_degreesPerSecond.z * deltaTime, k_FullTurn);

            // Applied in front of the start rotation, so the axes are the parent's: a coin
            // stood upright on its edge still spins around the vertical.
            transform.localRotation = Quaternion.Euler(m_angles) * m_startRotation;
        }
    }
}
