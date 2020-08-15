﻿using System.Collections.Generic;

using BoostBlasters.Profiles;

using Framework.UI;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI.MainMenu
{
    public class ProfileListMenu : MenuScreen
    {
        [Header("Prefabs")]

        [SerializeField] private Button m_profilePanelPrefab = null;

        [Header("UI Elements")]

        [SerializeField] private Button m_createProfile = null;
        [SerializeField] private VerticalNavigationBuilder m_buttonsLayout = null;
        [SerializeField] private VerticalNavigationBuilder m_profilesLayout = null;


        private readonly List<Button> m_profilePanels = new List<Button>();


        protected override void OnInitialize()
        {
            m_createProfile.onClick.AddListener(() => CreateNewProfile());

            Menu.Shown += (m) =>
            {
                switch (m)
                {
                    case RootMenu menu:
                        PrimarySelection.SelectDefault();
                        break;
                }
            };
        }

        protected override void OnShow()
        {
            ProfileManager.ProfileRenamed += OnProfileChamged;
            ProfileManager.ProfileDeleted += OnProfileChamged;

            Refresh();
        }

        protected override void OnHide()
        {
            ProfileManager.ProfileRenamed -= OnProfileChamged;
            ProfileManager.ProfileDeleted -= OnProfileChamged;
        }

        private void OnProfileChamged(Profile profile)
        {
            Refresh();
        }

        private void Refresh()
        {
            var profiles = ProfileManager.Profiles;
            var parent = m_profilesLayout.transform;

            for (var i = m_profilePanels.Count; i < profiles.Count; i++)
            {
                var button = Instantiate(m_profilePanelPrefab, parent, false);
                button.gameObject.AddComponent<AutoScrollViewElement>();
                m_profilePanels.Add(button);
            }
            for (var i = 0; i < m_profilePanels.Count; i++)
            {
                var panel = m_profilePanels[i];

                if (i < profiles.Count)
                {
                    var profile = profiles[i];

                    panel.onClick.RemoveAllListeners();
                    panel.onClick.AddListener(() => SelectProfile(profile));
                    panel.GetComponentInChildren<TMP_Text>().text = profile.Name;

                    panel.gameObject.SetActive(true);
                }
                else
                {
                    panel.gameObject.SetActive(false);
                }
            }

            m_buttonsLayout.UpdateNavigation();
            m_profilesLayout.UpdateNavigation();
        }

        private void CreateNewProfile()
        {
            void OnCreate(Profile profile)
            {
                if (profile != null)
                {
                    Refresh();
                    PrimarySelection.Current = m_profilePanels[m_profilePanels.Count - 1].gameObject;
                }
            }

            Menu.Get<ProfileNameMenu>().CreateNew(OnCreate, this);
        }

        private void SelectProfile(Profile profile)
        {
            Menu.Get<ProfileEditMenu>().Edit(profile, this);
        }
    }
}
