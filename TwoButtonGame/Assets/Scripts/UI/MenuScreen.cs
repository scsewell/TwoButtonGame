using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BoostBlasters.UI
{
    /// <summary>
    /// A menu that the user can interact with.
    /// </summary>
    public abstract class MenuScreen : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The element to select when going into this menu screen.")]
        private Selectable m_defaultSelection = null;

        [SerializeField]
        [Tooltip("Will the last selected element on this screen start selected when going back into this menu screen.")]
        private bool m_remeberLastSelection = false;

        /// <summary>
        /// Will the last selected element on this screen start selected when going back into this menu screen.
        /// </summary>
        public bool RemeberLastSelection
        {
            get => m_remeberLastSelection;
            set => m_remeberLastSelection = value;
        }

        private Canvas m_canvas = null;
        private Selectable m_lastSelected = null;

        /// <summary>
        /// When not null overrides the default selection for this menu screen.
        /// </summary>
        protected Selectable DefaultSelectionOverride { get; set; } = null;


        protected virtual void Awake()
        {
            m_canvas = GetComponent<Canvas>();
            m_canvas.enabled = false;
        }

        private void OnEnable()
        {
            m_canvas.enabled = true;

            OnEnableMenu();
        }

        private void OnDisable()
        {
            GameObject selected = null;

            if (EventSystem.current != null)
            {
                selected = EventSystem.current.currentSelectedGameObject;
            }

            if (selected != null && selected.transform.IsChildOf(transform))
            {
                m_lastSelected = selected.GetComponent<Selectable>();
            }

            m_canvas.enabled = false;

            OnDisableMenu();
        }

        /// <summary>
        /// Called once to initilize the menu.
        /// </summary>
        public abstract void InitMenu();

        /// <summary>
        /// Prepares the screen to be switched to.
        /// </summary>
        /// <param name="fullReset">Should the screen clear all state.</param>
        public void ResetMenu(bool fullReset)
        {
            OnResetMenu(fullReset);

            if (enabled)
            {
                HandleSelection();
            }
        }

        /// <summary>
        /// Updates the menu screen's logic.
        /// </summary>
        public void UpdateMenu()
        {
            if (enabled)
            {
                HandleSelection();
                OnUpdate();
            }
        }

        /// <summary>
        /// Updates the menu screen's visuals.
        /// </summary>
        public void UpdateGraphics()
        {
            if (enabled)
            {
                OnUpdateGraphics();
            }
        }

        /// <summary>
        /// Back out of this menu.
        /// </summary>
        public virtual void Back()
        {
        }

        protected virtual void HandleSelection()
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;

            if (selected == null || !selected.activeInHierarchy)
            {
                if (m_remeberLastSelection && m_lastSelected != null && m_lastSelected.isActiveAndEnabled)
                {
                    m_lastSelected.Select();
                    m_lastSelected = null;
                }
                else if (DefaultSelectionOverride != null && DefaultSelectionOverride.isActiveAndEnabled)
                {
                    DefaultSelectionOverride.Select();
                }
                else if (m_defaultSelection != null && m_defaultSelection.isActiveAndEnabled)
                {
                    m_defaultSelection.Select();
                }
            }
        }

        protected virtual void OnEnableMenu() { }
        protected virtual void OnDisableMenu() { }
        protected virtual void OnResetMenu(bool fullReset) { }
        protected virtual void OnUpdate() { }
        protected virtual void OnUpdateGraphics() { }
    }

    /// <summary>
    /// A menu that the user can interact with.
    /// </summary>
    /// <typeparam name="T">The menu this menu screen belongs to.</typeparam>
    [RequireComponent(typeof(Canvas))]
    public abstract class MenuScreen<T> : MenuScreen where T : MenuBase<T>
    {
        [SerializeField]
        [Tooltip("The menu screen to go to when backing out of this menu screen.")]
        private MenuScreen m_backMenu = null;

        [SerializeField]
        [Tooltip("Will pressing the back button close this menu screen even if a back menu is unassigned.")]
        private bool m_closeOnBack = false;

        /// <summary>
        /// The base menu instance that manages this menu screen.
        /// </summary>
        public T Menu { get; private set; } = null;


        protected override void Awake()
        {
            base.Awake();

            Menu = GetComponentInParent<T>();
        }

        /// <summary>
        /// Back out of this menu.
        /// </summary>
        public override void Back()
        { 
            if (m_backMenu || m_closeOnBack)
            {
                Menu.SetMenu(m_closeOnBack ? null : m_backMenu, TransitionSound.Back);
            }
        }
    }
}
