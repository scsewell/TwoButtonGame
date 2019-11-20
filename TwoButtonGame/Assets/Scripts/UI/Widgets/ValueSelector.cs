using System;

using UnityEngine;

using TMPro;

namespace BoostBlasters.UI
{
    /// <summary>
    /// A UI widget which allows selecting a value from a limited set of values.
    /// </summary>
    public class ValueSelector : MonoBehaviour
    {
        [Header("UI Elements")]

        [SerializeField] private TextMeshProUGUI m_label = null;
        [SerializeField] private TextMeshProUGUI m_valueText = null;

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
        /// The label text.
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
        /// An event triggered when the user changes the value and passes the new value.
        /// </summary>
        public event Action<string> ValueChanged;


        private void Awake()
        {
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
    }
}
