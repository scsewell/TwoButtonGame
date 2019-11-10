using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

namespace BoostBlasters.UI
{
    /// <summary>
    /// Manages a mutually exclusive set of menu screens.
    /// </summary>
    [RequireComponent(typeof(SoundPlayer))]
    public abstract class MenuBase<T> : MonoBehaviour where T : MenuBase<T>
    {
        private List<MenuScreen> m_menuScreens = null;
        private MenuScreen m_targetMenu = null;

        /// <summary>
        /// The sound player for the menu.
        /// </summary>
        public SoundPlayer Sound { get; private set; } = null;

        /// <summary>
        /// The menu screen that is currently being used.
        /// </summary>
        protected MenuScreen ActiveScreen { get; private set; } = null;

        public List<PlayerBaseInput> Inputs { get; private set; } = null;


        protected void InitBase(List<PlayerBaseInput> inputs)
        {
            Sound = GetComponent<SoundPlayer>();

            Inputs = inputs;

            CustomInputModule inputModule = gameObject.AddComponent<CustomInputModule>();
            CustomInput customInput = gameObject.AddComponent<CustomInput>();
            customInput.SetInputs(inputs);
            inputModule.SetInputOverride(customInput);

            m_menuScreens = new List<MenuScreen>();
            GetComponentsInChildren(m_menuScreens);

            foreach (MenuScreen<T> menu in m_menuScreens)
            {
                menu.InitMenu();
                menu.enabled = false;
            }
        }

        public void SetMenu(MenuScreen menu, TransitionSound sound = TransitionSound.Next)
        {
            if (m_targetMenu != menu)
            {
                m_targetMenu = menu;

                switch (sound)
                {
                    case TransitionSound.Open: Sound.PlayOpenMenuSound(); break;
                    case TransitionSound.Next: Sound.PlayNextMenuSound(); break;
                    case TransitionSound.Back: Sound.PlayBackMenuSound(); break;
                }
            }
        }

        /// <summary>
        /// Updates the menu.
        /// </summary>
        protected void UpdateBase()
        {
            foreach (MenuScreen<T> menu in m_menuScreens)
            {
                menu.UpdateMenu();
            }
        }

        /// <summary>
        /// Updates the menu at the end of the frame.
        /// </summary>
        /// <param name="fullReset">A function which decides if menus should be fully reset.</param>
        protected void LateUpdateBase(Func<MenuScreen, bool> fullReset)
        {
            // check that we are not already on the menu to switch to
            if (ActiveScreen != m_targetMenu)
            {
                MenuScreen previous = ActiveScreen;
                ActiveScreen = m_targetMenu;

                // disable all other menus screens
                foreach (MenuScreen<T> menu in m_menuScreens)
                {
                    if (menu != ActiveScreen as MenuScreen<T>)
                    {
                        menu.enabled = false;
                        menu.ResetMenu(fullReset(previous));
                    }
                }

                // clear the current selection
                EventSystem.current.SetSelectedGameObject(null);

                // enable the new menu screen
                if (ActiveScreen != null && ActiveScreen is MenuScreen<T> activeScreen)
                {
                    activeScreen.enabled = true;
                    activeScreen.ResetMenu(fullReset(previous));
                }
            }

            // update the menu graphics
            foreach (MenuScreen<T> menu in m_menuScreens)
            {
                menu.UpdateGraphics();
            }

            // play any sounds
            Sound.FlushSoundQueue();
        }
    }
}
