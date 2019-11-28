using System;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using TMPro;

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

        /// <summary>
        /// The label text.
        /// </summary>
        public string Label
        {
            get => m_label.text;
            set => m_label.text = value;
        }

        /// <summary>
        /// All the displayable values.
        /// </summary>
        public string[] Options { get; set; } = null;

        /// <summary>
        /// The currently selected value.
        /// </summary>
        public string Value
        {
            get => m_valueText.text;
            set
            {
                // get the index of the value in the options array
                int index = -1;
                for (int i = 0; i < Options.Length; i++)
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
                    SetValue(index);
                }
                else
                {
                    Debug.LogError($"Spinner \"{name}\" does not have value \"{value}\" as an option!");
                }
            }
        }

        /// <summary>
        /// The index of the currently selected value in the options array.
        /// </summary>
        protected int CurrentIndex { get; private set; } = -1;

        /// <summary>
        /// An event triggered when the user changes the value.
        /// </summary>
        public event Action<string> ValueChanged;


        private void Awake()
        {
            m_selectable = GetComponent<Selectable>();
            m_sound = GetComponentInParent<SoundPlayer>();
        }

        /// <summary>
        /// Selects the option at the provided index.
        /// </summary>
        /// <param name="newIndex">The new value index.</param>
        protected void SelectIndex(int newIndex)
        {
            if (CurrentIndex != newIndex)
            {
                SetValue(newIndex);
                ValueChanged?.Invoke(Value);
                m_sound.PlaySelectSound();
            }
        }

        private void SetValue(int index)
        {
            CurrentIndex = index;
            m_valueText.text = Options[CurrentIndex];

            OnValueChanged();
        }

        protected virtual void OnValueChanged()
        {
        }

        public void OnMove(AxisEventData eventData)
        {
            if (!m_selectable.IsInteractable())
            {
                return;
            }

            switch (eventData.moveDir)
            {
                case MoveDirection.Left: SelectIndex(GetRelativeIndex(eventData, -1)); break;
                case MoveDirection.Right: SelectIndex(GetRelativeIndex(eventData, 1)); break;
                default:
                    return;
            }
        }

        private int GetRelativeIndex(AxisEventData eventData, int offset)
        {
            // allow wrapping if this is not a repeat navigation input
            if (UIUtils.GetRepeatCount(eventData.currentInputModule) == 0)
            {
                return (CurrentIndex + Options.Length + offset) % Options.Length;
            }
            else
            {
                return Mathf.Clamp(CurrentIndex + offset, 0, Options.Length - 1);
            }
        }
    }
}
