using System;

using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BoostBlasters.UI
{
    /// <summary>
    /// A UI widget which allows cycling between values.
    /// </summary>
    public class Spinner : MonoBehaviour, IValueSelector, IMoveHandler
    {
        [Header("UI Elements")]

        [SerializeField] private TextMeshProUGUI m_label = null;
        [SerializeField] private TextMeshProUGUI m_valueText = null;

        private Selectable m_selectable = null;
        private SoundPlayer m_sound = null;
        private int m_index = -1;

        /// <inheritdoc/>
        public string Label
        {
            get => m_label.text;
            set => m_label.text = value;
        }

        /// <inheritdoc/>
        public string[] Options { get; set; } = null;

        /// <inheritdoc/>
        public string Value
        {
            get => Options[m_index];
            set
            {
                // get the index of the value in the options array
                var index = -1;
                for (var i = 0; i < Options.Length; i++)
                {
                    if (Options[i] == value)
                    {
                        index = i;
                        break;
                    }
                }

                // if the value is valid set the selection
                if (index >= 0)
                {
                    SelectIndex(index);
                }
                else
                {
                    Debug.LogError($"Value \"{value}\" is not a valid option for spinner \"{name}\"!");
                }
            }
        }

        /// <inheritdoc/>
        public int Index
        {
            get => m_index;
            set
            {
                if (value >= 0 && value < Options.Length)
                {
                    SelectIndex(value);
                }
                else
                {
                    Debug.LogError($"Index {value} is not valid for spinner \"{name}\"!");
                }
            }
        }

        /// <inheritdoc/>
        public bool Modifiable
        {
            get => m_selectable.interactable;
            set => m_selectable.interactable = value;
        }

        /// <inheritdoc/>
        public event Action<int> ValueChanged;


        protected virtual void Awake()
        {
            m_selectable = GetComponent<Selectable>();
            m_sound = GetComponentInParent<SoundPlayer>();
        }

        public void OnMove(AxisEventData eventData)
        {
            if (!m_selectable.IsInteractable())
            {
                return;
            }

            switch (eventData.moveDir)
            {
                case MoveDirection.Left: NavigateToIndex(GetRelativeIndex(eventData, -1)); break;
                case MoveDirection.Right: NavigateToIndex(GetRelativeIndex(eventData, 1)); break;
            }
        }

        private int GetRelativeIndex(AxisEventData eventData, int offset)
        {
            // allow wrapping if this is not a repeat navigation input
            if (UIUtils.GetRepeatCount(eventData.currentInputModule) == 0)
            {
                return (m_index + offset + Options.Length) % Options.Length;
            }
            else
            {
                return Mathf.Clamp(m_index + offset, 0, Options.Length - 1);
            }
        }

        private void NavigateToIndex(int newIndex)
        {
            if (m_index != newIndex)
            {
                SelectIndex(newIndex);
                m_sound.PlaySelectSound();
            }
        }

        private void SelectIndex(int newIndex)
        {
            if (m_index != newIndex)
            {
                m_index = newIndex;

                if (m_valueText != null)
                {
                    m_valueText.text = Options[newIndex];
                }

                ValueChanged?.Invoke(newIndex);
            }
        }
    }
}
