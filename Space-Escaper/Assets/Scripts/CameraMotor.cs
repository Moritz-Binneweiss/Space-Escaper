using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Follows the ship during a run.
    /// </summary>
    public class CameraMotor : MonoBehaviour
    {
        private const float k_BaseOffsetZ = -3f;

        // Mirrors the speed ramp in PlayerMotor.
        private const float k_SpeedIncreaseInterval = 5f;
        private const float k_SpeedIncreaseAmount = 0.2f;

        [Tooltip("The transform the camera follows.")]
        [SerializeField] private Transform m_target;
        [Tooltip("Offset from the target. Z is overwritten during a run.")]
        [SerializeField] private Vector3 m_offset;
        [Tooltip("Euler angles the camera turns towards.")]
        [SerializeField] private Vector3 m_rotation;

        private float m_speedBonus;
        private float m_lastSpeedIncreaseTime;

        public bool IsMoving { get; set; }

        private void LateUpdate()
        {
            if (!IsMoving)
            {
                return;
            }

            if (Time.time - m_lastSpeedIncreaseTime > k_SpeedIncreaseInterval)
            {
                m_lastSpeedIncreaseTime = Time.time;
                m_speedBonus += k_SpeedIncreaseAmount;
            }

            m_offset.z = k_BaseOffsetZ + m_speedBonus;

            Vector3 desiredPosition = m_target.position + m_offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(m_rotation), Time.deltaTime);
        }
    }
}
