using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;

using Framework;

namespace BoostBlasters.UI
{
    /// <summary>
    /// A menu that the user can interact with.
    /// </summary>
    public abstract class MenuScreen : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The input module used for the screen's main input, if used.")]
        private InputSystemUIInputModule m_primaryInputModulePrefab = null;

        [SerializeField]
        [Tooltip("The input module used for the screen's secondary input, if used.")]
        private InputSystemUIInputModule m_secondaryInputModulePrefab = null;

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
        private InputSystemUIInputModule m_primaryInputModule = null;
        private InputSystemUIInputModule m_secondaryInputModule = null;

        protected MultiplayerEventSystem PrimaryEvents { get; private set; } = null;
        protected MultiplayerEventSystem SecondaryEvents { get; private set; } = null;

        /// <summary>
        /// When not null overrides the default selection for this menu screen.
        /// </summary>
        protected Selectable DefaultSelectionOverride { get; set; } = null;


        protected virtual void Awake()
        {
            m_canvas = GetComponent<Canvas>();
            m_canvas.enabled = false;

            if (m_primaryInputModulePrefab != null)
            {
                m_primaryInputModule = Instantiate(m_primaryInputModulePrefab, transform);

                PrimaryEvents = m_primaryInputModule.GetComponent<MultiplayerEventSystem>();
                PrimaryEvents.firstSelectedGameObject = m_defaultSelection != null ? m_defaultSelection.gameObject : null;
                PrimaryEvents.playerRoot = transform.gameObject;
            }

            if (m_secondaryInputModulePrefab != null)
            {
                m_secondaryInputModule = Instantiate(m_secondaryInputModulePrefab, transform);

                SecondaryEvents = m_secondaryInputModule.GetComponent<MultiplayerEventSystem>();
                SecondaryEvents.firstSelectedGameObject = null;
                SecondaryEvents.playerRoot = transform.gameObject;
            }
        }

        private void OnEnable()
        {
            m_canvas.enabled = true;

            DelayedEnable();

            OnEnableMenu();
        }

        private async void DelayedEnable()
        {
            // An apparent bug with the input system requires that we 
            // wait a frame before enabling the input system, otherwise
            // no inputs are captured until we disable/enable the modules.
            await new WaitForEndOfFrame();

            if (isActiveAndEnabled)
            {
                if (m_primaryInputModule != null)
                {
                    m_primaryInputModule.gameObject.SetActive(true);
                }
                if (m_secondaryInputModule != null)
                {
                    m_secondaryInputModule.gameObject.SetActive(true);
                }
            }
        }

        private void OnDisable()
        {
            // clear the selection if it should be forgotten
            if (!m_remeberLastSelection)
            {
                if (PrimaryEvents != null)
                {
                    PrimaryEvents.SetSelectedGameObject(null);
                }
                if (SecondaryEvents != null)
                {
                    SecondaryEvents.SetSelectedGameObject(null);
                }
            }

            // disable the input for this screen while it is closed
            if (m_primaryInputModule != null)
            {
                m_primaryInputModule.gameObject.SetActive(false);
            }
            if (m_secondaryInputModule != null)
            {
                m_secondaryInputModule.gameObject.SetActive(false);
            }

            m_canvas.enabled = false;

            OnDisableMenu();
        }

        /// <summary>
        /// Called once to initilize the menu.
        /// </summary>
        public abstract void InitMenu();

        /// <summary>
        /// Prepares the menu screen prior to being openned.
        /// </summary>
        /// <param name="fullReset">Should the screen clear all state.</param>
        public void ResetMenu(bool fullReset)
        {
            OnResetMenu(fullReset);
        }

        /// <summary>
        /// Updates the menu screen's logic.
        /// </summary>
        public void UpdateMenu()
        {
            if (enabled)
            {
                OnUpdate();
                HandleSelection();
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
            if (PrimaryEvents != null && PrimaryEvents.currentSelectedGameObject == null)
            {
                PrimaryEvents.SetSelectedGameObject(PrimaryEvents.firstSelectedGameObject);
            }
            if (SecondaryEvents != null && SecondaryEvents.currentSelectedGameObject == null)
            {
                SecondaryEvents.SetSelectedGameObject(SecondaryEvents.firstSelectedGameObject);
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
        [Tooltip("Will pressing the back button close this menu screen.")]
        private bool m_closeOnBack = false;

        [SerializeField]
        [Tooltip("The menu screen to go to when backing out of this menu screen.")]
        private MenuScreen m_backMenu = null;

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
            if (m_closeOnBack && m_backMenu)
            {
                Menu.SetMenu(m_backMenu, TransitionSound.Back);
            }
        }
    }
}
