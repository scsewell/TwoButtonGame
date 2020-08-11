using System;

using UnityEngine;
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

        private Input m_input = null;
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
            get => m_input != null ? m_input.EventSystem.currentSelectedGameObject : m_selection;
            set
            {
                m_selection = value;

                if (m_input != null)
                {
                    m_input.EventSystem.SetSelectedGameObject(value);
                }
            }
        }

        /// <summary>
        /// Gets if this selection is currently active.
        /// </summary>
        public bool Enabled => m_input != null;

        /// <summary>
        /// Enables selection.
        /// </summary>
        /// <param name="input">The input to drive this selection with.</param>
        public void Enable(Input input)
        {
            m_input = input;

            var eventSystem = m_input.EventSystem;
            eventSystem.SetSelectedGameObject(m_selection);
            eventSystem.playerRoot = m_selectionRoot;
        }

        /// <summary>
        /// Disables selection.
        /// </summary>
        /// <param name="rememberLastSelection">Do not reset the selection when next enabled.</param>
        public void Disable(bool rememberLastSelection)
        {
            if (m_input != null)
            {
                var eventSystem = m_input.EventSystem;

                m_selection = rememberLastSelection ? eventSystem.currentSelectedGameObject : null;

                eventSystem.SetSelectedGameObject(null);
                eventSystem.playerRoot = null;

                m_input = null;
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
