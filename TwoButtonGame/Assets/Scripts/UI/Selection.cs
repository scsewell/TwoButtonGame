using System;

using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace BoostBlasters.UI
{
    /// <summary>
    /// A class that manages selection and input events for a menu.
    /// </summary>
    [Serializable]
    public class Selection
    {
        [SerializeField]
        [Tooltip("If set, only allows selection of children of this game object.")]
        private GameObject m_selectionRoot = null;

        [SerializeField]
        [Tooltip("The default element to select when initializing the selection.")]
        private Selectable m_defaultSelection = null;

        [SerializeField]
        [Tooltip("Remeber the last selected element when disabling input for this menu screen and select it again when input is next enabled.")]
        private bool m_remeberLastSelection = true;

        private MultiplayerEventSystem m_eventSystem = null;
        private GameObject m_selection = null;

        /// <summary>
        /// When not null, overrides the default selection.
        /// </summary>
        public GameObject DefaultSelectionOverride { get; set; } = null;

        /// <summary>
        /// The currently selected GameObject.
        /// </summary>
        public GameObject Current
        {
            get => m_eventSystem != null ? m_eventSystem.currentSelectedGameObject : m_selection;
            set
            {
                m_selection = value;

                if (m_eventSystem != null)
                {
                    m_eventSystem.SetSelectedGameObject(value);
                }
            }
        }

        /// <summary>
        /// Gets if this selection is currently active.
        /// </summary>
        public bool Enabled => m_eventSystem != null;

        /// <summary>
        /// Enables selection.
        /// </summary>
        /// <param name="input">The event system to drive this selection with.</param>
        public void Enable(MultiplayerEventSystem eventSystem)
        {
            m_eventSystem = eventSystem;

            m_eventSystem.SetSelectedGameObject(m_selection);
            m_eventSystem.playerRoot = m_selectionRoot;
        }

        /// <summary>
        /// Disables selection.
        /// </summary>
        public void Disable()
        {
            if (m_eventSystem != null)
            {
                m_selection = m_remeberLastSelection ? m_eventSystem.currentSelectedGameObject : null;

                m_eventSystem.SetSelectedGameObject(null);
                m_eventSystem.playerRoot = null;

                m_eventSystem = null;
            }
        }

        /// <summary>
        /// Selects the default selectable. 
        /// </summary>
        public void SelectDefault()
        {
            if (DefaultSelectionOverride != null)
            {
                Current = DefaultSelectionOverride.gameObject;
            }
            else if (m_defaultSelection != null)
            {
                Current = m_defaultSelection.gameObject;
            }
        }

        /// <summary>
        /// Selects the default selectable if nothing is currently selected. 
        /// </summary>
        public void AquireSelectionIfNeeded()
        {
            if (Current == null || !Current.activeInHierarchy)
            {
                SelectDefault();
            }
        }
    }
}
