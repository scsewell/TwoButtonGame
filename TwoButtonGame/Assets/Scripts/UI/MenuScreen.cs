using UnityEngine;

namespace BoostBlasters.UI
{
    /// <summary>
    /// A component that manages a menu screen the user can interact with.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public abstract class MenuScreen : MonoBehaviour
    {
        [Header("Interaction")]

        [SerializeField]
        [Tooltip("The primary selection configuration.")]
        private Selection m_primarySelection;

        [SerializeField]
        [Tooltip("The selection used for secondary navigation events (ex. L/R bumpers).")]
        private Selection m_secondarySelection;

        [SerializeField]
        [Tooltip("Enable selection and input for this menu screen automatically when it is shown.")]
        private bool m_enableInteractionOnShow = true;

        [SerializeField]
        [Tooltip("Remeber the last selected elements when disabling interaction on this screen and select them again when interaction is next enabled.")]
        private bool m_remeberLastSelection = false;

        [Header("Back")]

        [SerializeField]
        [Tooltip("The default action to take when the back button is pressed in this menu. " +
            "Use \"Hide\" to hide this menu screen. " +
            "Use \"Switch\" to hide this menu screen and open the one set using \"Back Menu\" ")]
        protected BackMode m_backMode = BackMode.None;

        protected enum BackMode
        {
            None,
            Hide,
            Switch,
        }

        [SerializeField]
        [Tooltip("The menu screen to go to when \"Back Mode\" is set to \"Switch\".")]
        protected MenuScreen m_backMenu = null;


        private Canvas m_canvas = null;

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
        public bool Visible
        {
            get => m_canvas.enabled;
            set => m_canvas.enabled = value;
        }

        /// <summary>
        /// Can this menu have a selection and receive input.
        /// </summary>
        public bool Interactable => m_primarySelection.Enabled && m_secondarySelection.Enabled;


        protected virtual void Awake()
        {
            m_canvas = GetComponent<Canvas>();
            m_canvas.enabled = false;

            Menu = GetComponentInParent<MenuBase>();

            m_primarySelection.SelectDefault();
            m_secondarySelection.SelectDefault();
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
        public void Show(MenuInput input)
        {
            if (m_enableInteractionOnShow)
            {
                SetInput(input);
            }

            if (!Visible)
            {
                Visible = true;
                OnShow();
            }
        }

        /// <summary>
        /// Makes the menu not visible.
        /// </summary>
        public void Hide()
        {
            SetInput(null);

            if (Visible)
            {
                Visible = false;
                OnHide();
            }
        }

        public void SetInput(MenuInput input)
        {
            if (!Interactable && input != null)
            {
                m_primarySelection.Enable(input.Primary);
                m_secondarySelection.Enable(input.Secondary);
            }
            else if (Interactable && input == null)
            {
                m_primarySelection.Disable(m_remeberLastSelection);
                m_secondarySelection.Disable(m_remeberLastSelection);
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
            if (Interactable)
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
            if (Interactable)
            {
                switch (m_backMode)
                {
                    case BackMode.Switch:
                        Menu.SwitchTo(m_backMenu, TransitionSound.Back);
                        break;
                    case BackMode.Hide:
                        Menu.Close(this, TransitionSound.Back);
                        break;
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
