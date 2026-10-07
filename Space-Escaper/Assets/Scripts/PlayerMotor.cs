using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Flies the ship forward, switches lanes on swipes and crashes it into obstacles.
    /// </summary>
    public class PlayerMotor : MonoBehaviour
    {
        private const string k_ObstacleTag = "Obstacle";
        private const string k_DeathTrigger = "Death";

        private const float k_LaneDistance = 3.5f;
        private const int k_LeftLane = 0;
        private const int k_MiddleLane = 1;
        private const int k_RightLane = 2;

        private const float k_StartSpeed = 11f;
        private const float k_SpeedIncreaseInterval = 5f;
        private const float k_SpeedIncreaseAmount = 0.2f;

        [SerializeField] private GameObject m_explosionVfx;
        [Tooltip("The ARISTOCRAT engine flame. Its emission is on while the ship runs.")]
        [SerializeField] private ParticleSystem m_engineFlame;

        private Animator m_animator;
        private CharacterController m_controller;
        private bool m_isRunning;
        private int m_desiredLane = k_MiddleLane;
        private float m_speed;
        private float m_lastSpeedIncreaseTime;

        public bool IsRunning => m_isRunning;

        private void Start()
        {
            m_speed = k_StartSpeed;
            m_controller = GetComponent<CharacterController>();
            m_animator = GetComponent<Animator>();
            SetEngineFlameEmission(false);
        }

        private void Update()
        {
            if (!m_isRunning)
            {
                return;
            }

            SetEngineFlameEmission(true);
            IncreaseSpeedOverTime();

            if (MobileInput.Instance.HasSwipedLeft)
            {
                ChangeLane(-1);
            }

            if (MobileInput.Instance.HasSwipedRight)
            {
                ChangeLane(1);
            }

            Move();
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.gameObject.CompareTag(k_ObstacleTag))
            {
                Crash();
            }
        }

        public void StartRunning()
        {
            m_isRunning = true;
            AudioSystem.Instance.StartEngine();

            // No music here: GameManager.Play() starts the game track, and stopping
            // music here would cut it off a frame later.
        }

        private void IncreaseSpeedOverTime()
        {
            if (Time.time - m_lastSpeedIncreaseTime <= k_SpeedIncreaseInterval)
            {
                return;
            }

            m_lastSpeedIncreaseTime = Time.time;
            m_speed += k_SpeedIncreaseAmount;
            GameManager.Instance.UpdateModifier(m_speed - k_StartSpeed);
        }

        private void ChangeLane(int direction)
        {
            m_desiredLane = Mathf.Clamp(m_desiredLane + direction, k_LeftLane, k_RightLane);
        }

        // Forward at full speed, sideways towards the lane at a rate that grows
        // with the distance to it.
        private void Move()
        {
            float laneX = (m_desiredLane - k_MiddleLane) * k_LaneDistance;
            Vector3 moveVector = new Vector3((laneX - transform.position.x) * m_speed, 0f, m_speed);
            m_controller.Move(moveVector * Time.deltaTime);
        }

        private void Crash()
        {
            m_animator.SetTrigger(k_DeathTrigger);
            m_isRunning = false;
            GameManager.Instance.HandleDeath();
            Instantiate(m_explosionVfx, transform.position, Quaternion.identity);
            SetEngineFlameEmission(false);
            AudioSystem.Instance.PlayExplosion();
            AudioSystem.Instance.StopEngine();
        }

        private void SetEngineFlameEmission(bool isEnabled)
        {
            ParticleSystem.EmissionModule emission = m_engineFlame.emission;
            emission.enabled = isEnabled;
        }
    }
}
