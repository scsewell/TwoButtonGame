using BoostBlasters.Profiles;

using Framework.UI;

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


        private ItemList<Profile> m_profiles;


        protected override void OnInitialize()
        {
            m_profiles = new ItemList<Profile>(m_profilesLayout, m_profilePanelPrefab, (p) => p.Name);
            m_profiles.PreLayoutUpdate += () => m_buttonsLayout.UpdateNavigation();
            m_profiles.Submit += SelectProfile;

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
            Profile.Added += OnProfileChamged;
            Profile.Renamed += OnProfileChamged;
            Profile.Deleted += OnProfileChamged;

            Refresh();
        }

        protected override void OnHide()
        {
            Profile.Added += OnProfileChamged;
            Profile.Renamed -= OnProfileChamged;
            Profile.Deleted -= OnProfileChamged;
        }

        private void OnProfileChamged(Profile profile)
        {
            Refresh();
        }

        private void Refresh()
        {
            m_profiles.Refresh(Profile.AllProfiles);
        }

        private void CreateNewProfile()
        {
            void OnCreate(Profile profile)
            {
                Menu.Open(this, TransitionSound.None);

                if (profile != null)
                {
                    Refresh();

                    if (m_profiles.TryGetItem(profile, out var item))
                    {
                        PrimarySelection.Current = item.gameObject;
                    }
                }
            }

            Menu.Get<ProfileNameMenu>().CreateNew(OnCreate);
        }

        private void SelectProfile(Profile profile)
        {
            Menu.Get<ProfileEditMenu>().Edit(profile, this);
        }
    }
}
