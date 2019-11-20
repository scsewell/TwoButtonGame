using UnityEngine;

using Framework.Settings;

namespace BoostBlasters.UI.MainMenus
{
    /// <summary>
    /// A UI widget which allows modifying a setting.
    /// </summary>
    public class SettingPanel : MonoBehaviour
    {
        private Spinner m_spinner = null;
        private Slider m_slider = null;

        /// <summary>
        /// The setting controlled by this panel.
        /// </summary>
        public Setting Setting { get; private set; } = null;


        private void Awake()
        {
            m_spinner = GetComponent<Spinner>();
            m_slider = GetComponent<Slider>();
        }

        public SettingPanel Init(Setting setting)
        {
            Setting = setting;

            if (m_spinner != null)
            {
                m_spinner.Init(setting.DisplayValues);
                m_spinner.Label = setting.name;
            }
            if (m_slider != null)
            {
                float min = 0f;
                float max = 0f;

                if (setting is RangeSetting<int> intRange)
                {
                    min = intRange.Min;
                    max = intRange.Max;
                }
                if (setting is RangeSetting<float> floatRange)
                {
                    min = floatRange.Min;
                    max = floatRange.Max;
                }

                m_slider.Init(setting.DisplayValues, min, max);
                m_slider.Label = setting.name;
            }

            GetValue();

            return this;
        }

        /// <summary>
        /// Updates the display from the current settings value.
        /// </summary>
        public void GetValue()
        {
            if (m_spinner != null)
            {
                m_spinner.Value = Setting.SerializedValue;
            }
            if (m_slider != null)
            {
                m_slider.Value = Setting.SerializedValue;
            }
        }

        /// <summary>
        /// Applies the currently selected option to the setting.
        /// </summary>
        public void Apply()
        {
            if (m_spinner != null)
            {
                Setting.SetSerializedValue(m_spinner.Value);
            }
            if (m_slider != null)
            {
                Setting.SetSerializedValue(m_slider.Value);
            }
        }
    }
}
