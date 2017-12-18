using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BoostBlasters.MainMenus
{
    public abstract class MenuScreen : MonoBehaviour
    {
        [SerializeField]
        private Menu m_backMenu = Menu.Root;

        [SerializeField]
        private Selectable m_defaultSelection = null;

        [SerializeField]
        private bool m_remeberLastSelection = false;
        public bool RemeberLastSelection
        {
            get { return m_remeberLastSelection; }
            set { m_remeberLastSelection = value; }
        }

        private Canvas m_canvas;
        private Selectable m_lastSelected;

        protected Selectable DefaultSelectionOverride { get; set; }

        private MainMenu m_menu;
        public MainMenu MainMenu { get { return m_menu; } }

        protected virtual void Awake()
        {
            m_menu = GetComponentInParent<MainMenu>();
            m_canvas = GetComponent<Canvas>();
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

        public abstract void InitMenu(RaceParameters lastRace);

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
                if (MainMenu.AvailableInputs.Any(i => i.UI_Cancel) || Input.GetKeyDown(KeyCode.Escape))
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
            if (m_backMenu != Menu.None)
            {
                MainMenu.SetMenu(m_backMenu, true);
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

        protected virtual void OnEnableMenu() {}
        protected virtual void OnDisableMenu() {}
        protected virtual void OnResetMenu(bool fullReset) {}
        protected virtual void OnUpdate() {}
        protected virtual void OnUpdateGraphics() {}
    }
}