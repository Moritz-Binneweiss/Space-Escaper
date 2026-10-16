using System;
using System.Collections;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// A coin on the track, collected when the ship flies through it.
    /// </summary>
    public class Coin : MonoBehaviour
    {
        private const string k_PlayerTag = "Player";
        private const float k_ShrinkDuration = 0.25f;

        // The dust emits for at most 1 s, and each speck lives up to 1 s.
        private const float k_DustLifetime = 2f;

        [SerializeField] private GameObject m_dustVfx;

        /// <summary>
        /// Raised for every coin the ship flies through.
        /// </summary>
        public static event Action Collected;

        // The editor starts Play Mode without a domain reload, so static fields would keep their
        // values from the last one.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Collected = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(k_PlayerTag))
            {
                return;
            }

            GetComponent<CapsuleCollider>().enabled = false;
            AudioSystem.Instance.PlayCoinPickup();
            OnCollected();
            GameObject dust = Instantiate(m_dustVfx, transform.position, Quaternion.identity);
            Destroy(dust, k_DustLifetime);
            StartCoroutine(ShrinkAway());
        }

        private static void OnCollected()
        {
            Collected?.Invoke();
        }

        private IEnumerator ShrinkAway()
        {
            Vector3 startScale = transform.localScale;
            for (float time = 0f; time < k_ShrinkDuration; time += Time.deltaTime)
            {
                transform.localScale = startScale * (1f - time / k_ShrinkDuration);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
