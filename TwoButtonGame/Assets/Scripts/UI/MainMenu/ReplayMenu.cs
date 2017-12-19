using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Framework.UI;

namespace BoostBlasters.Menus
{
    public class ReplayMenu : MenuScreen
    {
        [SerializeField]
        private GameObject m_selectPanelPrefab;

        [Header("UI Elements")]
        [SerializeField] private RectTransform m_replayListContent;
        [SerializeField] private Image m_arrowLeft;
        [SerializeField] private Image m_arrowRight;
        [SerializeField] private Text m_pageText;
        [SerializeField] private Button m_backButton;
        [SerializeField] private GameObject m_mainPanel;
        [SerializeField] private Image m_levelPreview;
        [SerializeField] private Text m_levelNameText;
        [SerializeField] private Text m_lapsText;
        [SerializeField] private Text m_finishedText;
        [SerializeField] private RectTransform m_resultsContent;

        [Header("Options")]
        [SerializeField]
        private int m_panelCount = 15;

        private List<ReplayInfo> m_infos;
        private List<ReplayPanel> m_replayPanels;
        private List<PlayerResultPanel> m_playerResults;
        private int m_infoPage;
        
        public override void InitMenu()
        {
            MainMenu menu = (MainMenu)Menu;

            m_backButton.onClick.AddListener(() => Menu.SetMenu(menu.Root, MenuBase.TransitionSound.Back));

            m_infos = new List<ReplayInfo>();
            m_replayPanels = new List<ReplayPanel>();

            for (int i = 0; i < m_panelCount; i++)
            {
                m_replayPanels.Add(Instantiate(m_selectPanelPrefab, m_replayListContent).AddComponent<ReplayPanel>());
            }

            m_playerResults = new List<PlayerResultPanel>();

            PlayerResultPanel resultsTemplate = m_resultsContent.GetComponentInChildren<PlayerResultPanel>();
            m_playerResults.Add(resultsTemplate);

            for (int i = 0; i < Consts.MAX_PLAYERS - 1; i++)
            {
                m_playerResults.Add(Instantiate(resultsTemplate, m_resultsContent));
            }
        }

        protected override void OnResetMenu(bool fullReset)
        {
            if (enabled)
            {
                ReplayManager.Instance.RefreshReplays();
                m_infos = ReplayManager.Instance.Replays;
            }
            m_infoPage = 0;
            ViewPage(0);
        }

        protected override void OnUpdate()
        {
            m_infos = ReplayManager.Instance.Replays;
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
                ReplayInfo info = selectedReplay.ReplayInfo;
                RaceResult[] results = info.RaceResults.OrderBy(r => r.Rank).ToArray();

                m_levelPreview.sprite = info.RaceParams.LevelConfig.Preview;
                m_levelNameText.text = info.RaceParams.LevelConfig.Name;
                m_lapsText.text = info.RaceParams.Laps.ToString();
                m_finishedText.text = results.Any(r => r.Finished) ? "Yes" : "No";

                for (int i = 0; i < m_playerResults.Count; i++)
                {
                    m_playerResults[i].SetResults((i < results.Length) ? results[i] : null);
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
                Menu.PlaySelectSound();
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
                DefaultSelectionOverride = UIHelper.SetNavigationVertical(m_replayListContent, null, m_backButton, null, null).First();
            }
            else
            {
                DefaultSelectionOverride = m_backButton;

                Navigation tempNav;

                tempNav = m_backButton.navigation;
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
