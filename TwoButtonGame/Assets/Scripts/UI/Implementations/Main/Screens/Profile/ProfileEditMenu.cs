using System.Linq;

using BoostBlasters.Profiles;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI.MainMenus
{
    public class ProfileEditMenu : MenuScreen
    {
        [Header("UI Elements")]

        [SerializeField] private Button m_rename = null;
        [SerializeField] private Button m_delete = null;
        [SerializeField] private TextMeshProUGUI m_profileName = null;
        [SerializeField] private TextMeshProUGUI m_racesCompleted = null;


        private Profile m_profile = null;
        private MenuScreen m_returnToMenu = null;


        protected override void OnInitialize()
        {
            m_rename.onClick.AddListener(() => Rename());
            m_delete.onClick.AddListener(() => Delete());
        }

        /// <summary>
        /// Opens a profile for editing.
        /// </summary>
        /// <param name="returnToMenu">The menu to go back to when finished.</param>
        public void Edit(Profile profile, MenuScreen returnToMenu)
        {
            m_profile = profile;
            m_returnToMenu = returnToMenu;

            Menu.Open(this, TransitionSound.Next);
        }

        protected override void OnShow()
        {
            Refresh();
        }

        public override void Back()
        {
            Menu.Close(this, TransitionSound.Back);
            Menu.Open(m_returnToMenu, TransitionSound.Back);

            PrimarySelection.SelectDefault();
            m_profile = null;
        }

        private void Refresh()
        {
            m_profileName.text = m_profile.Name;
            m_racesCompleted.text = m_profile.RaceResults.Count(r => r.Finished).ToString();
        }

        private void Rename()
        {
            void OnRename(Profile profile)
            {
                Refresh();
            }

            Menu.Get<ProfileNameMenu>().Rename(m_profile, OnRename, this);
        }

        private void Delete()
        {
            void OnDelete(bool accept)
            {
                if (accept)
                {
                    ProfileManager.DeleteProfile(m_profile);
                    Back();
                }
            }

            var message = $"Are you sure you want to delete profile \"{m_profile.Name}\"?";
            Menu.Get<ConfirmMenu>().ConfirmAction(message, OnDelete, this);
        }
    }
}
