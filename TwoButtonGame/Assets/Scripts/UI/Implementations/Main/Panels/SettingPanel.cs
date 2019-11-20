using UnityEngine;

using Framework;
using Framework.Settings;

namespace BoostBlasters.UI.MainMenus
{
    /// <summary>
    /// A UI widget which allows modifying a setting.
    /// </summary>
    public class SettingPanel : MonoBehaviour
    {
        private ValueSelector m_selector = null;

        /// <summary>
        /// The setting controlled by this panel.
        /// </summary>
        public Setting Setting { get; private set; } = null;


        private void Awake()
        {
            m_selector = GetComponent<ValueSelector>();
            m_selector.ValueChanged += Apply;
        }

        private void OnDestroy()
        {
            m_selector.ValueChanged -= Apply;
        }

        public SettingPanel Init(Setting setting)
        {
            Setting = setting;

            m_selector.Label = setting.name;
            m_selector.Options = setting.DisplayValues;

            if (m_selector is Slider slider)
            {
                if (setting is RangeSetting<int> intRange)
                {
                    slider.Range = new MinMaxRange(intRange.Min, intRange.Max);
                }
                if (setting is RangeSetting<float> floatRange)
                {
                    slider.Range = new MinMaxRange(floatRange.Min, floatRange.Max);
                }
            }

            LoadValue();

            return this;
        }

        /// <summary>
        /// Updates the display from the setting's current value.
        /// </summary>
        public void LoadValue()
        {
            m_selector.Value = Setting.SerializedValue;
        }

        private void Apply(string value)
        {
            Setting.SetSerializedValue(value);
        }
    }
}
