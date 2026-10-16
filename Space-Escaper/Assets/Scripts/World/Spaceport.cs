using System.Collections;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// The spaceport the ship takes off from. It switches itself off a few seconds after the run starts.
    /// </summary>
    public class Spaceport : MonoBehaviour
    {
        private const float k_HideDelay = 3f;

        private void OnEnable()
        {
            GameManager.RunStarted += GameManager_RunStarted;
        }

        private void OnDisable()
        {
            GameManager.RunStarted -= GameManager_RunStarted;
        }

        private void GameManager_RunStarted()
        {
            StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(k_HideDelay);
            gameObject.SetActive(false);
        }
    }
}
