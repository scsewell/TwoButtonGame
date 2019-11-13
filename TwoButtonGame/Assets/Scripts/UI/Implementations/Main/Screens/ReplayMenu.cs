﻿using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Framework.UI;

using BoostBlasters.Replays;
using BoostBlasters.Races;

namespace BoostBlasters.UI.MainMenus
{
    public class ReplayMenu : MenuScreen<MainMenu>
    {
        [SerializeField]
        private GameObject m_selectPanelPrefab = null;

        [Header("UI Elements")]

        [SerializeField] private RectTransform m_replayListContent = null;
        [SerializeField] private Image m_arrowLeft = null;
        [SerializeField] private Image m_arrowRight = null;
        [SerializeField] private Text m_pageText = null;
        [SerializeField] private Button m_backButton = null;
        [SerializeField] private GameObject m_mainPanel = null;
        [SerializeField] private Image m_levelPreview = null;
        [SerializeField] private Text m_levelNameText = null;
        [SerializeField] private Text m_lapsText = null;
        [SerializeField] private Text m_finishedText = null;
        [SerializeField] private RectTransform m_resultsContent = null;

        [Header("Options")]

        [SerializeField]
        private int m_panelCount = 15;

        private List<RecordingInfo> m_infos = null;
        private List<ReplayPanel> m_replayPanels = null;
        private List<PlayerResultPanel> m_playerResults = null;
        private int m_infoPage = 0;
        
        public override void InitMenu()
        {
            m_backButton.onClick.AddListener(() => Menu.SetMenu(Menu.Root, TransitionSound.Back));

            m_infos = new List<RecordingInfo>();
            m_replayPanels = new List<ReplayPanel>();

            for (int i = 0; i < m_panelCount; i++)
            {
                m_replayPanels.Add(Instantiate(m_selectPanelPrefab, m_replayListContent).AddComponent<ReplayPanel>());
            }

            m_playerResults = new List<PlayerResultPanel>();

            PlayerResultPanel resultsTemplate = m_resultsContent.GetComponentInChildren<PlayerResultPanel>();
            m_playerResults.Add(resultsTemplate);

            for (int i = 0; i < Consts.MAX_RACERS - 1; i++)
            {
                m_playerResults.Add(Instantiate(resultsTemplate, m_resultsContent));
            }
        }

        protected override void OnResetMenu(bool fullReset)
        {
            if (enabled)
            {
                RecordingManager.Instance.RefreshRecordingsAsync();
                m_infos = RecordingManager.Instance.Replays;
            }
            m_infoPage = 0;
            ViewPage(0);
        }

        protected override void OnUpdate()
        {
            m_infos = RecordingManager.Instance.Replays;
        }

        protected override void OnUpdateGraphics()
        {
            int maxPage = GetMaxPageCount();
            m_pageText.text = (m_infoPage + 1) + "/" + (maxPage + 1);

            ViewPage(m_infoPage);

            GameObject selected = EventSystem.current.currentSelectedGameObject;

            ReplayPanel selectedReplay = null;
            foreach (ReplayPanel replay in m_replayPanels)
            {
                if (replay.gameObject == selected)
                {
                    selectedReplay = replay;
                }
                replay.UpdateGraphics();
            }

            m_mainPanel.SetActive(selectedReplay != null);

            m_arrowLeft.enabled = m_infoPage > 0 && m_mainPanel.activeInHierarchy;
            m_arrowRight.enabled = m_infoPage < maxPage && m_mainPanel.activeInHierarchy;

            if (m_mainPanel.activeInHierarchy)
            {
                RecordingInfo info = selectedReplay.ReplayInfo;
                RaceResult[] results = info.RaceResults.OrderBy(r => r.Rank).ToArray();

                m_levelPreview.sprite = info.RaceParams.Level.Preview;
                m_levelNameText.text = info.RaceParams.Level.Name;
                m_lapsText.text = info.RaceParams.Laps.ToString();
                m_finishedText.text = results.Any(r => r.Finished) ? "Yes" : "No";

                for (int i = 0; i < m_playerResults.Count; i++)
                {
                    if (i < results.Length)
                    {
                        m_playerResults[i].SetResults(info.RaceParams.Racers[i].Profile, results[i]);
                    }
                    else
                    {
                        m_playerResults[i].SetResults(null, null);
                    }
                }
            }
        }

        public void ViewPreviousReplays()
        {
            ChangePage(-1);
        }

        public void ViewNextReplays()
        {
            ChangePage(1);
        }

        private void ChangePage(int offset)
        {
            int oldPage = m_infoPage;

            m_infoPage = Mathf.Clamp(m_infoPage + offset, 0, GetMaxPageCount());

            if (m_infoPage != oldPage)
            {
                ViewPage(m_infoPage);
                Menu.Sound.PlaySelectSound();
            }
        }

        private void ViewPage(int page)
        {
            for (int i = 0; i < m_replayPanels.Count; i++)
            {
                int index = (page * m_panelCount) + i;

                m_replayPanels[i].SetRecording((index < m_infos.Count) ? m_infos[index] : null);
            }

            if (m_replayPanels[0].isActiveAndEnabled)
            {
                DefaultSelectionOverride = UIHelper.SetNavigationVertical(new NavConfig()
                {
                    parent = m_replayListContent,
                    down = m_backButton,
                }
                ).First();
            }
            else
            {
                DefaultSelectionOverride = m_backButton;

                Navigation tempNav = m_backButton.navigation;
                tempNav.selectOnUp = null;
                m_backButton.navigation = tempNav;
            }
        }

        private int GetMaxPageCount()
        {
            return (m_infos.Count / m_panelCount) + (m_infos.Count > 0 && m_infos.Count % m_panelCount == 0 ? -1 : 0);
        }
    }
}
