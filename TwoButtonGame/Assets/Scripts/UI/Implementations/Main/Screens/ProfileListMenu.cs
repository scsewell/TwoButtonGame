using System.Collections.Generic;

using BoostBlasters.Profiles;

using Framework.UI;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI.MainMenus
{
    public class ProfileListMenu : MenuScreen
    {
        [Header("UI Elements")]

        [SerializeField] private VerticalNavigationBuilder m_profilesLayout = null;
        [SerializeField] private HorizontalNavigationBuilder m_pagesLayout = null;
        [SerializeField] private GameObject m_pageDisplay = null;
        [SerializeField] private TextMeshProUGUI m_pageText = null;
        [SerializeField] private Image m_pageLeftArrow = null;
        [SerializeField] private Image m_pageRightArrow = null;


        private readonly List<ProfilePanel> m_profilePanels = new List<ProfilePanel>();
        private readonly List<GameObject> m_pages = new List<GameObject>();
        private int m_page;

        private int PageCount => Mathf.CeilToInt((float)(1 + ProfileManager.Profiles.Count) / m_profilePanels.Count);


        protected override void OnInitialize()
        {
            GetComponentsInChildren(true, m_profilePanels);

            Menu.Shown += (m) =>
            {
                if (m is RootMenu)
                {
                    m_page = 0;
                }
            };
        }

        protected override void OnShow()
        {
            CreatePages();

            SecondarySelection.DefaultSelectionOverride = m_pages[m_page].gameObject;
        }

        private void CreatePages()
        {
            // the page selection operates on hidden tabs created for each page
            var tabs = m_pagesLayout.transform;

            while (tabs.childCount < PageCount)
            {
                var page = tabs.childCount;

                var go = new GameObject($"Page {page + 1}");
                go.transform.SetParent(tabs, false);

                go.AddComponent<LayoutElement>();
                go.AddComponent<Selectable>();
                go.AddComponent<SoundListener>();
                go.AddComponent<NavigationHandler>();
                go.AddComponent<Tab>().Selected += () => ChangePage(page);

                m_pages.Add(go);
            }

            for (var i = 0; i < tabs.childCount; i++)
            {
                tabs.GetChild(i).gameObject.SetActive(i < PageCount);
            }

            m_pagesLayout.UpdateNavigation();

            ViewPage(m_page);
        }

        private void ChangePage(int page)
        {
            ViewPage(page);
            PrimarySelection.SelectDefault();
        }

        private void ViewPage(int page)
        {
            m_page = Mathf.Clamp(page, 0, PageCount - 1);

            // Update the panels to reflect the profiles for this page. The first panel
            // on the first page is used to add a new profile.
            for (var i = 0; i < m_profilePanels.Count; i++)
            {
                var panel = m_profilePanels[i];

                if (m_page == 0 && i == 0)
                {
                    panel.Init(ProfilePanel.Mode.AddNew, null, OnSelect);
                }
                else
                {
                    var profiles = ProfileManager.Profiles;

                    var index = (m_page * m_profilePanels.Count) + i - 1;
                    var profile = index < profiles.Count ? profiles[index] : null;

                    panel.Init(ProfilePanel.Mode.Profile, profile, OnSelect);
                }
            }

            m_profilesLayout.UpdateNavigation();

            // Update the page display
            var showPages = PageCount > 1;

            if (showPages)
            {
                m_pageText.text = $"{m_page + 1}/{PageCount}";

                m_pageLeftArrow.enabled = Interactable && m_page > 0;
                m_pageRightArrow.enabled = Interactable && m_page < PageCount - 1;
            }

            m_pageDisplay.SetActive(showPages);
        }

        private void OnSelect(ProfilePanel panel)
        {
            switch (panel.CurrentMode)
            {
                case ProfilePanel.Mode.AddNew:
                {
                    void OnCreate(Profile profile)
                    {
                        CreatePages();
                    }

                    Menu.Get<ProfileNameMenu>().CreateNew(OnCreate, this);
                    break;
                }
            }
        }
    }
}
