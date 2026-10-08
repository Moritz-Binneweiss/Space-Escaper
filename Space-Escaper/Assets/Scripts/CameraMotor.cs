using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Follows the ship during a run.
    /// </summary>
    public class CameraMotor : MonoBehaviour
    {
        // How quickly the camera closes in on its place, per second. At 1 it has
        // settled behind the ship about 3 s after the run starts.
        private const float k_FollowSharpness = 1f;

        [Tooltip("The ship the camera follows.")]
        [SerializeField] private Transform m_target;
        [Tooltip("Where the camera stays relative to the ship during a run, at any speed.")]
        [SerializeField] private Vector3 m_offset;
        [Tooltip("Euler angles the camera turns towards.")]
        [SerializeField] private Vector3 m_rotation;

        private PlayerMotor m_ship;

        public bool IsMoving { get; set; }

        private void Awake()
        {
            m_ship = m_target.GetComponent<PlayerMotor>();
        }

        private void LateUpdate()
        {
            // Nothing moves while the game is paused, and the lag below divides by the blend.
            if (!IsMoving || Time.deltaTime <= 0f)
            {
                return;
            }

            float blend = 1f - Mathf.Exp(-k_FollowSharpness * Time.deltaTime);

            // Each frame the ship flies on first, then the camera closes the share blend of
            // the gap. That leaves it behind its place by this lag, about one second of
            // flight. Aiming that far ahead keeps it at the offset at any speed and frame rate.
            float lag = m_ship.Speed * Time.deltaTime * (1f - blend) / blend;
            Vector3 desiredPosition = m_target.position + m_offset + Vector3.forward * lag;

            transform.position = Vector3.Lerp(transform.position, desiredPosition, blend);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(m_rotation), blend);
        }
    }
}
