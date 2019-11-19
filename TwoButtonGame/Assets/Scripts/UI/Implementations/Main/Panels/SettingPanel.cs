using UnityEngine;
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

        private SoundPlayer m_sound = null;
        private Setting m_setting = null;
        private string[] m_options = null;

        /// <summary>
        /// The setting controlled by this panel.
        /// </summary>
        public Setting Setting => m_setting;

        private int CurrentIndex
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
            int newIndex;
            switch (eventData.moveDir)
            {
                case MoveDirection.Left:    newIndex = GetRelativeIndex(eventData, -1); break;
                case MoveDirection.Right:   newIndex = GetRelativeIndex(eventData, 1);  break;
                default:
                    return;
            }

            if (newIndex != CurrentIndex)
            {
                m_valueText.text = m_options[newIndex];
                m_sound.PlaySelectSound();
            }
        }

        private int GetRelativeIndex(AxisEventData eventData, int offset)
        {
            // allow wrapping if this is not a repeat navigation input
            if (UIUtils.GetRepeatCount(eventData.currentInputModule) == 0)
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
