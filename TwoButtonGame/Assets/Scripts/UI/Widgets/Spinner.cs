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
            get => m_valueText.text;
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
                    Debug.LogError($"Spinner \"{name}\" does not have value \"{value}\" as an option!");
                }
            }
        }

        /// <inheritdoc/>
        public event Action<string> ValueChanged;

        /// <summary>
        /// The index of the currently selected value in the options array.
        /// </summary>
        protected int CurrentIndex { get; private set; } = -1;


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
                return (CurrentIndex + offset + Options.Length) % Options.Length;
            }
            else
            {
                return Mathf.Clamp(CurrentIndex + offset, 0, Options.Length - 1);
            }
        }

        private void NavigateToIndex(int newIndex)
        {
            if (CurrentIndex != newIndex)
            {
                SelectIndex(newIndex);
                m_sound.PlaySelectSound();
            }
        }

        private void SelectIndex(int newIndex)
        {
            if (CurrentIndex != newIndex)
            {
                CurrentIndex = newIndex;
                m_valueText.text = Options[newIndex];

                ValueChanged?.Invoke(Value);
            }
        }
    }
}
