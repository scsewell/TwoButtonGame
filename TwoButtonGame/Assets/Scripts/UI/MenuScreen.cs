using System;
using System.Linq;

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
        private MenuScreen m_backMenu = null;
        [SerializeField]
        private bool m_closeOnBack = false;
        [SerializeField]
        private Selectable m_defaultSelection = null;

        [SerializeField]
        private bool m_remeberLastSelection = false;
        public bool RemeberLastSelection
        {
            get => m_remeberLastSelection;
            set => m_remeberLastSelection = value;
        }

        private Canvas m_canvas = null;
        private Selectable m_lastSelected = null;

        protected Selectable DefaultSelectionOverride { get; set; } = null;

        private MenuBase m_baseMenu = null;
        public MenuBase Menu => m_baseMenu;

        protected virtual void Awake()
        {
            m_baseMenu = GetComponentInParent<MenuBase>();
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
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null && selected.transform.IsChildOf(transform))
            {
                m_lastSelected = selected.GetComponent<Selectable>();
            }

            m_canvas.enabled = false;
            OnDisableMenu();
        }

        public abstract void InitMenu();

        public void ResetMenu(bool fullReset)
        {
            OnResetMenu(fullReset);
            if (enabled)
            {
                HandleSelection();
            }
        }

        public void UpdateMenu()
        {
            if (enabled)
            {
                if (Menu.Inputs.Any(i => i.UI_Cancel) || Input.GetKeyDown(KeyCode.Escape))
                {
                    OnBack();
                }

                HandleSelection();
                OnUpdate();
            }
        }

        public void UpdateGraphics()
        {
            if (enabled)
            {
                OnUpdateGraphics();
            }
        }

        protected virtual void OnBack()
        {
            if (m_backMenu != null || m_closeOnBack)
            {
                m_baseMenu.SetMenu(m_closeOnBack ? null : m_backMenu, MenuBase.TransitionSound.Back);
            }
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
}
