using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace BoostBlasters.UI
{
    /// <summary>
    /// A menu UI component that manages the <see cref="MenuScreen"/> instances that
    /// constitute a menu.
    /// </summary>
    [RequireComponent(typeof(SoundPlayer))]
    public abstract class MenuBase : MonoBehaviour
    {
        [Header("Input")]

        [SerializeField]
        [Tooltip("The selection used for primary navigation and submit/cancel events.")]
        private InputSystemUIInputModule m_primaryInputPrefab;

        [SerializeField]
        [Tooltip("The input modeule used for secondary navigation events (ex. L/R bumpers).")]
        private InputSystemUIInputModule m_secondaryInputPrefab;


        private struct Transition
        {
            public enum Type
            {
                Open,
                Close,
            }

            public Type type;
            public MenuScreen screen;
            public TransitionSound sound;
        }

        private readonly List<MenuInput> m_inputs = new List<MenuInput>();
        private readonly List<MenuScreen> m_menuScreens = new List<MenuScreen>();
        private readonly Dictionary<Type, List<MenuScreen>> m_typeToScreens = new Dictionary<Type, List<MenuScreen>>();
        private readonly Dictionary<MenuScreen, Transition> m_transitions = new Dictionary<MenuScreen, Transition>();

        /// <summary>
        /// The sound player for the menu.
        /// </summary>
        public SoundPlayer Sound { get; private set; }

        /// <summary>
        /// An event triggered when a menu screen has become visible.
        /// </summary>
        public event Action<MenuScreen> Shown;

        /// <summary>
        /// An event triggered when a menu screen has become hidden.
        /// </summary>
        public event Action<MenuScreen> Hidden;


        protected virtual void Awake()
        {
            Sound = GetComponent<SoundPlayer>();

            // initialize the menu input
            var primary = new Input(Instantiate(m_primaryInputPrefab, transform));
            var secondary = new Input(Instantiate(m_secondaryInputPrefab, transform));
            m_inputs.Add(new MenuInput(primary, secondary));

            // get all menu screens in the menu
            GetComponentsInChildren(true, m_menuScreens);

            foreach (var menu in m_menuScreens)
            {
                var type = menu.GetType();

                if (!m_typeToScreens.TryGetValue(type, out var menus))
                {
                    menus = new List<MenuScreen>();
                    m_typeToScreens.Add(type, menus);
                }

                menus.Add(menu);
            }
        }

        protected virtual void Start()
        {
            foreach (var menu in m_menuScreens)
            {
                menu.Initialize();
            }
        }

        protected virtual void Update()
        {
            FlushTransitions();
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
                menu.UpdateVisuals();
            }
        }

        /// <summary>
        /// Gets the first <see cref="MenuScreen"/> instance in this menu.
        /// </summary>
        /// <typeparam name="TScreen">The type of the menu screen to get.</typeparam>
        /// <returns>The menu screen, or null if there is no screen of the given
        /// type in the menu.</returns>
        public TScreen Get<TScreen>() where TScreen : MenuScreen
        {
            return m_typeToScreens.TryGetValue(typeof(TScreen), out var screens) ? screens[0] as TScreen : null;
        }
       
        /// <summary>
        /// Gets all the <see cref="MenuScreen"/> instances in this menu.
        /// </summary>
        /// <typeparam name="TScreen">The type of the menu screens to get.</typeparam>
        /// <param name="results">The list used to return the results.</param>
        public void GetAll<TScreen>(List<TScreen> results) where TScreen : MenuScreen
        {
            results.Clear();

            if (m_typeToScreens.TryGetValue(typeof(TScreen), out var screens))
            {
                foreach (var screen in screens)
                {
                    results.Add(screen as TScreen);
                }
            }
        }

        /// <summary>
        /// Show a menu screen.
        /// </summary>
        /// <typeparam name="TScreen">The type of the screen to show.</typeparam>
        /// <param name="sound">The sound to play for the transition.</param>
        public void Open<TScreen>(TransitionSound sound) where TScreen : MenuScreen
        {
            Open(Get<TScreen>(), sound);
        }

        /// <summary>
        /// Show a menu screen.
        /// </summary>
        /// <param name="screen">The screen to show.</param>
        /// <param name="sound">The sound to play for the transition.</param>
        public void Open(MenuScreen screen, TransitionSound sound)
        {
            if (screen != null)
            {
                foreach (var menu in m_menuScreens)
                {
                    menu.SetInput(null);
                }

                m_transitions[screen] = new Transition
                {
                    type = Transition.Type.Open,
                    screen = screen,
                    sound = sound,
                };
            }
        }

        /// <summary>
        /// Hides a menu screen.
        /// </summary>
        /// <typeparam name="TScreen">The type of the screen to close.</typeparam>
        /// <param name="sound">The sound to play for the transition.</param>
        public void Close<TScreen>(TransitionSound sound) where TScreen : MenuScreen
        {
            Close(Get<TScreen>(), sound);
        }

        /// <summary>
        /// Hides a menu screen.
        /// </summary>
        /// <param name="screen">The screen to close.</param>
        /// <param name="sound">The sound to play for the transition.</param>
        public void Close(MenuScreen screen, TransitionSound sound)
        {
            if (screen != null)
            {
                m_transitions[screen] = new Transition
                {
                    type = Transition.Type.Close,
                    screen = screen,
                    sound = sound,
                };
            }
        }

        /// <summary>
        /// Opens a menu, closing all other menu screens.
        /// </summary>
        /// <typeparam name="TScreen">The type of the screen to show.</typeparam>
        /// <param name="sound">The sound to play for the transition.</param>
        public void SwitchTo<TScreen>(TransitionSound sound) where TScreen : MenuScreen
        {
            SwitchTo(Get<TScreen>(), sound);
        }

        /// <summary>
        /// Opens a menu, closing all other menu screens.
        /// </summary>
        /// <param name="screen">The screen to show.</param>
        /// <param name="sound">The sound to play for the transition.</param>
        public void SwitchTo(MenuScreen screen, TransitionSound sound)
        {
            CloseAll(TransitionSound.None);
            Open(screen, sound);
        }

        /// <summary>
        /// Closes all menu screens.
        /// </summary>
        /// <param name="sound">The sound to play for the transition.</param>
        public void CloseAll(TransitionSound sound)
        {
            foreach (var menu in m_menuScreens)
            {
                Close(menu, sound);
            }
        }

        private void FlushTransitions()
        {
            // hide all menus before showing new ones
            foreach (var transition in m_transitions)
            {
                if (transition.Value.type == Transition.Type.Close)
                {
                    DoTransition(transition.Value);
                }
            }
            foreach (var transition in m_transitions)
            {
                if (transition.Value.type == Transition.Type.Open)
                {
                    DoTransition(transition.Value);
                }
            }

            m_transitions.Clear();
        }

        private void DoTransition(Transition transition)
        {
            var screen = transition.screen;

            switch (transition.type)
            {
                case Transition.Type.Open:
                    screen.Show(m_inputs[0]);
                    Shown?.Invoke(screen);
                    break;
                case Transition.Type.Close:
                    transition.screen.Hide();
                    Hidden?.Invoke(screen);
                    break;
            }

            switch (transition.sound)
            {
                case TransitionSound.Open: Sound.PlayOpenMenuSound(); break;
                case TransitionSound.Next: Sound.PlayNextMenuSound(); break;
                case TransitionSound.Back: Sound.PlayBackMenuSound(); break;
            }
        }
    }
}
