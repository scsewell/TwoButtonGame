using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Framework.UI;

namespace BoostBlasters.MainMenus
{
    public class ReplayMenu : MenuScreen
    {
        [SerializeField]
        private ReplayPanel m_replayPanelPrefab;

        [Header("UI Elements")]
        [SerializeField] private RectTransform m_replayListContent;
        [SerializeField] private Image m_arrowLeft;
        [SerializeField] private Image m_arrowRight;
        [SerializeField] private Text m_pageText;
        [SerializeField] private Button m_backButton;
        [SerializeField] private GameObject m_resultsPanel;
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

        protected override void Awake()
        {
            base.Awake();

            m_backButton.onClick.AddListener(() => MainMenu.SetMenu(Menu.Root, true));

            m_infos = new List<ReplayInfo>();
            m_replayPanels = new List<ReplayPanel>();

            for (int i = 0; i < m_panelCount; i++)
            {
                ReplayPanel panel = Instantiate(m_replayPanelPrefab, m_replayListContent);
                m_replayPanels.Add(panel);
            }

            m_playerResults = new List<PlayerResultPanel>();

            PlayerResultPanel resultsTemplate = m_resultsContent.GetComponentInChildren<PlayerResultPanel>();
            m_playerResults.Add(resultsTemplate);

            for (int i = 0; i < Consts.MAX_PLAYERS - 1; i++)
            {
                PlayerResultPanel panel = Instantiate(resultsTemplate, m_resultsContent);
                m_playerResults.Add(panel);
            }
        }

        public override void InitMenu(RaceParameters lastRace)
        {
        }

        protected override void OnResetMenu(bool fullReset)
        {
            m_infos = ReplayManager.Instance.GetReplays();
            m_infoPage = 0;
            ViewPage(0);
        }

        protected override void OnUpdateGraphics()
        {
            int maxPage = (m_infos.Count / m_panelCount);
            m_pageText.text = (m_infoPage + 1) + "/" + (maxPage + 1);

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

            m_arrowLeft.enabled = m_infoPage > 0;
            m_arrowRight.enabled = m_infoPage < maxPage;

            m_resultsPanel.SetActive(selectedReplay != null);

            if (selectedReplay != null)
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
            m_infoPage = Mathf.Clamp(m_infoPage + offset, 0, m_infos.Count / m_panelCount);
            if (m_infoPage != oldPage)
            {
                ViewPage(m_infoPage);
                MainMenu.PlaySelectSound();
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
                DefaultSelectionOverride = m_replayPanels[0].GetComponent<Selectable>();

                Navigation explicitNav = new Navigation();
                explicitNav.mode = Navigation.Mode.Explicit;

                UIHelper.SetNavigationVertical(m_replayListContent, explicitNav, explicitNav, explicitNav);
                Selectable last = m_replayPanels.Last(p => p.isActiveAndEnabled).GetComponent<Selectable>();

                Navigation tempNav;

                tempNav = last.navigation;
                tempNav.selectOnDown = m_backButton;
                last.navigation = tempNav;

                tempNav = m_backButton.navigation;
                tempNav.selectOnUp = last;
                m_backButton.navigation = tempNav;
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
    }
}
