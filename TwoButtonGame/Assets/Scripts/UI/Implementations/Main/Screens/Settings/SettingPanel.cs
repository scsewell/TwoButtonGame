using Framework;
using Framework.Settings;
using Framework.UI;

using UnityEngine;

namespace BoostBlasters.UI.MainMenu
{
    /// <summary>
    /// A UI widget which allows modifying a setting.
    /// </summary>
    public class SettingPanel : MonoBehaviour
    {
        private IValueSelector m_selector = null;
        private VerticalNavigationBuilder m_navigation = null;

        /// <summary>
        /// The setting controlled by this panel.
        /// </summary>
        public Setting Setting { get; private set; } = null;


        private void Awake()
        {
            m_selector = GetComponent<IValueSelector>();
            m_selector.ValueChanged += Apply;
        }

        private void OnDestroy()
        {
            m_selector.ValueChanged -= Apply;

            Setting.ModifiabilityChanged -= OnModifiabilityChanged;
            Setting.VisibilityChanged -= OnVisiblityChanged;
        }

        public SettingPanel Init(Setting setting, VerticalNavigationBuilder navigation)
        {
            Setting = setting;
            m_navigation = navigation;

            Setting.ModifiabilityChanged += OnModifiabilityChanged;
            Setting.VisibilityChanged += OnVisiblityChanged;

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
            OnVisiblityChanged(Setting.IsVisible);
            OnModifiabilityChanged(Setting.IsModifiable);

            return this;
        }

        /// <summary>
        /// Updates the display from the setting's current value.
        /// </summary>
        public void LoadValue()
        {
            m_selector.Value = Setting.SerializedValue;
        }

        private void Apply(int index)
        {
            Setting.SetSerializedValue(m_selector.Value);
        }

        private void OnModifiabilityChanged(bool modifiable)
        {
            m_selector.Modifiable = modifiable;
            m_navigation.UpdateNavigation();
        }

        private void OnVisiblityChanged(bool visible)
        {
            gameObject.SetActive(visible);
            m_navigation.UpdateNavigation();
        }
    }
}
