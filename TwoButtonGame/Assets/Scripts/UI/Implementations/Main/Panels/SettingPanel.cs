using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Framework.Settings;

namespace BoostBlasters.UI.MainMenus
{
    public class SettingPanel : MonoBehaviour, IMoveHandler
    {
        [SerializeField] private Text m_label = null;
        [SerializeField] private Text m_valueText = null;

        private SoundPlayer m_sound = null;
        private Setting m_setting = null;
        private string[] m_options = null;

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

        public void GetValue()
        {
            m_valueText.text = m_setting.SerializedValue;
        }

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

        public void OnPreviousValue()
        {
            m_valueText.text = m_options[(GetCurrentIndex() + m_options.Length - 1) % m_options.Length];
            m_sound.PlaySelectSound();
        }

        public void OnNextValue()
        {
            m_valueText.text = m_options[(GetCurrentIndex() + 1) % m_options.Length];
            m_sound.PlaySelectSound();
        }

        private int GetCurrentIndex()
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
}
