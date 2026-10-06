using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Shows the first ARISTOCRAT skin and hides the other two when the scene starts.
    /// </summary>
    public class SkinChange : MonoBehaviour
    {
        [SerializeField] private GameObject m_aristocrat;
        [SerializeField] private GameObject m_aristocratSkin2;
        [SerializeField] private GameObject m_aristocratSkin3;

        private void Start()
        {
            m_aristocrat.SetActive(true);
            m_aristocratSkin2.SetActive(false);
            m_aristocratSkin3.SetActive(false);
        }
    }
}
