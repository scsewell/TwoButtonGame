using System;

using BoostBlasters.Characters;
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
        private Profile m_profile;
        private Character m_character;

        /// <summary>
        /// The user assigned to this panel.
        /// </summary>
        public UserInput User { get; private set; }

        /// <summary>
        /// Has this user finished character selection.
        /// </summary>
        public bool Ready { get; private set; }

        /// <summary>
        /// Is the user ready, or is there no user assigned.
        /// </summary>
        public bool CanContinue => Ready || User == null;

        /// <summary>
        /// An event invoked when <see cref="CanContinue"/> has changed.
        /// </summary>
        public event Action CanContinueChanged;

        /// <summary>
        /// An event invoked when the user want to continue to the next menu screen.
        /// </summary>
        public event Action Continue;


        private void Awake()
        {
            m_menu = GetComponentInParent<MenuBase>();

            m_profileMenu.ProfileSelected += OnProfileSelected;
            m_characterMenu.CharacterSelected += OnCharacterSelected;
            m_characterMenu.ReadyChanged += OnReadyChanged;
            m_characterMenu.Continue += Continue;
        }

        /// <summary>
        /// Displays the panel.
        /// </summary>
        /// <param name="racerConfig">The configuration to initialize from, or null to reset.</param>
        public void Open(PlayerRacerConfig racerConfig)
        {
            if (racerConfig == null)
            {
                ResetPanel();

                m_menu.Open(m_joinMenu, null, TransitionSound.None);
            }
            else
            {
                User = racerConfig.Input;
                m_profile = racerConfig.Profile;
                m_character = racerConfig.Character;
                Ready = true;

                m_characterMenu.Set(m_profile, m_character, Ready);
                m_menu.Open(m_characterMenu, User, TransitionSound.None);

                CanContinueChanged?.Invoke();
            }
        }

        /// <summary>
        /// Gets the racer config for this player.
        /// </summary>
        /// <returns>A new config instance, or null if the player is not ready.</returns>
        public PlayerRacerConfig GetConfig()
        {
            return Ready ? new PlayerRacerConfig(m_character, m_profile, UnityEngine.Random.ColorHSV(), User) : null;
        }

        /// <summary>
        /// Close the menus under this panel.
        /// </summary>
        public void Close()
        {
            ResetPanel();

            m_menu.Close(m_joinMenu, TransitionSound.Back);
            m_menu.Close(m_profileMenu, TransitionSound.Back);
            m_menu.Close(m_characterMenu, TransitionSound.Back);
        }

        /// <summary>
        /// Assigns a user to the panel.
        /// </summary>
        /// <param name="user">The user to associate with the panel. Cannot be null.</param>
        public void JoinUser(UserInput user)
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
            m_profileMenu.Set(m_profile);

            m_menu.Close(m_joinMenu, TransitionSound.Next);
            m_menu.Open(m_profileMenu, User, TransitionSound.Next);

            CanContinueChanged?.Invoke();
        }

        /// <summary>
        /// Backs out to the profile select menu.
        /// </summary>
        public void Leave()
        {
            ResetPanel();

            m_menu.Open(m_joinMenu, null, TransitionSound.Back);
            m_menu.Close(m_profileMenu, TransitionSound.Back);
            m_menu.Close(m_characterMenu, TransitionSound.Back);
        }

        /// <summary>
        /// Backs out to the character select menu.
        /// </summary>
        public void BackToProfile()
        {
            if (m_profile.IsTemporary)
            {
                ProfileManager.ReleaseTemporaryProfile(m_profile);
                m_profile = null;
            }
            Ready = false;

            m_profileMenu.Set(m_profile);

            m_menu.Open(m_profileMenu, User, TransitionSound.Back);
            m_menu.Close(m_characterMenu, TransitionSound.Back);

            CanContinueChanged?.Invoke();
        }

        private void OnProfileSelected(Profile profile)
        {
            m_profile = profile;

            m_characterMenu.Set(m_profile, m_character, Ready);

            m_menu.Close(m_profileMenu, TransitionSound.Next);
            m_menu.Open(m_characterMenu, User, TransitionSound.Next);
        }

        private void OnCharacterSelected(Character character)
        {
            m_character = character;
        }

        private void OnReadyChanged(bool ready)
        {
            Ready = ready;
            CanContinueChanged?.Invoke();
        }

        private void ResetPanel()
        {
            if (User != null)
            {
                User.Leave();
                User = null;
            }

            if (m_profile != null)
            {
                if (m_profile.IsTemporary)
                {
                    ProfileManager.ReleaseTemporaryProfile(m_profile);
                }
                m_profile = null;
            }

            Ready = false;
            m_character = null;

            CanContinueChanged?.Invoke();
        }
    }
}
