using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BoostBlasters.MainMenus
{
    public abstract class MenuScreen : MonoBehaviour
    {
        [SerializeField]
        private Selectable m_defaultSelection;
        [SerializeField]
        private bool m_remeberLastSelection = false;

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
            m_canvas.enabled = false;
            OnDisableMenu();
        }

        public abstract void InitMenu(RaceParameters lastRace);

        public void ResetMenu(bool fullReset)
        {
            if (enabled)
            {
                OnResetMenu(fullReset);
            }
        }

        public void UpdateMenu()
        {
            if (enabled)
            {
                GameObject selected = EventSystem.current.currentSelectedGameObject;

                if (selected != null && selected.transform.IsChildOf(transform))
                {
                    m_lastSelected = selected.GetComponent<Selectable>();
                }

                if (selected == null || !selected.activeInHierarchy)
                {
                    if (m_remeberLastSelection && m_lastSelected != null)
                    {
                        m_lastSelected.Select();
                    }
                    else if (DefaultSelectionOverride != null)
                    {
                        DefaultSelectionOverride.Select();
                    }
                    else if (m_defaultSelection != null)
                    {
                        m_defaultSelection.Select();
                    }
                }

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

        protected virtual void OnEnableMenu() {}
        protected virtual void OnDisableMenu() {}
        protected virtual void OnResetMenu(bool fullReset) {}
        protected virtual void OnUpdate() {}
        protected virtual void OnUpdateGraphics() {}
    }
}