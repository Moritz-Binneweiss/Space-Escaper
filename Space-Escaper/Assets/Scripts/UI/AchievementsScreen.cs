using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The achievements, opened from the main menu: each with its reward and how far it is, the rewards to claim at
    /// the top, and the share unlocked in the ring next to the title.
    /// </summary>
    public class AchievementsScreen : MonoBehaviour
    {
        // Where the round end of the ring's fill sits, the middle of its stroke, as in the mockup.
        private const float k_RingRadius = 25f;

        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private ScreenManager m_screens;
        [SerializeField] private AchievementTracker m_achievements;
        [SerializeField] private Button m_backButton;
        [Tooltip("The column of entries, one per achievement. The screen puts them in order.")]
        [SerializeField] private RectTransform m_list;
        [Tooltip("The green part of the ring, filled clockwise from the top.")]
        [SerializeField] private Image m_ringFill;
        [Tooltip("The round end of the fill at the top.")]
        [SerializeField] private GameObject m_ringStart;
        [Tooltip("The round end of the fill where it stops.")]
        [SerializeField] private RectTransform m_ringEnd;
        [SerializeField] private LocalizedText m_shareText;
        [SerializeField] private AchievementLook m_inProgressLook;
        [SerializeField] private AchievementLook m_toClaimLook;
        [SerializeField] private AchievementLook m_claimedLook;
        [SerializeField] private AchievementLook m_comingSoonLook;

        private AchievementEntry[] m_entries;
        private int[] m_entryPlaces;

        // For tests, which tap them like a player.
        internal IReadOnlyList<AchievementEntry> Entries => m_entries;

        internal Button BackButton => m_backButton;

        private void Awake()
        {
            m_entries = m_list.GetComponentsInChildren<AchievementEntry>(true);
            m_entryPlaces = new int[m_entries.Length];
        }

        private void OnEnable()
        {
            m_backButton.onClick.AddListener(BackButton_Clicked);
            foreach (AchievementEntry entry in m_entries)
            {
                entry.ClaimClicked += Entry_ClaimClicked;
            }

            ShowEntries();
            ShowShareUnlocked();

            // Opens at the top, where the rewards to claim are.
            m_list.anchoredPosition = Vector2.zero;
        }

        private void OnDisable()
        {
            m_backButton.onClick.RemoveListener(BackButton_Clicked);
            foreach (AchievementEntry entry in m_entries)
            {
                entry.ClaimClicked -= Entry_ClaimClicked;
            }
        }

        // The achievements leave the state of the game as it is, so going back shows the screen of the
        // state, the main menu.
        private void BackButton_Clicked()
        {
            m_screens.Show(m_gameManager.State);
        }

        // A claimed entry stays where it is until the screen opens again, so the list does not move under the finger.
        private void Entry_ClaimClicked(AchievementEntry entry)
        {
            if (!m_achievements.TryClaim(entry.Achievement))
            {
                return;
            }

            AudioSystem.Instance.PlayCoinPickup();
            ShowEntry(entry);
        }

        // Shows every entry in its state and puts them in order: the rewards to claim first, then what is on its way
        // and what comes later, the claimed ones last. Within each, the order of the catalog.
        private void ShowEntries()
        {
            int placesPerState = m_achievements.Achievements.Count + 1;
            for (int i = 0; i < m_entries.Length; i++)
            {
                AchievementState state = ShowEntry(m_entries[i]);
                m_entryPlaces[i] = GetListPlace(state) * placesPerState + GetCatalogPlace(m_entries[i].Achievement);
            }

            Array.Sort(m_entryPlaces, m_entries);
            for (int i = 0; i < m_entries.Length; i++)
            {
                m_entries[i].transform.SetSiblingIndex(i);
            }
        }

        private AchievementState ShowEntry(AchievementEntry entry)
        {
            AchievementData achievement = entry.Achievement;
            AchievementState state = m_achievements.GetState(achievement);
            entry.Show(state, m_achievements.GetProgress(achievement), m_achievements.GetTarget(achievement),
                GetLook(state));
            return state;
        }

        private void ShowShareUnlocked()
        {
            float share = m_achievements.ShareUnlocked;
            m_ringFill.fillAmount = share;
            m_ringStart.SetActive(share > 0f);
            m_ringEnd.gameObject.SetActive(share > 0f);
            float angle = share * 2f * Mathf.PI;
            m_ringEnd.anchoredPosition = new Vector2(k_RingRadius * Mathf.Sin(angle), k_RingRadius * Mathf.Cos(angle));

            // Rounded down, so 100 % means all of them.
            m_shareText.ShowNumbers(Mathf.Floor(share * 100f));
        }

        private AchievementLook GetLook(AchievementState state)
        {
            switch (state)
            {
                case AchievementState.ToClaim:
                    return m_toClaimLook;

                case AchievementState.Claimed:
                    return m_claimedLook;

                case AchievementState.ComingSoon:
                    return m_comingSoonLook;

                default:
                    return m_inProgressLook;
            }
        }

        private int GetCatalogPlace(AchievementData achievement)
        {
            IReadOnlyList<AchievementData> achievements = m_achievements.Achievements;
            for (int i = 0; i < achievements.Count; i++)
            {
                if (achievements[i] == achievement)
                {
                    return i;
                }
            }

            return achievements.Count;
        }

        private static int GetListPlace(AchievementState state)
        {
            switch (state)
            {
                case AchievementState.ToClaim:
                    return 0;

                case AchievementState.InProgress:
                    return 1;

                case AchievementState.ComingSoon:
                    return 2;

                default:
                    return 3;
            }
        }
    }
}
