using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Framework.UI;

namespace BoostBlasters.Menus
{
    public class ProfilesMenu : MenuScreen
    {
        [SerializeField]
        private GameObject m_selectPanelPrefab;

        [Header("UI Elements")]
        [SerializeField] private RectTransform m_selectContent;
        [SerializeField] private Image m_arrowLeft;
        [SerializeField] private Image m_arrowRight;
        [SerializeField] private Text m_pageText;
        [SerializeField] private Button m_backButton;
        [SerializeField] private GameObject m_mainContent;
        [SerializeField] private Button m_closeButton;
        [SerializeField] private Button m_editButton;
        [SerializeField] private Button m_deleteButton;
        [SerializeField] private Text m_profleNameText;
        [SerializeField] private Text m_raceCountText;
        [SerializeField] private Text m_winRateText;

        [Header("Options")]
        [SerializeField]
        private int m_selectPanelCount = 15;

        private List<PlayerProfilePanel> m_selectPanels;
        private int m_page;
        private PlayerProfile m_selectedProfile;
        private PlayerProfilePanel m_selectedPanel;

        public override void InitMenu()
        {
            m_backButton.onClick.AddListener(   () => Menu.SetMenu(((MainMenu)Menu).Root, MenuBase.TransitionSound.Back));
            m_closeButton.onClick.AddListener(  () => CloseSelectedProfile());
            m_editButton.onClick.AddListener(   () => EditSelectedProfile());
            m_deleteButton.onClick.AddListener( () => ConfirmProfileDelete());

            m_selectPanels = new List<PlayerProfilePanel>();

            for (int i = 0; i < m_selectPanelCount; i++)
            {
                m_selectPanels.Add(Instantiate(m_selectPanelPrefab, m_selectContent).AddComponent<PlayerProfilePanel>());
            }
        }

        protected override void OnResetMenu(bool fullReset)
        {
            if (fullReset)
            {
                m_page = 0;
                ViewPage(0);
            }
            else
            {
                ViewPage(m_page);
            }
        }

        protected override void OnBack()
        {
            if (m_selectedProfile != null)
            {
                CloseSelectedProfile();
            }
            else
            {
                base.OnBack();
            }
        }
        
        protected override void OnUpdateGraphics()
        {
            IReadOnlyList<PlayerProfile> profiles = PlayerProfileManager.Instance.Profiles;

            int maxPage = (profiles.Count / m_selectPanelCount);
            m_pageText.text = (m_page + 1) + "/" + (maxPage + 1);
            
            PlayerProfilePanel selectedPanel = null;
            foreach (PlayerProfilePanel panel in m_selectPanels)
            {
                if (panel.gameObject == EventSystem.current.currentSelectedGameObject)
                {
                    selectedPanel = panel;
                }
                panel.UpdateGraphics(selectedPanel == panel, false);
            }
            
            m_arrowLeft.enabled = m_page > 0 && selectedPanel != null;
            m_arrowRight.enabled = m_page < maxPage && selectedPanel != null;

            PlayerProfile menuProfile = m_selectedProfile ?? (selectedPanel != null ? selectedPanel.Profile : null);

            m_mainContent.SetActive(menuProfile != null);

            if (m_mainContent.activeInHierarchy)
            {
                m_profleNameText.text = menuProfile.Name;

                IReadOnlyList<RaceResult> results = menuProfile.AllResults;
                m_raceCountText.text = results.Count.ToString();

                float winRate = 0;
                if (results.Count > 0)
                {
                    winRate = 100 * ((float)results.Count(r => r.Rank == 1) / results.Count);
                }

                m_winRateText.text = winRate.ToString("N2") + "%";
            }
        }

        private void OnSelect(PlayerProfilePanel panel, PlayerProfile profile, PlayerProfilePanel.Mode mode)
        {
            m_selectedPanel = panel;
            m_selectedProfile = profile;
            m_mainContent.SetActive(m_selectedProfile != null);

            EventSystem.current.SetSelectedGameObject(null);
            HandleSelection();

            RemeberLastSelection = true;
            
            if (mode == PlayerProfilePanel.Mode.AddNew)
            {
                MainMenu menu = (MainMenu)Menu;
                menu.ProfileName.EditProfile(PlayerProfileManager.Instance.AddNewProfile(), true, menu.Profiles, null);
            }
        }

        protected override void HandleSelection()
        {
            Selectable selectProfile = m_selectPanels[0].isActiveAndEnabled ? m_selectPanels[0].GetComponent<Selectable>() : m_backButton;
            DefaultSelectionOverride = m_selectedProfile != null ? m_closeButton : selectProfile;

            base.HandleSelection();
        }

        private void CloseSelectedProfile()
        {
            RemeberLastSelection = false;

            if (m_selectedPanel != null)
            {
                GameObject toSelect = m_selectPanels[Mathf.Min(m_selectPanels.IndexOf(m_selectedPanel), m_selectPanels.Count - 1)].gameObject;
                EventSystem.current.SetSelectedGameObject(toSelect);
            }
            m_selectedProfile = null;

            Menu.PlayCancelSound();
        }

        private void EditSelectedProfile()
        {
            MainMenu menu = (MainMenu)Menu;

            menu.SetMenu(menu.ProfileName);
            menu.ProfileName.EditProfile(m_selectedProfile, false, menu.Profiles, null);
        }

        private void ConfirmProfileDelete()
        {
            MainMenu menu = (MainMenu)Menu;
            menu.Confirm.ConfirmAction("Delete Profile:   " + m_selectedProfile.Name, DeleteSelectedProfile, menu.Profiles);
        }

        private void DeleteSelectedProfile(bool confirmed)
        {
            if (confirmed)
            {
                PlayerProfileManager.Instance.DeleteProfile(m_selectedProfile);
                CloseSelectedProfile();
                ViewPage(m_page);
            }
        }

        private void OnMove(AxisEventData eventData)
        {
            switch (eventData.moveDir)
            {
                case MoveDirection.Left: ChangePage(-1); break;
                case MoveDirection.Right: ChangePage(1); break;
            }
        }

        private void ChangePage(int offset)
        {
            int oldPage = m_page;
            m_page = Mathf.Clamp(m_page + offset, 0, PlayerProfileManager.Instance.Profiles.Count / m_selectPanelCount);

            if (m_page != oldPage)
            {
                ViewPage(m_page);
                Menu.PlaySelectSound();
            }
        }

        private void ViewPage(int page)
        {
            m_page = Mathf.Clamp(page, 0, PlayerProfileManager.Instance.Profiles.Count / m_selectPanelCount);

            for (int i = 0; i < m_selectPanels.Count; i++)
            {
                bool isAddNew = (i == 0 && m_page == 0);
                int index = Mathf.Max((m_page * m_selectPanelCount) + i - 1, 0);

                IReadOnlyList<PlayerProfile> profiles = PlayerProfileManager.Instance.Profiles;
                PlayerProfilePanel.Mode mode = isAddNew ? PlayerProfilePanel.Mode.AddNew : PlayerProfilePanel.Mode.Profile;

                m_selectPanels[i].SetProfile((!isAddNew && index < profiles.Count) ? profiles[index] : null, mode, OnSelect, OnMove);
            }

            if (m_selectPanels[0].isActiveAndEnabled)
            {
                UIHelper.SetNavigationVertical(m_selectContent, null, m_backButton, null, null);
            }
            else
            {
                Navigation tempNav;

                tempNav = m_backButton.navigation;
                tempNav.selectOnUp = null;
                m_backButton.navigation = tempNav;
            }
        }
    }
}
