using System;
using System.Linq;

using UnityEngine;

using TMPro;

namespace BoostBlasters.UI
{
    /// <summary>
    /// A UI widget which allows selecting from a list of values.
    /// </summary>
    public class Dropdown : MonoBehaviour, IValueSelector
    {
        [Header("UI Elements")]

        [SerializeField] private TextMeshProUGUI m_label = null;

        private TMP_Dropdown m_dropdown = null;
        private string[] m_options = null;

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
        public string[] Options
        {
            get => m_options;
            set
            {
                if (m_options != value)
                {
                    m_options = value;

                    if (m_options == null || m_options.Length == 0)
                    {
                        m_dropdown.ClearOptions();
                    }
                    else
                    {
                        m_dropdown.options = m_options.Select(o => new TMP_Dropdown.OptionData(o)).ToList();
                    }
                }
            }
        }

        /// <summary>
        /// The label text.
        /// </summary>
        public string Value
        {
            get => m_options[m_dropdown.value];
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
                    m_dropdown.value = index;
                }
                else
                {
                    Debug.LogError($"Dropdown \"{name}\" does not have value \"{value}\" as an option!");
                }
            }
        }

        /// <summary>
        /// An event triggered when the user changes the value.
        /// </summary>
        public event Action<string> ValueChanged;


        private void Awake()
        {
            m_dropdown = GetComponent<TMP_Dropdown>();

            m_dropdown.onValueChanged.AddListener(SelectIndex);
        }

        private void SelectIndex(int newIndex)
        {
            ValueChanged?.Invoke(Value);
        }
    }
}
