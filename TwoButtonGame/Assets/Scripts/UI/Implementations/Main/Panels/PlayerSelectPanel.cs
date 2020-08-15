using System;

using BoostBlasters.Input;
using BoostBlasters.Races;

using UnityEngine;

namespace BoostBlasters.UI.MainMenu
{
    [Serializable]
    public class PlayerSelectPanel : MonoBehaviour
    {
        [SerializeField] private PlayerJoinMenu m_joinMenu = null;
        [SerializeField] private PlayerProfileSelectMenu m_profileMenu = null;
        [SerializeField] private PlayerCharacterSelectMenu m_characterMenu = null;


        private MenuBase m_menu;

        public PlayerJoinMenu JoinMenu => m_joinMenu;
        public PlayerProfileSelectMenu ProfileMenu => m_profileMenu;
        public PlayerCharacterSelectMenu CharacterMenu => m_characterMenu;

        /// <summary>
        /// The user bound to this panel.
        /// </summary>
        public UserInput User { get; private set; }


        private void Awake()
        {
            m_menu = GetComponentInParent<MenuBase>();
        }

        /// <summary>
        /// Opens the panel.
        /// </summary>
        /// <param name="raceParams">The configuration to initialize from, or null to reset.</param>
        public void Open(RaceParameters raceParams)
        {
            if (raceParams == null)
            {
                User = null;
                m_menu.Open(m_joinMenu, null, TransitionSound.None);
            }
            else
            {
                //User = 
                //m_menu.Open(m_characterMenu, User, TransitionSound.None);
            }
        }

        /// <summary>
        /// Assigns a user to the panel.
        /// </summary>
        /// <param name="user">The user to associate with the panel.</param>
        public void AssignUser(UserInput user)
        {
            if (user == null)
            {
                Debug.LogError($"Cannot assign a null user to panel {name}!");
                return;
            }
            if (User != null)
            {
                Debug.LogError($"A user is already assigned to panel {name}!");
                return;
            }

            User = user;

            m_menu.Close(m_joinMenu, TransitionSound.Next);
            m_menu.Open(m_profileMenu, User, TransitionSound.Next);
        }

        /// <summary>
        /// Clear the user assigned to this panel.
        /// </summary>
        public void Leave()
        {
            if (User != null)
            {
                User.Leave();
                User = null;
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
            m_menu.Close(m_joinMenu, TransitionSound.Back);
            m_menu.Close(m_profileMenu, TransitionSound.Back);
            m_menu.Close(m_characterMenu, TransitionSound.Back);
        }
    }
}
