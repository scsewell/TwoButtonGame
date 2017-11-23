using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Framework.UI;

namespace BoostBlasters.MainMenus
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
        [SerializeField] private Text m_profleName;

        [Header("Options")]
        [SerializeField]
        private int m_selectPanelCount = 15;

        private List<PlayerProfilePanel> m_selectPanels;
        private int m_page;
        private PlayerProfile m_selectedProfile;
        private PlayerProfilePanel m_selectedPanel;

        public override void InitMenu(RaceParameters lastRace)
        {
            m_backButton.onClick.AddListener(   () => MainMenu.SetMenu(Menu.Root, true));
            m_closeButton.onClick.AddListener(  () => CloseSelectedProfile());
            m_editButton.onClick.AddListener(   () => EditSelectedProfile());
            m_deleteButton.onClick.AddListener( () => DeleteSelectedProfile());

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
                panel.UpdateGraphics();
            }

            m_mainContent.SetActive(m_selectedProfile != null);

            m_arrowLeft.enabled = m_page > 0 && selectedPanel != null;
            m_arrowRight.enabled = m_page < maxPage && selectedPanel != null;

            if (m_mainContent.activeInHierarchy)
            {
                m_profleName.text = m_selectedProfile.Name;
            }
        }

        public void OnSelect(PlayerProfilePanel panel, PlayerProfile profile, bool createNew)
        {
            m_selectedPanel = panel;
            m_selectedProfile = profile;
            m_mainContent.SetActive(m_selectedProfile != null);

            EventSystem.current.SetSelectedGameObject(null);
            HandleSelection();

            if (createNew)
            {
                MainMenu.SetMenu(Menu.ProfileName);
                MainMenu.ProfileName.EditProfile(PlayerProfileManager.Instance.AddNewProfile(), true, Menu.Profiles);
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
            if (m_selectedPanel != null)
            {
                GameObject toSelect = m_selectPanels[Mathf.Min(m_selectPanels.IndexOf(m_selectedPanel), m_selectPanels.Count - 1)].gameObject;
                EventSystem.current.SetSelectedGameObject(toSelect);
            }
            m_selectedProfile = null;

            MainMenu.PlayCancelSound();
        }

        private void EditSelectedProfile()
        {
            MainMenu.SetMenu(Menu.ProfileName);
            MainMenu.ProfileName.EditProfile(m_selectedProfile, false, Menu.Profiles);
        }

        private void DeleteSelectedProfile()
        {
            PlayerProfileManager.Instance.DeleteProfile(m_selectedProfile);
            CloseSelectedProfile();
            ViewPage(m_page);
        }

        public void ViewPreviousPage()
        {
            ChangePage(-1);
        }

        public void ViewNextPage()
        {
            ChangePage(1);
        }

        private void ChangePage(int offset)
        {
            int oldPage = m_page;
            m_page = Mathf.Clamp(m_page + offset, 0, PlayerProfileManager.Instance.Profiles.Count / m_selectPanelCount);

            if (m_page != oldPage)
            {
                ViewPage(m_page);
                MainMenu.PlaySelectSound();
            }
        }

        private void ViewPage(int page)
        {
            for (int i = 0; i < m_selectPanels.Count; i++)
            {
                bool isAddNew = (i == 0 && page == 0);
                int index = Mathf.Max((page * m_selectPanelCount) + i - 1, 0);

                IReadOnlyList<PlayerProfile> profiles = PlayerProfileManager.Instance.Profiles;
                m_selectPanels[i].SetProfile((!isAddNew && index < profiles.Count) ? profiles[index] : null, isAddNew);
            }

            if (m_selectPanels[0].isActiveAndEnabled)
            {
                Navigation explicitNav = new Navigation();
                explicitNav.mode = Navigation.Mode.Explicit;

                UIHelper.SetNavigationVertical(m_selectContent, explicitNav, explicitNav, explicitNav);
                Selectable last = m_selectPanels.Last(p => p.isActiveAndEnabled).GetComponent<Selectable>();

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
                Navigation tempNav;

                tempNav = m_backButton.navigation;
                tempNav.selectOnUp = null;
                m_backButton.navigation = tempNav;
            }
        }
    }
}
