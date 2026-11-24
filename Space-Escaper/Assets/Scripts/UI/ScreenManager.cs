using System;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// Shows one screen of the UI at a time: the screen of the state the game is in, or the settings, the colorways,
    /// the credits, the achievements or the collectibles over it. The achievements slide in over the main menu from
    /// the left and the collectibles from the right, the sides of their buttons, and the pause rises over the run from
    /// below like a tablet. Each goes back out the way it came, every other change fades. The hangar and the scene
    /// change behind a curtain in the ground of the colorway, which hides the cut of the camera and the loading.
    /// </summary>
    public class ScreenManager : MonoBehaviour
    {
        private enum Motion
        {
            None,
            Fade,
            SlideIn,
            SlideOut,
        }

        private enum Side
        {
            None,
            Left,
            Right,
            Bottom,
        }

        private const float k_FadeDuration = 0.2f;

        // In and out as fast as the platform in the hangar of the mockups.
        private const float k_SlideInDuration = 0.34f;
        private const float k_SlideOutDuration = 0.26f;
        private const float k_CurtainCloseDuration = 0.2f;
        private const float k_CurtainOpenDuration = 0.3f;

        // A longer frame, like the two of loading the scene, moves a change on by this much only. So no hitch skips
        // one.
        private const float k_MaxStep = 1f / 30f;

        [SerializeField] private GameObject m_mainMenu;
        [SerializeField] private GameObject m_gameMenu;
        [SerializeField] private GameObject m_pauseMenu;
        [SerializeField] private GameObject m_settings;
        [SerializeField] private GameObject m_colorways;
        [SerializeField] private GameObject m_credits;
        [SerializeField] private GameObject m_achievements;
        [SerializeField] private GameObject m_collectibles;
        [SerializeField] private GameObject m_shop;
        [SerializeField] private GameObject m_deathMenu;
        [Tooltip("The part of the pause that rises from below, while the dim of the pause fades in over the run.")]
        [SerializeField] private RectTransform m_pauseSheet;
        [Tooltip("Covers the screen while the hangar opens or closes and while the scene loads, and takes the taps.")]
        [SerializeField] private GameObject m_curtain;

        private readonly Vector3[] m_corners = new Vector3[4];
        private GameObject[] m_screens;
        private GameObject m_shownScreen;
        private GameObject m_leavingScreen;
        private CanvasGroup m_aboveGroup;
        private CanvasGroup m_belowGroup;
        private bool m_isAboveEntering;
        private float m_aboveGround;
        private RectTransform m_panel;
        private Vector2 m_panelPlace;
        private Vector2 m_panelOffset;
        private CanvasRenderer m_panelDim;
        private CanvasRenderer m_pauseDim;
        private Motion m_motion;
        private float m_motionTime;
        private CanvasGroup m_curtainGroup;
        private Action m_changeBehindCurtain;
        private bool m_isCurtainClosing;
        private float m_curtainTime;
        private float m_curtainStartAlpha;

        // For tests, which tap like a player, once the screen stands.
        internal bool IsChanging => m_motion != Motion.None || m_curtain.activeSelf;

        internal GameObject Curtain => m_curtain;

        internal RectTransform PauseSheet => m_pauseSheet;

        // Behind it nothing shows, so a screen changes there without a motion.
        private bool IsCovered => m_curtain.activeSelf && m_curtainGroup.alpha >= 1f;

        private void Awake()
        {
            m_screens = new[]
            {
                m_mainMenu, m_gameMenu, m_pauseMenu, m_settings, m_colorways, m_credits, m_achievements, m_collectibles,
                m_shop, m_deathMenu
            };
            m_curtainGroup = m_curtain.GetComponent<CanvasGroup>();
            m_pauseDim = m_pauseMenu.GetComponent<Graphic>().canvasRenderer;

            // Every scene starts behind the curtain, also when the app starts. The game starts in the main menu,
            // whichever screen was left switched on while editing the scene.
            m_curtain.SetActive(true);
            OpenCurtain();
            Show(GameState.Menu);
        }

        private void Update()
        {
            float step = Mathf.Min(Time.unscaledDeltaTime, k_MaxStep);
            if (m_motion != Motion.None)
            {
                m_motionTime += step;
                ShowMotion();
            }

            if (m_curtain.activeSelf)
            {
                m_curtainTime += step;
                ShowCurtain();
            }
        }

        public void Show(GameState state)
        {
            Show(GetScreen(state));
        }

        public void ShowSettings()
        {
            Show(m_settings);
        }

        public void ShowColorways()
        {
            Show(m_colorways);
        }

        public void ShowCredits()
        {
            Show(m_credits);
        }

        public void ShowAchievements()
        {
            Show(m_achievements);
        }

        public void ShowCollectibles()
        {
            Show(m_collectibles);
        }

        /// <summary>
        /// Closes the curtain, makes the change behind it and opens the curtain again. Closing, it takes the taps, so
        /// a second change cannot come meanwhile. One that does is dropped.
        /// </summary>
        public void ChangeBehindCurtain(Action change)
        {
            if (m_isCurtainClosing)
            {
                return;
            }

            m_changeBehindCurtain = change;
            m_isCurtainClosing = true;
            m_curtainStartAlpha = m_curtain.activeSelf ? m_curtainGroup.alpha : 0f;
            m_curtainTime = 0f;
            m_curtainGroup.blocksRaycasts = true;
            m_curtain.SetActive(true);
            ShowCurtain();
        }

        private GameObject GetScreen(GameState state)
        {
            switch (state)
            {
                case GameState.Hangar:
                    return m_shop;

                case GameState.Run:
                    return m_gameMenu;

                case GameState.Paused:
                    return m_pauseMenu;

                case GameState.Dead:
                    return m_deathMenu;

                default:
                    return m_mainMenu;
            }
        }

        // The side a screen slides in from over the screen below it and back out to: the achievements and the
        // collectibles over the main menu from the sides of their buttons next to LAUNCH, the pause over the run from
        // below. None for every other pair, which fades.
        private Side GetSide(GameObject screen, GameObject below)
        {
            if (below == m_mainMenu && screen == m_achievements)
            {
                return Side.Left;
            }

            if (below == m_mainMenu && screen == m_collectibles)
            {
                return Side.Right;
            }

            if (below == m_gameMenu && screen == m_pauseMenu)
            {
                return Side.Bottom;
            }

            return Side.None;
        }

        private void Show(GameObject screen)
        {
            if (screen == m_shownScreen)
            {
                return;
            }

            // A change in the middle of another one ends that one first.
            EndMotion();
            GameObject previous = m_shownScreen;
            m_shownScreen = screen;
            foreach (GameObject candidate in m_screens)
            {
                if (candidate != screen && candidate != previous)
                {
                    candidate.SetActive(false);
                }
            }

            screen.SetActive(true);
            if (previous == null || IsCovered)
            {
                if (previous != null)
                {
                    previous.SetActive(false);
                }

                ShowAtRest(screen);
                return;
            }

            StartMotion(previous, screen);
        }

        // A screen with a side slides over the one below it, which shows as much as the sliding one leaves free of it.
        // So its bright parts do not shine through the nearly opaque ground of the one above. All other screens fade,
        // the later one in the UI over the earlier one. The screen leaving takes no more taps.
        private void StartMotion(GameObject leaving, GameObject entering)
        {
            GameObject sliding = null;
            Side side = GetSide(entering, leaving);
            if (side != Side.None)
            {
                m_motion = Motion.SlideIn;
                sliding = entering;
            }
            else
            {
                side = GetSide(leaving, entering);
                m_motion = side != Side.None ? Motion.SlideOut : Motion.Fade;
                sliding = side != Side.None ? leaving : null;
            }

            m_isAboveEntering = sliding != null
                ? sliding == entering
                : entering.transform.GetSiblingIndex() > leaving.transform.GetSiblingIndex();
            GameObject above = m_isAboveEntering ? entering : leaving;
            m_aboveGroup = above.GetComponent<CanvasGroup>();
            m_belowGroup = (m_isAboveEntering ? leaving : entering).GetComponent<CanvasGroup>();
            m_aboveGround = GetGroundOpacity(above);
            if (sliding != null)
            {
                // The pause slides only its sheet, its dim fades over the run.
                bool isPause = sliding == m_pauseMenu;
                m_panel = isPause ? m_pauseSheet : (RectTransform)sliding.transform;
                m_panelDim = isPause ? m_pauseDim : null;
                m_panelPlace = m_panel.anchoredPosition;
                m_panelOffset = GetOffsetOutOfSight(m_panel, side);
            }

            m_leavingScreen = leaving;
            m_motionTime = 0f;
            leaving.GetComponent<CanvasGroup>().blocksRaycasts = false;
            entering.GetComponent<CanvasGroup>().blocksRaycasts = true;
            ShowMotion();
        }

        private void ShowMotion()
        {
            float time = Mathf.Clamp01(m_motionTime / GetDuration(m_motion));
            float shown = GetShownAbove(time);
            if (m_motion == Motion.Fade)
            {
                m_aboveGroup.alpha = shown;
                m_belowGroup.alpha = GetAlphaBelow(shown, m_aboveGround);
            }
            else
            {
                m_belowGroup.alpha = 1f - shown;
                m_panel.anchoredPosition = m_panelPlace + m_panelOffset * (1f - shown);
                if (m_panelDim != null)
                {
                    m_panelDim.SetAlpha(shown);
                }
            }

            if (time >= 1f)
            {
                EndMotion();
            }
        }

        // How far the screen above is in, faded or slid over the one below.
        private float GetShownAbove(float time)
        {
            switch (m_motion)
            {
                case Motion.SlideIn:
                    return Easing.EaseOut(time);

                case Motion.SlideOut:
                    return 1f - Easing.EaseIn(time);

                default:
                    float faded = Easing.EaseInOut(time);
                    return m_isAboveEntering ? faded : 1f - faded;
            }
        }

        private void EndMotion()
        {
            if (m_motion == Motion.None)
            {
                return;
            }

            if (m_motion != Motion.Fade)
            {
                m_panel.anchoredPosition = m_panelPlace;
                if (m_panelDim != null)
                {
                    m_panelDim.SetAlpha(1f);
                }
            }

            m_motion = Motion.None;
            m_leavingScreen.SetActive(false);
            ShowAtRest(m_leavingScreen);
            ShowAtRest(m_shownScreen);
        }

        // From its place to just out of sight on its side: as far as its edge on the other side lies from the edge of
        // the UI it leaves by.
        private Vector2 GetOffsetOutOfSight(RectTransform panel, Side side)
        {
            RectTransform root = (RectTransform)transform;
            panel.GetWorldCorners(m_corners);
            Vector2 min = root.InverseTransformPoint(m_corners[0]);
            Vector2 max = root.InverseTransformPoint(m_corners[2]);
            Rect bounds = root.rect;
            switch (side)
            {
                case Side.Left:
                    return new Vector2(bounds.xMin - max.x, 0f);

                case Side.Right:
                    return new Vector2(bounds.xMax - min.x, 0f);

                default:
                    return new Vector2(0f, bounds.yMin - max.y);
            }
        }

        private static float GetDuration(Motion motion)
        {
            switch (motion)
            {
                case Motion.SlideIn:
                    return k_SlideInDuration;

                case Motion.SlideOut:
                    return k_SlideOutDuration;

                default:
                    return k_FadeDuration;
            }
        }

        // How much of what lies behind a screen its ground covers, nothing for a screen over the world.
        private static float GetGroundOpacity(GameObject screen)
        {
            Graphic ground = screen.GetComponent<Graphic>();
            return ground != null ? ground.color.a : 0f;
        }

        // The screen below a fading one shows exactly as much as the fading one does not, also where it shows through
        // the ground of that one. Plain crossfading would let what lies behind both shine through halfway, as two
        // nearly opaque grounds at half their alpha cover only three quarters. Internal for tests.
        internal static float GetAlphaBelow(float alphaAbove, float groundAbove)
        {
            float through = 1f - groundAbove * alphaAbove;
            return through > 0f ? Mathf.Clamp01((1f - alphaAbove) / through) : 0f;
        }

        private static void ShowAtRest(GameObject screen)
        {
            CanvasGroup group = screen.GetComponent<CanvasGroup>();
            group.alpha = 1f;
            group.blocksRaycasts = true;
        }

        // Once closed, the curtain makes the change and opens again. The change finds it closed, so a screen it shows
        // comes at once.
        private void ShowCurtain()
        {
            if (m_isCurtainClosing)
            {
                float closeTime = Mathf.Clamp01(m_curtainTime / k_CurtainCloseDuration);
                m_curtainGroup.alpha = Mathf.Lerp(m_curtainStartAlpha, 1f, Easing.EaseIn(closeTime));
                if (closeTime >= 1f)
                {
                    Action change = m_changeBehindCurtain;
                    m_changeBehindCurtain = null;
                    OpenCurtain();
                    change();
                }

                return;
            }

            float openTime = Mathf.Clamp01(m_curtainTime / k_CurtainOpenDuration);
            m_curtainGroup.alpha = 1f - Easing.EaseOut(openTime);
            if (openTime >= 1f)
            {
                m_curtain.SetActive(false);
            }
        }

        // Opening, it takes no taps, so the screen behind it does as soon as it shows.
        private void OpenCurtain()
        {
            m_isCurtainClosing = false;
            m_curtainTime = 0f;
            m_curtainGroup.alpha = 1f;
            m_curtainGroup.blocksRaycasts = false;
        }
    }
}
