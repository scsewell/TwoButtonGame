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
        [Tooltip("The primary selection configuration.")]
        private Selection m_primarySelection;

        [SerializeField]
        [Tooltip("The secondary selection configuration.")]
        private Selection m_secondarySelection;

        [SerializeField]
        [Tooltip("Enable selection and input for this menu screen automatically when it is shown.")]
        private bool m_enableInteractionOnShow = true;

        [SerializeField]
        [Tooltip("Remeber the last selected elements when disabling interaction on this screen and select them again when interaction is next enabled.")]
        private bool m_remeberLastSelection = false;

        [SerializeField]
        [Tooltip("If enabled, pressing cancel closes this menu screen.")]
        private bool m_closeOnBack = false;

        [SerializeField]
        [Tooltip("The menu screen to go to when backing out of this menu screen.")]
        private MenuScreen m_backMenu = null;

        private Canvas m_canvas = null;
        private bool m_interactable = false;

        /// <summary>
        /// The menu that owns this menu screen.
        /// </summary>
        public MenuBase Menu { get; private set; }

        /// <summary>
        /// The selection used for primary navigation and submit/cancel events.
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

        /// <summary>
        /// Remeber the last selected elements when disabling interaction on this screen and select them
        /// again when interaction is next enabled.
        /// </summary>
        public bool RemeberLastSelection
        {
            get => m_remeberLastSelection;
            set => m_remeberLastSelection = value;
        }

        /// <summary>
        /// Is this menu currently shown.
        /// </summary>
        public bool Visible { get; private set; } = false;

        /// <summary>
        /// Can this menu have a selection and receive input.
        /// </summary>
        public bool Interactable
        {
            get => m_interactable;
            set
            {
                if (m_interactable != value)
                {
                    m_interactable = value;

                    if (m_interactable)
                    {
                        m_primarySelection.OnEnable();
                        m_secondarySelection.OnEnable();
                    }
                    else
                    {
                        m_primarySelection.OnDisable(m_remeberLastSelection);
                        m_secondarySelection.OnDisable(m_remeberLastSelection);
                    }
                }
            }
        }

        protected virtual void Awake()
        {
            m_canvas = GetComponent<Canvas>();
            m_canvas.enabled = false;

            Menu = GetComponentInParent<MenuBase>();

            m_primarySelection.Initialize(this);
            m_secondarySelection.Initialize(this);
        }

        /// <summary>
        /// Called once to initilize the menu.
        /// </summary>
        public void Initialize()
        {
            OnInitialize();
            OnHide();
        }

        /// <summary>
        /// Makes the menu visible.
        /// </summary>
        public void Show()
        {
            if (!Visible)
            {
                Visible = true;
                m_canvas.enabled = true;

                Interactable = m_enableInteractionOnShow;

                OnShow();
            }
        }

        /// <summary>
        /// Makes the menu not visible.
        /// </summary>
        public void Hide()
        {
            if (Visible)
            {
                Visible = false;
                m_canvas.enabled = false;

                Interactable = false;

                OnHide();
            }
        }

        /// <summary>
        /// Updates the menu logic.
        /// </summary>
        public void UpdateMenu()
        {
            if (Visible)
            {
                OnUpdate();
            }
            if (m_interactable)
            {
                m_primarySelection.AquireSelectionIfNeeded();
                m_secondarySelection.AquireSelectionIfNeeded();
            }
        }

        /// <summary>
        /// Updates the menu visuals.
        /// </summary>
        public void UpdateVisuals()
        {
            if (Visible)
            {
                OnUpdateVisuals();
            }
        }

        /// <summary>
        /// Back out of this menu.
        /// </summary>
        public virtual void Back()
        {
            if (m_interactable && m_closeOnBack)
            {
                if (m_backMenu)
                {
                    Menu.SwitchTo(m_backMenu, TransitionSound.Back);
                }
                else
                {
                    Menu.Close(this, TransitionSound.Back);
                }
            }
        }

        /// <summary>
        /// Called when the menu is initializing.
        /// </summary>
        protected virtual void OnInitialize() {}

        /// <summary>
        /// Called after the menu becomes visible.
        /// </summary>
        protected virtual void OnShow() {}

        /// <summary>
        /// Called after the menu becomes not visible.
        /// </summary>
        protected virtual void OnHide() {}

        /// <summary>
        /// Called to update the menu logic.
        /// </summary>
        protected virtual void OnUpdate() {}

        /// <summary>
        /// Called to update the menu visuals.
        /// </summary>
        protected virtual void OnUpdateVisuals() {}
    }
}
