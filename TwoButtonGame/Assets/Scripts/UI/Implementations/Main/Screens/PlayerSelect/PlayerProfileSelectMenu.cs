using System;
using System.Collections.Generic;

using BoostBlasters.Profiles;

using Framework.UI;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI.MainMenu
{
    public class PlayerProfileSelectMenu : MenuScreen
    {
        [Header("Prefabs")]

        [SerializeField] private Button m_profilePanelPrefab = null;

        [Header("UI Elements")]

        [SerializeField] private Button m_createProfile = null;
        [SerializeField] private Button m_guestProfile = null;
        [SerializeField] private VerticalNavigationBuilder m_buttonsLayout = null;
        [SerializeField] private VerticalNavigationBuilder m_profilesLayout = null;


        private readonly List<Button> m_profilePanels = new List<Button>();
        private PlayerSelectPanel m_panel;

        /// <summary>
        /// An event invoked once the user has selected a profile.
        /// </summary>
        public event Action<Profile> ProfileSelected;


        protected override void OnInitialize()
        {
            m_panel = GetComponentInParent<PlayerSelectPanel>();

            m_guestProfile.onClick.AddListener(() => UseGuestProfile());
            m_createProfile.onClick.AddListener(() => CreateNewProfile());

            Menu.Shown += (m) =>
            {
                if (m == m_panel.JoinMenu)
                {
                    PrimarySelection.SelectDefault();
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

        public override void Back()
        {
            m_panel.Leave();
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

        private void UseGuestProfile()
        {
            var profile = ProfileManager.CreateTemporaryProfile("Guest", true);
            SelectProfile(profile);
        }

        private void CreateNewProfile()
        {
            var input = Input;

            void OnCreate(Profile profile)
            {
                Menu.Open(this, input, TransitionSound.None);

                if (profile != null)
                {
                    SelectProfile(profile);
                }
            }

            Menu.Get<ProfileNameMenu>().CreateNew(OnCreate, input);
        }

        private void SelectProfile(Profile profile)
        {
            ProfileSelected?.Invoke(profile);
        }
    }
}
