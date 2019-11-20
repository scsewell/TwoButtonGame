using UnityEngine;
using UnityEngine.EventSystems;

using TMPro;

namespace BoostBlasters.UI
{
    /// <summary>
    /// A UI widget which allows for choosing a numeric value on a range.
    /// </summary>
    public class Slider : MonoBehaviour, IMoveHandler
    {
        [Header("UI Elements")]

        [SerializeField] private TextMeshProUGUI m_label = null;
        [SerializeField] private TextMeshProUGUI m_valueText = null;
        [SerializeField] private RectTransform m_fill = null;

        private SoundPlayer m_sound = null;
        private string[] m_options = null;
        private int m_currentIndex = -1;
        private float m_min = 0f;
        private float m_max = 0f;

        /// <summary>
        /// The label text.
        /// </summary>
        public string Label
        {
            get => m_label.text;
            set => m_label.text = value;
        }

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
                for (int i = 0; i < m_options.Length; i++)
                {
                    if (m_options[i] == value)
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


        private void Awake()
        {
            m_sound = GetComponentInParent<SoundPlayer>();
        }

        /// <summary>
        /// Initializes the slider.
        /// </summary>
        /// <param name="options">The display values.</param>
        /// <param name="min">The minimum value of the setting range.</param>
        /// <param name="max">The maximum value of the setting range.</param>
        public Slider Init(string[] options, float min, float max)
        {
            m_options = options;
            m_min = min;
            m_max = max;
            return this;
        }

        public void OnMove(AxisEventData eventData)
        {
            int newIndex;
            switch (eventData.moveDir)
            {
                case MoveDirection.Left: newIndex = GetRelativeIndex(eventData, -1); break;
                case MoveDirection.Right: newIndex = GetRelativeIndex(eventData, 1); break;
                default:
                    return;
            }

            if (m_currentIndex != newIndex)
            {
                SetValue(newIndex);
                m_sound.PlaySelectSound();
            }
        }

        private int GetRelativeIndex(AxisEventData eventData, int offset)
        {
            // allow wrapping if this is not a repeat navigation input
            if (UIUtils.GetRepeatCount(eventData.currentInputModule) == 0)
            {
                return (m_currentIndex + m_options.Length + offset) % m_options.Length;
            }
            else
            {
                return Mathf.Clamp(m_currentIndex + offset, 0, m_options.Length - 1);
            }
        }

        private void SetValue(int index)
        {
            string value = m_options[index];
            
            m_currentIndex = index;
            m_valueText.text = value;

            // fill the bar to match the value in the range
            Vector2 anchor = m_fill.anchorMax;
            anchor.x = Mathf.InverseLerp(m_min, m_max, float.Parse(value));
            m_fill.anchorMax = anchor;
        }
    }
}
