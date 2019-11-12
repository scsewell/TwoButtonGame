using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;

using Framework.Settings;

namespace BoostBlasters.UI.MainMenus
{
    /// <summary>
    /// A UI widget which allows modifying a setting.
    /// </summary>
    public class SettingPanel : MonoBehaviour, IMoveHandler
    {
        [Header("UI Elements")]

        [SerializeField] private TextMeshProUGUI m_label = null;
        [SerializeField] private TextMeshProUGUI m_valueText = null;
        [SerializeField] private CanvasGroup m_leftArrow = null;
        [SerializeField] private CanvasGroup m_rightArrow = null;

        [Header("Options")]

        [SerializeField]
        [Tooltip("Enable wrapping around the options list.")]
        private bool m_wrap = false;

        private SoundPlayer m_sound = null;
        private Setting m_setting = null;
        private string[] m_options = null;

        public int CurrentIndex
        {
            get
            {
                for (int i = 0; i < m_options.Length; i++)
                {
                    if (m_options[i] == m_valueText.text)
                    {
                        return i;
                    }
                }
                return 0;
            }
        }


        private void Awake()
        {
            m_sound = GetComponentInParent<SoundPlayer>();
        }

        public SettingPanel Init(Setting setting)
        {
            m_setting = setting;

            m_label.text = setting.name;
            m_options = setting.DisplayValues;

            GetValue();

            return this;
        }

        /// <summary>
        /// Updates the display from the current settings value.
        /// </summary>
        public void GetValue()
        {
            m_valueText.text = m_setting.SerializedValue;
            SetArrows();
        }

        /// <summary>
        /// Applies the currently selected option to the setting.
        /// </summary>
        public void Apply()
        {
            m_setting.SetSerializedValue(m_valueText.text);
        }

        public void OnMove(AxisEventData eventData)
        {
            switch (eventData.moveDir)
            {
                case MoveDirection.Left: OnPreviousValue(); break;
                case MoveDirection.Right: OnNextValue(); break;
            }
        }

        private void OnPreviousValue()
        {
            int newIndex = GetRelativeIndex(-1);

            if (newIndex != CurrentIndex)
            {
                m_valueText.text = m_options[newIndex];
                m_sound.PlaySelectSound();

                SetArrows();
            }
        }

        private void OnNextValue()
        {
            int newIndex = GetRelativeIndex(1);

            if (newIndex != CurrentIndex)
            {
                m_valueText.text = m_options[newIndex];
                m_sound.PlaySelectSound();

                SetArrows();
            }
        }

        private void SetArrows()
        {
            // only enable the arrows if there is a new value to scroll to
            // in their direction
            m_leftArrow.alpha = CurrentIndex != GetRelativeIndex(-1) ? 1f : 0f;
            m_rightArrow.alpha = CurrentIndex != GetRelativeIndex(1) ? 1f : 0f;
        }

        private int GetRelativeIndex(int offset)
        {
            if (m_wrap)
            {
                return (CurrentIndex + m_options.Length + offset) % m_options.Length;
            }
            else
            {
                return Mathf.Clamp(CurrentIndex + offset, 0, m_options.Length - 1);
            }
        }
    }
}
