using System.Collections.Generic;

using UnityEngine;

namespace BoostBlasters.UI
{
    /// <summary>
    /// Manages a mutually exclusive set of menu screens.
    /// </summary>
    [RequireComponent(typeof(SoundPlayer))]
    public abstract class MenuBase<T> : MonoBehaviour where T : MenuBase<T>
    {
        private readonly List<MenuScreen> m_menuScreens = new List<MenuScreen>();
        private MenuScreen m_targetMenu = null;
        private TransitionSound m_targetSound = TransitionSound.None;

        /// <summary>
        /// The sound player for the menu.
        /// </summary>
        public SoundPlayer Sound { get; private set; } = null;

        /// <summary>
        /// The menu screen that is currently open.
        /// </summary>
        protected MenuScreen ActiveScreen { get; private set; } = null;


        protected virtual void Awake()
        {
            Sound = GetComponent<SoundPlayer>();
            GetComponentsInChildren(m_menuScreens);
        }

        protected virtual void Start()
        {
            foreach (MenuScreen menu in m_menuScreens)
            {
                menu.InitMenu();
                menu.enabled = false;
            }
        }

        protected virtual void Update()
        {
            DoScreenTransition();

            Sound.FlushSoundQueue();

            foreach (MenuScreen menu in m_menuScreens)
            {
                menu.UpdateMenu();
            }
        }

        protected virtual void LateUpdate()
        {
            foreach (MenuScreen menu in m_menuScreens)
            {
                menu.UpdateGraphics();
            }
        }

        /// <summary>
        /// Changes to the target menu.
        /// </summary>
        /// <param name="menu">The menu to enable.</param>
        /// <param name="sound">The sound to play for the menu transition.</param>
        public void SetMenu(MenuScreen menu, TransitionSound sound = TransitionSound.Next)
        {
            m_targetMenu = menu;
            m_targetSound = sound;
        }

        /// <summary>
        /// Determines if a menu screen should get initialized back to default values
        /// when a menu screen transition is occuring.
        /// </summary>
        /// <param name="menu">The menu to reset.</param>
        /// <param name="from">The previous menu screen.</param>
        /// <param name="to">The menu screen being opened.</param>
        /// <returns>If true the menu is reset.</returns>
        protected virtual bool ShouldFullReset(MenuScreen menu, MenuScreen from, MenuScreen to)
        {
            return true;
        }

        private void DoScreenTransition()
        {
            // check that we are not already on the menu to switch to
            if (ActiveScreen != m_targetMenu)
            {
                MenuScreen previous = ActiveScreen;
                ActiveScreen = m_targetMenu;

                foreach (MenuScreen menu in m_menuScreens)
                {
                    menu.enabled = menu == ActiveScreen;
                    menu.ResetMenu(ShouldFullReset(menu, previous, ActiveScreen));
                }

                // play the transition sound
                switch (m_targetSound)
                {
                    case TransitionSound.Open: Sound.PlayOpenMenuSound(); break;
                    case TransitionSound.Next: Sound.PlayNextMenuSound(); break;
                    case TransitionSound.Back: Sound.PlayBackMenuSound(); break;
                }
            }
        }
    }
}
