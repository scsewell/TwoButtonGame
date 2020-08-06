using System;
using System.Collections.Generic;

using UnityEngine;

namespace BoostBlasters.UI
{
    /// <summary>
    /// A menu UI component that manages the <see cref="MenuScreen"/> instances that
    /// constitute a menu.
    /// </summary>
    [RequireComponent(typeof(SoundPlayer))]
    public abstract class MenuBase : MonoBehaviour
    {
        private readonly List<MenuScreen> m_menuScreens = new List<MenuScreen>();
        private readonly Dictionary<Type, MenuScreen> m_typeToMenu = new Dictionary<Type, MenuScreen>();

        private MenuScreen m_targetMenu = null;
        private TransitionSound m_targetSound;

        /// <summary>
        /// The sound player for the menu.
        /// </summary>
        public SoundPlayer Sound { get; private set; }

        /// <summary>
        /// The menu screen that is currently open.
        /// </summary>
        protected MenuScreen ActiveScreen { get; private set; } = null;


        protected virtual void Awake()
        {
            Sound = GetComponent<SoundPlayer>();
            GetComponentsInChildren(true, m_menuScreens);

            foreach (var menu in m_menuScreens)
            {
                m_typeToMenu.Add(menu.GetType(), menu);
            }
        }

        protected virtual void Start()
        {
            foreach (var menu in m_menuScreens)
            {
                menu.InitMenu();
                menu.enabled = false;
            }
        }

        protected virtual void Update()
        {
            DoScreenTransition();

            Sound.FlushSoundQueue();

            foreach (var menu in m_menuScreens)
            {
                menu.UpdateMenu();
            }
        }

        protected virtual void LateUpdate()
        {
            foreach (var menu in m_menuScreens)
            {
                menu.UpdateGraphics();
            }
        }

        /// <summary>
        /// Gets a <see cref="MenuScreen"/> from this menu.
        /// </summary>
        /// <typeparam name="TScreen">The type of the menu screen to get.</typeparam>
        /// <returns>The menu screen, or null if there is no screen of the given
        /// type in the menu.</returns>
        public TScreen Get<TScreen>() where TScreen : MenuScreen
        {
            return m_typeToMenu.TryGetValue(typeof(TScreen), out var screen) ? screen as TScreen : null;
        }

        /// <summary>
        /// Changes the active menu screen to a given screen.
        /// </summary>
        /// <typeparam name="TScreen">The type of the screen to activate.</typeparam>
        /// <param name="sound">The sound to play for the menu transition.</param>
        public void SwitchTo<TScreen>(TransitionSound sound = TransitionSound.Next) where TScreen : MenuScreen
        {
            SwitchTo(Get<TScreen>(), sound);
        }

        /// <summary>
        /// Changes the active menu screen to a given screen.
        /// </summary>
        /// <param name="screen">The screen to activate. If null the active screen is closed.</param>
        /// <param name="sound">The sound to play for the menu transition.</param>
        public void SwitchTo(MenuScreen screen, TransitionSound sound = TransitionSound.Next)
        {
            m_targetMenu = screen;
            m_targetSound = sound;
        }

        /// <summary>
        /// Closes all menu screens.
        /// </summary>
        /// <param name="sound">The sound to play for the menu transition.</param>
        public void Close(TransitionSound sound = TransitionSound.Next)
        {
            SwitchTo(null, sound);
        }

        /// <summary>
        /// Determines if all menu screens should be reset back to their default state
        /// when a menu screen transition is occuring.
        /// </summary>
        /// <param name="from">The previous menu screen.</param>
        /// <param name="to">The menu screen being opened.</param>
        /// <returns>If true all menu is reset upon transition.</returns>
        protected virtual bool ShouldForceReset(MenuScreen from, MenuScreen to)
        {
            return false;
        }

        private void DoScreenTransition()
        {
            if (ActiveScreen != m_targetMenu)
            {
                if (ActiveScreen != null)
                {
                    ActiveScreen.enabled = false;
                }

                var previous = ActiveScreen;
                ActiveScreen = m_targetMenu;

                if (ActiveScreen != null)
                {
                    ActiveScreen.enabled = true;
                }

                var forceReset = ShouldForceReset(previous, ActiveScreen);
                foreach (var menu in m_menuScreens)
                {
                    menu.OnTransition(forceReset, previous, ActiveScreen);
                }

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
