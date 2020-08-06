using UnityEngine;

namespace BoostBlasters.UI
{
    /// <summary>
    /// A component that manages a menu screen the user can interact with.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public abstract class MenuScreen : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The primary input/selection configuration.")]
        private Selection m_primarySelection;

        [SerializeField]
        [Tooltip("The secondary input/selection configuration.")]
        private Selection m_secondarySelection;

        [SerializeField]
        [Tooltip("Remeber the last selected element on this screen and select it when going back into this menu screen.")]
        private bool m_remeberLastSelection = false;

        [SerializeField]
        [Tooltip("If enabled, pressing cancel closes this menu screen.")]
        private bool m_closeOnBack = false;

        [SerializeField]
        [Tooltip("The menu screen to go to when backing out of this menu screen.")]
        private MenuScreen m_backMenu = null;

        private Canvas m_canvas = null;

        /// <summary>
        /// The menu that owns this menu screen.
        /// </summary>
        public MenuBase Menu { get; private set; }

        /// <summary>
        /// Remeber the last selected elements on this screen and select them when going
        /// back into this menu screen.
        /// </summary>
        public bool RemeberLastSelection
        {
            get => m_remeberLastSelection;
            set => m_remeberLastSelection = value;
        }

        /// <summary>
        /// The selection used for primary navigation and submit/cancellation events.
        /// </summary>
        protected Selection PrimarySelection => m_primarySelection;

        /// <summary>
        /// The selection used for secondary navigation events (ex. L/R bumpers).
        /// </summary>
        /// <remarks>
        /// This is generally intended for things like tabbed sub-menus, so the tabs can have
        /// a separate selection from the selectables in the sub-menus themselves.
        /// </remarks>
        protected Selection SecondarySelection => m_secondarySelection;


        protected virtual void Awake()
        {
            m_canvas = GetComponent<Canvas>();
            m_canvas.enabled = false;

            Menu = GetComponentInParent<MenuBase>();

            m_primarySelection.Initialize(this);
            m_secondarySelection.Initialize(this);
        }

        private void OnEnable()
        {
            m_canvas.enabled = true;

            m_primarySelection.OnEnable();
            m_secondarySelection.OnEnable();

            OnEnableMenu();
        }

        private void OnDisable()
        {
            m_canvas.enabled = false;

            m_primarySelection.OnDisable(m_remeberLastSelection);
            m_secondarySelection.OnDisable(m_remeberLastSelection);

            OnDisableMenu();
        }

        /// <summary>
        /// Updates the menu logic.
        /// </summary>
        public void UpdateMenu()
        {
            if (enabled)
            {
                OnUpdate();

                m_primarySelection.AquireSelectionIfNeeded();
                m_secondarySelection.AquireSelectionIfNeeded();
            }
        }

        /// <summary>
        /// Updates the menu visuals.
        /// </summary>
        public void UpdateGraphics()
        {
            if (enabled)
            {
                OnUpdateGraphics();
            }
        }

        /// <summary>
        /// Backs out of this menu.
        /// </summary>
        public virtual void Back()
        {
            if (enabled && m_closeOnBack && m_backMenu)
            {
                Menu.SwitchTo(m_backMenu, TransitionSound.Back);
            }
        }

        /// <summary>
        /// Called once to initilize the menu.
        /// </summary>
        public abstract void InitMenu();

        /// <summary>
        /// Prepares the menu screen prior to being made visible.
        /// </summary>
        /// <param name="fullReset">Should the screen clear all state.</param>
        /// <param name="from">The previous menu screen.</param>
        /// <param name="to">The menu screen being opened.</param>
        public virtual void OnTransition(bool fullReset, MenuScreen from, MenuScreen to)
        {
        }

        protected virtual void OnEnableMenu() { }
        protected virtual void OnDisableMenu() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnUpdateGraphics() { }
    }
}
