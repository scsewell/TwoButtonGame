using System;

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
        private ItemList<Profile> m_profiles;

        /// <summary>
        /// An event invoked once the user has selected a profile.
        /// </summary>
        public event Action<Profile> ProfileSelected;


        protected override void OnInitialize()
        {
            m_panel = GetComponentInParent<PlayerSelectPanel>();

            m_profiles = new ItemList<Profile>(m_profilesLayout, m_profilePanelPrefab, (p) => p.Name);
            m_profiles.PreLayoutUpdate += () => m_buttonsLayout.UpdateNavigation();
            m_profiles.Submit += SelectProfile;

            m_guestProfile.onClick.AddListener(() => UseGuestProfile());
            m_createProfile.onClick.AddListener(() => CreateNewProfile());
        }

        /// <summary>
        /// Sets the menu state.
        /// </summary>
        /// <param name="profile">The profile to select, or null to reset the selection.</param>
        public void Set(Profile profile)
        {
            Refresh();

            if (profile != null && m_profiles.TryGetItem(profile, out var item))
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
            ProfileManager.Added += OnProfileChamged;
            ProfileManager.Renamed += OnProfileChamged;
            ProfileManager.Deleted += OnProfileChamged;

            Refresh();
        }

        protected override void OnHide()
        {
            ProfileManager.Added -= OnProfileChamged;
            ProfileManager.Renamed -= OnProfileChamged;
            ProfileManager.Deleted -= OnProfileChamged;
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
            m_profiles.Refresh(ProfileManager.Profiles);
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
