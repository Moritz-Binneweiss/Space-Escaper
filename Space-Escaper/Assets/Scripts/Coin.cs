using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// A coin on the track, collected when the ship flies through it.
    /// </summary>
    public class Coin : MonoBehaviour
    {
        private const string k_PlayerTag = "Player";
        private const string k_CollectedTrigger = "Collected";
        private const float k_DestroyDelay = 1.5f;

        // The dust emits for at most 1 s, and each speck lives up to 1 s.
        private const float k_DustLifetime = 2f;

        [SerializeField] private GameObject m_dustVfx;

        private Animator m_animator;

        private void Start()
        {
            m_animator = GetComponent<Animator>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(k_PlayerTag))
            {
                return;
            }

            GetComponent<CapsuleCollider>().enabled = false;
            AudioSystem.Instance.PlayCoinPickup();
            GameManager.Instance.CollectCoin();
            m_animator.SetTrigger(k_CollectedTrigger);
            GameObject dust = Instantiate(m_dustVfx, transform.position, Quaternion.identity);
            Destroy(dust, k_DustLifetime);
            Destroy(gameObject, k_DestroyDelay);
        }
    }
}
