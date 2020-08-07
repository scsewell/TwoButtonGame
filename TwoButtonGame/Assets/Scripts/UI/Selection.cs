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
        [Tooltip("The input module used to trigger events for this selection. If not assigned, this selection is not used.")]
        private InputSystemUIInputModule m_inputModulePrefab = null;

        [SerializeField]
        [Tooltip("The default element to select when initializing the selection.")]
        private Selectable m_defaultSelection = null;

        private GameObject m_inputModule = null;
        private MultiplayerEventSystem m_eventSystem = null;

        /// <summary>
        /// When not null, overrides the default selection for this menu screen.
        /// </summary>
        public GameObject DefaultSelectionOverride { get; set; } = null;

        /// <summary>
        /// The current selection.
        /// </summary>
        public GameObject Current
        {
            get => m_eventSystem != null ? m_eventSystem.currentSelectedGameObject : null;
            set
            {
                if (m_eventSystem != null)
                {
                    m_eventSystem.SetSelectedGameObject(value);
                }
            }
        }

        /// <summary>
        /// Initializes the input and event system.
        /// </summary>
        /// <param name="screen">The menu screen using this selection.</param>
        public void Initialize(MenuScreen screen)
        {
            if (m_inputModule == null && m_inputModulePrefab != null)
            {
                m_inputModule = UnityEngine.Object.Instantiate(m_inputModulePrefab, screen.transform).gameObject;

                m_eventSystem = m_inputModule.GetComponent<MultiplayerEventSystem>();
                m_eventSystem.firstSelectedGameObject = m_defaultSelection != null ? m_defaultSelection.gameObject : null;
                m_eventSystem.playerRoot = screen.gameObject;

                OnDisable(false);
            }
        }

        /// <summary>
        /// Enables input and selection.
        /// </summary>
        public void OnEnable()
        {
            if (m_inputModule != null)
            {
                m_inputModule.SetActive(true);
            }
        }

        /// <summary>
        /// Disables input and selection.
        /// </summary>
        /// <param name="rememberLastSelection">Do not reset the selection when next enabled.</param>
        public void OnDisable(bool rememberLastSelection)
        {
            if (!rememberLastSelection && m_eventSystem != null)
            {
                m_eventSystem.SetSelectedGameObject(null);
            }
            if (m_inputModule != null)
            {
                m_inputModule.SetActive(false);
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
            else if (m_eventSystem != null)
            {
                Current = m_eventSystem.firstSelectedGameObject;
            }
        }

        /// <summary>
        /// Selects the default selectable if nothing is currently selected. 
        /// </summary>
        public void AquireSelectionIfNeeded()
        {
            if (Current == null)
            {
                SelectDefault();
            }
        }
    }
}
