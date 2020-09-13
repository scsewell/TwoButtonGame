using System;

using BoostBlasters.Characters;
using BoostBlasters.Input;
using BoostBlasters.Profiles;
using BoostBlasters.Races;

using UnityEngine;

namespace BoostBlasters.UI.MainMenu
{
    /// <summary>
    /// A component that manages the menus used a single user to prepare for a race.
    /// </summary>
    public class PlayerSelectPanel : MonoBehaviour
    {
        [SerializeField] private PlayerJoinMenu m_joinMenu = null;
        [SerializeField] private PlayerProfileSelectMenu m_profileMenu = null;
        [SerializeField] private PlayerCharacterSelectMenu m_characterMenu = null;


        private MenuBase m_menu;

        /// <summary>
        /// The user assigned to this panel.
        /// </summary>
        public UserInput User { get; private set; }

        /// <summary>
        /// The profile selected by the user.
        /// </summary>
        public IProfile Profile { get; private set; }

        /// <summary>
        /// The character selected by the user.
        /// </summary>
        public Character Character { get; private set; }

        /// <summary>
        /// Has this user finished character selection.
        /// </summary>
        public bool Ready { get; private set; }

        /// <summary>
        /// Is the user ready, or is there no user assigned.
        /// </summary>
        public bool CanContinue => Ready || User == null;

        /// <summary>
        /// An event invoked when <see cref="Profile"/> has changed.
        /// </summary>
        public event Action ProfileChanged;

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
            m_characterMenu.Continue += OnContinue;
        }
        
        /// <summary>
        /// Displays the panel.
        /// </summary>
        /// <param name="racerConfig">The configuration to initialize from, or null to reset.</param>
        public void Open(PlayerRacerConfig racerConfig)
        {
            ResetPanel(true);

            if (racerConfig == null)
            {
                m_menu.Open(m_joinMenu, null, TransitionSound.None);
            }
            else
            {
                User = racerConfig.Input;
                Profile = racerConfig.Profile;
                Character = racerConfig.Character;
                Ready = true;

                m_characterMenu.Set(Profile, Character, Ready);

                m_menu.Open(m_characterMenu, User, TransitionSound.None);

                ProfileChanged?.Invoke();
                CanContinueChanged?.Invoke();
            }
        }

        /// <summary>
        /// Gets the racer config for this player.
        /// </summary>
        /// <returns>A new config instance, or null if the player is not ready.</returns>
        public PlayerRacerConfig GetConfig()
        {
            return Ready ? new PlayerRacerConfig(Character, Profile, User) : null;
        }

        /// <summary>
        /// Close the menus under this panel.
        /// </summary>
        /// <param name="sound">The menu transition sound to play.</param>
        public void Close(TransitionSound sound)
        {
            ResetPanel(false);

            m_menu.Close(m_joinMenu, sound);
            m_menu.Close(m_profileMenu, sound);
            m_menu.Close(m_characterMenu, sound);
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
            m_profileMenu.Set(Profile);

            m_menu.Close(m_joinMenu, TransitionSound.Next);
            m_menu.Open(m_profileMenu, User, TransitionSound.Next);

            CanContinueChanged?.Invoke();
        }

        /// <summary>
        /// Backs out to the join menu.
        /// </summary>
        public void Leave()
        {
            if (User != null)
            {
                User.Leave();
            }

            ResetPanel(true);

            m_menu.Open(m_joinMenu, null, TransitionSound.Back);
            m_menu.Close(m_profileMenu, TransitionSound.Back);
            m_menu.Close(m_characterMenu, TransitionSound.Back);
        }

        /// <summary>
        /// Backs out to the character select menu.
        /// </summary>
        public void BackToProfile()
        {
            Ready = false;

            m_profileMenu.Set(Profile);

            ResetProfile(true);

            m_menu.Open(m_profileMenu, User, TransitionSound.Back);
            m_menu.Close(m_characterMenu, TransitionSound.Back);

            CanContinueChanged?.Invoke();
        }

        private void OnProfileSelected(IProfile profile)
        {
            Profile = profile;

            m_characterMenu.Set(Profile, Character, Ready);

            m_menu.Close(m_profileMenu, TransitionSound.Next);
            m_menu.Open(m_characterMenu, User, TransitionSound.Next);

            ProfileChanged?.Invoke();
        }

        private void OnCharacterSelected(Character character)
        {
            Character = character;
        }

        private void OnReadyChanged(bool ready)
        {
            Ready = ready;
            CanContinueChanged?.Invoke();
        }

        private void OnContinue()
        {
            Continue?.Invoke();
        }

        private void ResetPanel(bool releaseProfile)
        {
            User = null;
            ResetProfile(releaseProfile);
            Character = null;
            Ready = false;

            CanContinueChanged?.Invoke();
        }

        private void ResetProfile(bool releaseProfile)
        {
            if (Profile != null)
            {
                if (releaseProfile && Profile is GuestProfile guestProfile)
                {
                    guestProfile.Release();
                }
                Profile = null;
                ProfileChanged?.Invoke();
            }
        }
    }
}
