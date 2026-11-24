using System;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Shows the hangar while it is open: the building with the shown ship on its stand, seen through the hangar
    /// camera instead of the main camera. For another family the platform slides out of sight with the ship along the
    /// floor towards the bottom of the screen, like on a conveyor belt, and comes back from there with the next one.
    /// </summary>
    public class Hangar : MonoBehaviour
    {
        private enum Slide
        {
            None,
            Out,
            In,
        }

        // As fast as the platform in the hangar of the mockups.
        private const float k_SlideOutDuration = 0.26f;
        private const float k_SlideInDuration = 0.34f;

        // Beyond the edge of the view, so not even a glint of the platform peeks in.
        private const float k_OutOfSightMargin = 0.25f;

        // The plane along the bottom edge of the view, as Unity lists the planes of a view.
        private const int k_BottomPlane = 2;

        [SerializeField] private GameObject m_mainCamera;
        [SerializeField] private GameObject m_shopCamera;
        [SerializeField] private GameObject m_building;
        [Tooltip("The model of the shown ship is created here.")]
        [SerializeField] private Transform m_stand;
        [Tooltip("The platform under the stand, which slides out of sight with the ship and back in with another one.")]
        [SerializeField] private Transform m_platform;

        private readonly Plane[] m_viewPlanes = new Plane[6];
        private GameObject m_model;
        private Camera m_camera;
        private Slide m_slide;
        private float m_slideTime;
        private Vector3 m_direction;
        private float m_distance;
        private Vector3 m_platformRest;
        private Vector3 m_standRest;
        private Action m_showFamily;

        public bool IsSliding => m_slide != Slide.None;

        // For tests, which follow the slide.
        internal Transform Platform => m_platform;

        internal Transform Stand => m_stand;

        internal Camera ShopCamera => m_camera;

        private void Awake()
        {
            m_camera = m_shopCamera.GetComponent<Camera>();
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
            if (m_slide == Slide.Out)
            {
                float outTime = Mathf.Clamp01(m_slideTime / k_SlideOutDuration);
                Place(m_distance * Easing.EaseIn(outTime));
                if (outTime >= 1f)
                {
                    SlideInNextFamily();
                }

                return;
            }

            float inTime = Mathf.Clamp01(m_slideTime / k_SlideInDuration);
            Place(m_distance * (1f - Easing.EaseOut(inTime)));
            if (inTime >= 1f)
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
        /// Slides the platform with the shown ship out of sight towards the bottom of the screen, shows another family
        /// once it is out and slides the platform back in from there. A step while the platform slides does nothing.
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

            // Along the floor towards the camera, which is down the screen.
            m_direction = Vector3.ProjectOnPlane(-m_camera.transform.forward, Vector3.up).normalized;
            m_distance = GetDistanceOutOfSight();
            m_slideTime = 0f;
            m_slide = Slide.Out;
        }

        // Out of sight, the next family takes the stand and comes back from as far out as its ship needs.
        private void SlideInNextFamily()
        {
            Place(0f);
            ShowFamily();
            m_distance = GetDistanceOutOfSight();
            m_slideTime = 0f;
            m_slide = Slide.In;
            Place(m_distance);
        }

        private void ShowFamily()
        {
            Action showFamily = m_showFamily;
            m_showFamily = null;
            showFamily();
        }

        private void Place(float distance)
        {
            Vector3 shift = m_direction * distance;
            m_platform.position = m_platformRest + shift;
            m_stand.position = m_standRest + shift;
        }

        // How far the platform and the ship on it slide until no corner of them is above the bottom edge of the view.
        // A corner leaves the view where it passes the plane along that edge, which it nears by the share of the way
        // that points out of the view.
        private float GetDistanceOutOfSight()
        {
            Bounds bounds = GetBounds(m_platform);
            bounds.Encapsulate(GetBounds(m_stand));
            GeometryUtility.CalculateFrustumPlanes(m_camera, m_viewPlanes);
            Plane bottom = m_viewPlanes[k_BottomPlane];
            float nearing = -Vector3.Dot(bottom.normal, m_direction);
            float distance = 0f;
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = new Vector3(
                    (i & 1) == 0 ? bounds.min.x : bounds.max.x,
                    (i & 2) == 0 ? bounds.min.y : bounds.max.y,
                    (i & 4) == 0 ? bounds.min.z : bounds.max.z);
                distance = Mathf.Max(distance, bottom.GetDistanceToPoint(corner) / nearing);
            }

            return distance + k_OutOfSightMargin;
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

            if (m_slide == Slide.Out)
            {
                ShowFamily();
            }

            Place(0f);
            m_slide = Slide.None;
        }
    }
}
