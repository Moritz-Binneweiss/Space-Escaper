using System;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// Shows one screen of the UI at a time: the screen of the state the game is in, or the settings, the colorways,
    /// the credits, the achievements or the collectibles over it. The achievements slide in over the main menu from
    /// the left and the collectibles from the right, the sides of their buttons, and the pause rises over the run from
    /// below like a tablet, which a finger can also swipe away. Each goes back out the way it came, every other change
    /// fades. The hangar and the scene change behind a curtain in the ground of the colorway, which hides the cut of
    /// the camera and the loading.
    /// </summary>
    public class ScreenManager : MonoBehaviour
    {
        private enum Motion
        {
            None,
            Fade,
            SlideIn,
            SlideOut,
            Settle,
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

        // A sheet a finger lets go of too early springs back into its place this fast.
        private const float k_SettleDuration = 0.2f;

        // A finger swipes the sheet of the pause away once it has dragged it a quarter of its way out, or would have
        // within this many seconds at the speed it lets go with.
        private const float k_SwipeAwayShare = 0.25f;
        private const float k_SwipeLookAhead = 0.2f;

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
        private Canvas m_canvas;
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
        private Vector2 m_pauseSheetPlace;
        private CanvasRenderer m_pauseDim;
        private Motion m_motion;
        private float m_motionTime;
        private float m_startShown;
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
            m_canvas = GetComponent<Canvas>();
            m_curtainGroup = m_curtain.GetComponent<CanvasGroup>();
            m_pauseDim = m_pauseMenu.GetComponent<Graphic>().canvasRenderer;
            m_pauseSheetPlace = m_pauseSheet.anchoredPosition;

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

        /// <summary>
        /// Moves the sheet of the pause with a finger, by its move in pixels, up positive. It goes down to out of sight
        /// but not above its place, and the dim of the pause fades with it. Only while the pause stands.
        /// </summary>
        public void DragPauseSheet(float pixels)
        {
            if (m_shownScreen != m_pauseMenu || (m_motion != Motion.None && m_motion != Motion.Settle))
            {
                return;
            }

            // The finger takes over a sheet that springs back.
            m_motion = Motion.None;
            Vector2 offset = GetPauseSheetOffset();
            float shown = GetShown(m_pauseSheet.anchoredPosition, m_pauseSheetPlace, offset);
            ShowPauseSheet(Mathf.Clamp01(shown + pixels / m_canvas.scaleFactor / -offset.y), offset);
        }

        /// <summary>
        /// Whether a finger that lets go of the sheet of the pause at this speed, in pixels per second downwards,
        /// swipes it away: when it has dragged it a quarter of its way out, or would have a moment later.
        /// </summary>
        public bool IsPauseSheetSwipedAway(float pixelsPerSecond)
        {
            if (m_shownScreen != m_pauseMenu)
            {
                return false;
            }

            Vector2 offset = GetPauseSheetOffset();
            float way = -offset.y;
            float dragged = (1f - GetShown(m_pauseSheet.anchoredPosition, m_pauseSheetPlace, offset)) * way;
            float ahead = Mathf.Max(0f, pixelsPerSecond / m_canvas.scaleFactor) * k_SwipeLookAhead;
            return dragged + ahead > way * k_SwipeAwayShare;
        }

        /// <summary>
        /// Lets the sheet of the pause spring back into its place after a finger let go of it too early.
        /// </summary>
        public void SettlePauseSheet()
        {
            if (m_shownScreen != m_pauseMenu || m_motion != Motion.None)
            {
                return;
            }

            m_panel = m_pauseSheet;
            m_panelDim = m_pauseDim;
            m_panelPlace = m_pauseSheetPlace;
            m_panelOffset = GetPauseSheetOffset();
            m_startShown = GetShown(m_pauseSheet.anchoredPosition, m_panelPlace, m_panelOffset);
            if (m_startShown >= 1f)
            {
                return;
            }

            m_motion = Motion.Settle;
            m_motionTime = 0f;
            ShowMotion();
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

            // A change in the middle of another one ends that one first. A sheet springing back after a finger goes
            // on from where it is, the pause puts it back in its place once it is gone.
            if (m_motion == Motion.Settle)
            {
                m_motion = Motion.None;
            }
            else
            {
                EndMotion();
            }

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
                    ShowAtRest(previous);
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
                // The pause slides only its sheet, its dim fades over the run. A finger may have moved the sheet out
                // of its place, it leaves from there.
                bool isPause = sliding == m_pauseMenu;
                m_panel = isPause ? m_pauseSheet : (RectTransform)sliding.transform;
                m_panelDim = isPause ? m_pauseDim : null;
                m_panelPlace = isPause ? m_pauseSheetPlace : m_panel.anchoredPosition;
                m_panelOffset = GetOffsetOutOfSight(m_panel, side) + (m_panel.anchoredPosition - m_panelPlace);
                m_startShown = m_motion == Motion.SlideIn
                    ? 0f
                    : GetShown(m_panel.anchoredPosition, m_panelPlace, m_panelOffset);
            }

            m_leavingScreen = leaving;
            m_motionTime = 0f;
            leaving.GetComponent<CanvasGroup>().blocksRaycasts = false;
            entering.GetComponent<CanvasGroup>().blocksRaycasts = true;
            ShowMotion();
        }

        private void ShowMotion()
        {
            float duration = GetDuration();
            float time = duration > 0f ? Mathf.Clamp01(m_motionTime / duration) : 1f;
            float shown = GetShownAbove(time);
            switch (m_motion)
            {
                case Motion.Fade:
                    m_aboveGroup.alpha = shown;
                    m_belowGroup.alpha = GetAlphaBelow(shown, m_aboveGround);
                    break;

                case Motion.Settle:
                    ShowPanel(shown);
                    break;

                default:
                    m_belowGroup.alpha = GetAlphaBelowSlide(shown);
                    ShowPanel(shown);
                    break;
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
                    // Let go by a finger, the sheet goes on at once instead of starting slowly.
                    float gone = m_startShown < 1f ? Easing.EaseOut(time) : Easing.EaseIn(time);
                    return m_startShown * (1f - gone);

                case Motion.Settle:
                    return Mathf.Lerp(m_startShown, 1f, Easing.EaseOut(time));

                default:
                    float faded = Easing.EaseInOut(time);
                    return m_isAboveEntering ? faded : 1f - faded;
            }
        }

        // The screen below a sliding one shows as much as the sliding one leaves free of it. After a finger has moved
        // the sheet of the pause, the run below comes from nothing, as it did not show while the finger moved.
        private float GetAlphaBelowSlide(float shown)
        {
            if (m_motion == Motion.SlideOut && m_startShown > 0f)
            {
                return 1f - shown / m_startShown;
            }

            return 1f - shown;
        }

        private float GetDuration()
        {
            switch (m_motion)
            {
                case Motion.SlideIn:
                    return k_SlideInDuration;

                case Motion.SlideOut:
                    // Only the rest of the way, after a finger.
                    return k_SlideOutDuration * m_startShown;

                case Motion.Settle:
                    return k_SettleDuration;

                default:
                    return k_FadeDuration;
            }
        }

        private void ShowPanel(float shown)
        {
            m_panel.anchoredPosition = m_panelPlace + m_panelOffset * (1f - shown);
            if (m_panelDim != null)
            {
                m_panelDim.SetAlpha(shown);
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
                ShowPanel(1f);
            }

            bool isSettle = m_motion == Motion.Settle;
            m_motion = Motion.None;
            if (isSettle)
            {
                return;
            }

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

        // From the place of the sheet of the pause to just below the screen, wherever a finger holds it.
        private Vector2 GetPauseSheetOffset()
        {
            return GetOffsetOutOfSight(m_pauseSheet, Side.Bottom) + (m_pauseSheet.anchoredPosition - m_pauseSheetPlace);
        }

        private void ShowPauseSheet(float shown, Vector2 offset)
        {
            m_pauseSheet.anchoredPosition = m_pauseSheetPlace + offset * (1f - shown);
            m_pauseDim.SetAlpha(shown);
        }

        // How much of a panel at this position is in: 1 in its place, 0 out of sight.
        private static float GetShown(Vector2 position, Vector2 place, Vector2 offset)
        {
            return 1f - Vector2.Dot(position - place, offset) / offset.sqrMagnitude;
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

        // The pause also puts its sheet back, which a finger may have left out of its place.
        private void ShowAtRest(GameObject screen)
        {
            CanvasGroup group = screen.GetComponent<CanvasGroup>();
            group.alpha = 1f;
            group.blocksRaycasts = true;
            if (screen == m_pauseMenu)
            {
                m_pauseSheet.anchoredPosition = m_pauseSheetPlace;
                m_pauseDim.SetAlpha(1f);
            }
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
