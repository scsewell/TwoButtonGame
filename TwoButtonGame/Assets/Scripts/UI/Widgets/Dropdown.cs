using System;
using System.Linq;

using TMPro;

using UnityEngine;

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

        /// <inheritdoc/>
        public string Label
        {
            get => m_label.text;
            set => m_label.text = value;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public string Value
        {
            get => m_options[m_dropdown.value];
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
                    m_dropdown.value = index;
                }
                else
                {
                    Debug.LogError($"Value \"{value}\" is not valid for dropdown \"{name}\"!");
                }
            }
        }

        /// <inheritdoc/>
        public int Index
        {
            get => m_dropdown.value;
            set
            {
                if (value >= 0 && value < Options.Length)
                {
                    m_dropdown.value = value;
                }
                else
                {
                    Debug.LogError($"Index {value} is not valid for dropdown \"{name}\"!");
                }
            }
        }

        /// <inheritdoc/>
        public bool Modifiable
        {
            get => m_dropdown.interactable;
            set => m_dropdown.interactable = value;
        }

        /// <inheritdoc/>
        public event Action<int> ValueChanged;


        private void Awake()
        {
            m_dropdown = GetComponent<TMP_Dropdown>();
            m_dropdown.onValueChanged.AddListener(SelectIndex);
        }

        private void SelectIndex(int newIndex)
        {
            ValueChanged?.Invoke(newIndex);
        }
    }
}
