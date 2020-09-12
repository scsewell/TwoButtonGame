using System;
using System.Collections.Generic;
using System.Linq;

using BoostBlasters.Profiles;

using Framework.UI;

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


        private PlayerSelectPanel m_panel;
        private List<PlayerSelectPanel> m_otherPanels;
        private ItemList<IProfile> m_profiles;

        /// <summary>
        /// An event invoked once the user has selected a profile.
        /// </summary>
        public event Action<IProfile> ProfileSelected;


        protected override void OnInitialize()
        {
            m_panel = GetComponentInParent<PlayerSelectPanel>();

            m_otherPanels = m_panel.transform.parent.GetComponentsInChildren<PlayerSelectPanel>(true)
                .Where(p => p != m_panel)
                .ToList();

            foreach (var panel in m_otherPanels)
            {
                panel.ProfileChanged += Refresh;
            }

            m_profiles = new ItemList<IProfile>(m_profilesLayout, m_profilePanelPrefab, (p) => p.Name);
            m_profiles.PreLayoutUpdate += () => m_buttonsLayout.UpdateNavigation();
            m_profiles.Submit += SelectProfile;

            m_guestProfile.onClick.AddListener(() => UseGuestProfile());
            m_createProfile.onClick.AddListener(() => CreateNewProfile());
        }

        /// <summary>
        /// Sets the menu state.
        /// </summary>
        /// <param name="profile">The profile to select, or null to reset the selection.</param>
        public void Set(IProfile profile)
        {
            Refresh();

            if (m_profiles.TryGetItem(profile, out var item))
            {
                PrimarySelection.Current = item.gameObject;
            }
            else
            {
                PrimarySelection.SelectDefault();
            }
        }

        protected override void OnShow()
        {
            Profile.Added += OnProfileChamged;
            Profile.Renamed += OnProfileChamged;
            Profile.Deleted += OnProfileChamged;

            Refresh();
        }

        protected override void OnHide()
        {
            Profile.Added -= OnProfileChamged;
            Profile.Renamed -= OnProfileChamged;
            Profile.Deleted -= OnProfileChamged;
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
            // Only allow selecting profiles not used by another user
            m_profiles.Refresh(Profile.AllProfiles, (profile, item) =>
            {
                item.interactable = !m_otherPanels.Any(panel => panel.Profile == profile);
            });
        }

        private void UseGuestProfile()
        {
            var profile = new GuestProfile();
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

        private void SelectProfile(IProfile profile)
        {
            ProfileSelected?.Invoke(profile);
        }
    }
}
