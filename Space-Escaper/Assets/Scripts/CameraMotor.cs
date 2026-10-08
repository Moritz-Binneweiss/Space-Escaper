using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Follows the ship during a run and brakes to a stop behind the wreck after a crash.
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
        [Tooltip("Where the camera comes to rest relative to the wreck after a crash.")]
        [SerializeField] private Vector3 m_crashOffset;
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
            // Nothing moves while the game is paused, and FollowShip divides by the blend.
            if (!IsMoving || Time.deltaTime <= 0f)
            {
                return;
            }

            if (m_ship.IsRunning)
            {
                FollowShip();
            }
            else
            {
                BrakeBehindWreck();
            }
        }

        private void FollowShip()
        {
            float blend = 1f - Mathf.Exp(-k_FollowSharpness * Time.deltaTime);

            // Each frame the ship flies on first, then the camera closes the share blend of
            // the gap. That leaves it behind its place by this lag, about one second of
            // flight. Aiming that far ahead keeps it at the offset at any speed and frame rate.
            float lag = m_ship.Speed * Time.deltaTime * (1f - blend) / blend;
            MoveTowards(m_target.position + m_offset + Vector3.forward * lag, blend);
        }

        // The wreck stands still, so there is no lag to make up. Closing the gap at the
        // ship's speed divided by the distance between the two offsets starts the camera
        // off at the speed it flew with, so it brakes smoothly instead of with a jolt.
        private void BrakeBehindWreck()
        {
            float brakingSharpness = m_ship.Speed / Mathf.Abs(m_offset.z - m_crashOffset.z);
            float blend = 1f - Mathf.Exp(-brakingSharpness * Time.deltaTime);
            MoveTowards(m_target.position + m_crashOffset, blend);
        }

        private void MoveTowards(Vector3 desiredPosition, float blend)
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, blend);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(m_rotation), blend);
        }
    }
}
