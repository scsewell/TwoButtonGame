using BoostBlasters.Input;
using BoostBlasters.Profiles;
using BoostBlasters.Races;

using UnityEngine;

namespace BoostBlasters.UI.MainMenu
{
    public class PlayerSelectPanel : MonoBehaviour
    {
        [SerializeField] private PlayerJoinMenu m_joinMenu = null;
        [SerializeField] private PlayerProfileSelectMenu m_profileMenu = null;
        [SerializeField] private PlayerCharacterSelectMenu m_characterMenu = null;


        private MenuBase m_menu;
        private UserInput m_user;
        private Profile m_profile;

        public PlayerJoinMenu JoinMenu => m_joinMenu;
        public PlayerProfileSelectMenu ProfileMenu => m_profileMenu;
        public PlayerCharacterSelectMenu CharacterMenu => m_characterMenu;


        private void Awake()
        {
            m_menu = GetComponentInParent<MenuBase>();

            m_profileMenu.ProfileSelected += OnProfileSelected;
        }

        /// <summary>
        /// Displays the panel.
        /// </summary>
        /// <param name="raceParams">The configuration to initialize from, or null to reset.</param>
        public void Open(RaceParameters raceParams)
        {
            if (raceParams == null)
            {
                m_user = null;
                m_profile = null;
                m_menu.Open(m_joinMenu, null, TransitionSound.None);
            }
            else
            {
                //m_user = 
                //m_profile =
                //m_menu.Open(m_characterMenu, m_user, TransitionSound.None);
            }
        }

        /// <summary>
        /// Assigns a user to the panel.
        /// </summary>
        /// <param name="user">The user to associate with the panel. Cannot be null.</param>
        public void AssignUser(UserInput user)
        {
            if (user == null)
            {
                Debug.LogError($"Cannot assign a null user to panel {name}!");
                return;
            }
            if (m_user != null)
            {
                Debug.LogError($"A user is already assigned to panel {name}!");
                return;
            }

            m_user = user;

            m_menu.Close(m_joinMenu, TransitionSound.Next);
            m_menu.Open(m_profileMenu, m_user, TransitionSound.Next);
        }

        /// <summary>
        /// Clear the user assigned to this panel.
        /// </summary>
        public void Leave()
        {
            if (m_user != null)
            {
                m_user.Leave();
                m_user = null;
            }

            m_menu.Open(m_joinMenu, null, TransitionSound.Back);
            m_menu.Close(m_profileMenu, TransitionSound.Back);
            m_menu.Close(m_characterMenu, TransitionSound.Back);
        }

        /// <summary>
        /// Close the menus under this panel.
        /// </summary>
        public void Close()
        {
            if (m_user != null)
            {
                m_user.Leave();
                m_user = null;
            }

            m_menu.Close(m_joinMenu, TransitionSound.Back);
            m_menu.Close(m_profileMenu, TransitionSound.Back);
            m_menu.Close(m_characterMenu, TransitionSound.Back);
        }

        private void OnProfileSelected(Profile profile)
        {
            m_profile = profile;

            m_menu.Close(m_profileMenu, TransitionSound.Next);
            m_menu.Open(m_characterMenu, m_user, TransitionSound.Next);
        }
    }
}
