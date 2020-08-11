using System.Collections.Generic;
using System.Linq;

using BoostBlasters.Profiles;

using Framework.UI;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BoostBlasters.UI.MainMenus
{
    public class ProfilesMenu : MenuScreen
    {
        [Header("Prefabs")]

        [SerializeField] private GameObject m_selectPanelPrefab = null;

        [Header("UI Elements")]

        [SerializeField] private RectTransform m_selectContent = null;
        [SerializeField] private Image m_arrowLeft = null;
        [SerializeField] private Image m_arrowRight = null;
        [SerializeField] private Text m_pageText = null;
        [SerializeField] private Button m_backButton = null;
        [SerializeField] private GameObject m_mainContent = null;
        [SerializeField] private Button m_closeButton = null;
        [SerializeField] private Button m_editButton = null;
        [SerializeField] private Button m_deleteButton = null;
        [SerializeField] private Text m_profleNameText = null;
        [SerializeField] private Text m_raceCountText = null;
        [SerializeField] private Text m_winRateText = null;

        [Header("Options")]

        [SerializeField]
        private int m_selectPanelCount = 15;

        private List<ProfilePanel> m_selectPanels = null;
        private int m_page = 0;
        private Profile m_selectedProfile = null;
        private ProfilePanel m_selectedPanel = null;

        protected override void OnInitialize()
        {
            m_backButton.onClick.AddListener(() => Menu.SwitchTo<RootMenu>(TransitionSound.Back));
            m_closeButton.onClick.AddListener(() => CloseSelectedProfile());
            m_editButton.onClick.AddListener(() => EditSelectedProfile());
            m_deleteButton.onClick.AddListener(() => ConfirmProfileDelete());

            m_selectPanels = new List<ProfilePanel>();

            for (var i = 0; i < m_selectPanelCount; i++)
            {
                m_selectPanels.Add(Instantiate(m_selectPanelPrefab, m_selectContent).AddComponent<ProfilePanel>());
            }
        }

        //protected override void OnResetMenu(bool fullReset)
        //{
        //    if (fullReset)
        //    {
        //        m_page = 0;
        //        ViewPage(0);
        //    }
        //    else
        //    {
        //        ViewPage(m_page);
        //    }
        //}

        public override void Back()
        {
            if (m_selectedProfile != null)
            {
                CloseSelectedProfile();
            }
            else
            {
                base.Back();
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            SetDefaultSelection();
        }

        protected override void OnUpdateVisuals()
        {
            var profiles = ProfileManager.Profiles;

            var maxPage = (profiles.Count / m_selectPanelCount);
            m_pageText.text = (m_page + 1) + "/" + (maxPage + 1);

            ProfilePanel selectedPanel = null;
            foreach (var panel in m_selectPanels)
            {
                if (panel.gameObject == PrimarySelection.Current)
                {
                    selectedPanel = panel;
                }
                //panel.UpdateGraphics(selectedPanel == panel, false);
            }

            m_arrowLeft.enabled = m_page > 0 && selectedPanel != null;
            m_arrowRight.enabled = m_page < maxPage && selectedPanel != null;

            var menuProfile = m_selectedProfile ?? (selectedPanel != null ? selectedPanel.Profile : null);

            m_mainContent.SetActive(menuProfile != null);

            if (m_mainContent.activeInHierarchy)
            {
                m_profleNameText.text = menuProfile.Name;

                var results = menuProfile.RaceResults;
                m_raceCountText.text = results.Count.ToString();

                float winRate = 0;
                if (results.Count > 0)
                {
                    winRate = 100 * ((float)results.Count(r => r.Rank == 1) / results.Count);
                }

                m_winRateText.text = winRate.ToString("N2") + "%";
            }
        }

        private void OnSelect(ProfilePanel panel, Profile profile, ProfilePanel.Mode mode)
        {
            m_selectedPanel = panel;
            m_selectedProfile = profile;
            m_mainContent.SetActive(m_selectedProfile != null);

            SetDefaultSelection();
            PrimarySelection.SelectDefault();

            RemeberLastSelection = true;

            if (mode == ProfilePanel.Mode.AddNew)
            {
                var menu = Menu as MainMenu;
                menu.Get<ProfileNameMenu>().Rename(ProfileManager.CreateProfile(), null, this);
            }
        }

        private void SetDefaultSelection()
        {
            var selectProfile = m_selectPanels[0].isActiveAndEnabled ? m_selectPanels[0].GetComponent<Selectable>() : m_backButton;
            PrimarySelection.DefaultSelectionOverride = (m_selectedProfile != null ? m_closeButton : selectProfile).gameObject;
        }

        private void CloseSelectedProfile()
        {
            RemeberLastSelection = false;

            if (m_selectedPanel != null)
            {
                var toSelect = m_selectPanels[Mathf.Min(m_selectPanels.IndexOf(m_selectedPanel), m_selectPanels.Count - 1)].gameObject;
                PrimarySelection.Current = toSelect;
            }
            m_selectedProfile = null;

            Menu.Sound.PlayCancelSound();
        }

        private void EditSelectedProfile()
        {
            var menu = (MainMenu)Menu;
            menu.Get<ProfileNameMenu>().Rename(m_selectedProfile, null, this);
        }

        private void ConfirmProfileDelete()
        {
            var menu = (MainMenu)Menu;
            menu.Get<ConfirmMenu>().ConfirmAction("Delete Profile:   " + m_selectedProfile.Name, DeleteSelectedProfile, this);
        }

        private void DeleteSelectedProfile(bool confirmed)
        {
            if (confirmed)
            {
                ProfileManager.DeleteProfile(m_selectedProfile);
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
            var oldPage = m_page;
            m_page = Mathf.Clamp(m_page + offset, 0, ProfileManager.Profiles.Count / m_selectPanelCount);

            if (m_page != oldPage)
            {
                ViewPage(m_page);
                Menu.Sound.PlaySelectSound();
            }
        }

        private void ViewPage(int page)
        {
            m_page = Mathf.Clamp(page, 0, ProfileManager.Profiles.Count / m_selectPanelCount);

            for (var i = 0; i < m_selectPanels.Count; i++)
            {
                var isAddNew = (i == 0 && m_page == 0);
                var index = Mathf.Max((m_page * m_selectPanelCount) + i - 1, 0);

                var profiles = ProfileManager.Profiles;
                var mode = isAddNew ? ProfilePanel.Mode.AddNew : ProfilePanel.Mode.Profile;

                //m_selectPanels[i].SetProfile((!isAddNew && index < profiles.Count) ? profiles[index] : null, mode, OnSelect, OnMove);
            }

            if (m_selectPanels[0].isActiveAndEnabled)
            {
                UIHelper.SetNavigationVertical(new NavConfig()
                {
                    parent = m_selectContent,
                    down = m_backButton,
                });
            }
            else
            {
                var tempNav = m_backButton.navigation;
                tempNav.selectOnUp = null;
                m_backButton.navigation = tempNav;
            }
        }
    }
}
