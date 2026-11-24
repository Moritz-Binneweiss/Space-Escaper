using System;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Shows the hangar while it is open: the building with the shown ship on its stand, seen through the hangar
    /// camera instead of the main camera. For another family the platform sinks into the floor with the ship, like a
    /// lift, and rises again with the next one.
    /// </summary>
    public class Hangar : MonoBehaviour
    {
        private enum Slide
        {
            None,
            Down,
            Up,
        }

        // As fast as the platform in the hangar of the mockups.
        private const float k_SlideDownDuration = 0.26f;
        private const float k_SlideUpDuration = 0.34f;

        // Below the floor, so not even a glint of the ship peeks out.
        private const float k_OutOfSightMargin = 0.25f;

        [SerializeField] private GameObject m_mainCamera;
        [SerializeField] private GameObject m_shopCamera;
        [SerializeField] private GameObject m_building;
        [Tooltip("The model of the shown ship is created here.")]
        [SerializeField] private Transform m_stand;
        [Tooltip("The platform under the stand, which sinks into the floor with the ship and rises with another one.")]
        [SerializeField] private Transform m_platform;

        private GameObject m_model;
        private Slide m_slide;
        private float m_slideTime;
        private float m_floor;
        private float m_depth;
        private Vector3 m_platformRest;
        private Vector3 m_standRest;
        private Action m_showFamily;

        public bool IsSliding => m_slide != Slide.None;

        // For tests, which follow the slide.
        internal Transform Platform => m_platform;

        internal Transform Stand => m_stand;

        private void Awake()
        {
            Close();
        }

        private void OnEnable()
        {
            GameManager.HangarOpened += GameManager_HangarOpened;
            GameManager.HangarClosed += GameManager_HangarClosed;
        }

        private void OnDisable()
        {
            GameManager.HangarOpened -= GameManager_HangarOpened;
            GameManager.HangarClosed -= GameManager_HangarClosed;
        }

        private void Update()
        {
            if (m_slide == Slide.None)
            {
                return;
            }

            m_slideTime += Time.deltaTime;
            if (m_slide == Slide.Down)
            {
                float downTime = Mathf.Clamp01(m_slideTime / k_SlideDownDuration);
                Place(m_depth * Easing.EaseIn(downTime));
                if (downTime >= 1f)
                {
                    RiseWithNextFamily();
                }

                return;
            }

            float upTime = Mathf.Clamp01(m_slideTime / k_SlideUpDuration);
            Place(m_depth * (1f - Easing.EaseOut(upTime)));
            if (upTime >= 1f)
            {
                m_slide = Slide.None;
            }
        }

        /// <summary>
        /// Puts the model of this ship on the stand in place of the old one.
        /// </summary>
        public void ShowShip(ShipData ship)
        {
            if (m_model != null)
            {
                Destroy(m_model);
            }

            // The model prefab carries its place on the flown ship. The hangar
            // shows it on its stand instead, at a size of its own.
            m_model = Instantiate(ship.Model, m_stand);
            m_model.transform.localPosition = Vector3.zero;
            m_model.transform.localScale = ship.HangarScale;
        }

        /// <summary>
        /// Lets the platform with the shown ship sink into the floor, shows another family once it is out of sight
        /// and lets the platform rise with it again. A step while the platform moves does nothing.
        /// </summary>
        public void SlideToFamily(Action showFamily)
        {
            if (m_slide != Slide.None)
            {
                return;
            }

            m_showFamily = showFamily;
            m_platformRest = m_platform.position;
            m_standRest = m_stand.position;

            // The platform stands on the floor, which hides what sinks below it like the shaft of a lift.
            m_floor = GetBounds(m_platform).min.y;
            m_depth = GetDepthOutOfSight();
            m_slideTime = 0f;
            m_slide = Slide.Down;
        }

        // Out of sight, the next family takes the stand and rises from as deep as its ship needs.
        private void RiseWithNextFamily()
        {
            Place(0f);
            ShowFamily();
            m_depth = GetDepthOutOfSight();
            m_slideTime = 0f;
            m_slide = Slide.Up;
            Place(m_depth);
        }

        private void ShowFamily()
        {
            Action showFamily = m_showFamily;
            m_showFamily = null;
            showFamily();
        }

        private void Place(float depth)
        {
            Vector3 shift = Vector3.down * depth;
            m_platform.position = m_platformRest + shift;
            m_stand.position = m_standRest + shift;
        }

        // How deep the platform and the ship on it sink until even the top of the ship is under the floor.
        private float GetDepthOutOfSight()
        {
            Bounds bounds = GetBounds(m_platform);
            bounds.Encapsulate(GetBounds(m_stand));
            return bounds.max.y - m_floor + k_OutOfSightMargin;
        }

        private static Bounds GetBounds(Transform root)
        {
            Bounds bounds = new Bounds(root.position, Vector3.zero);
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }

        private void GameManager_HangarOpened()
        {
            Open();
        }

        private void GameManager_HangarClosed()
        {
            Close();
        }

        private void Open()
        {
            m_mainCamera.SetActive(false);
            m_shopCamera.SetActive(true);
            m_building.SetActive(true);
        }

        private void Close()
        {
            FinishSlide();
            m_mainCamera.SetActive(true);
            m_shopCamera.SetActive(false);
            m_building.SetActive(false);
        }

        // Closing in the middle of a slide shows the next family at once, in its place.
        private void FinishSlide()
        {
            if (m_slide == Slide.None)
            {
                return;
            }

            if (m_slide == Slide.Down)
            {
                ShowFamily();
            }

            Place(0f);
            m_slide = Slide.None;
        }
    }
}
