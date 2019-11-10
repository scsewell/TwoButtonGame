using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Framework.UI;
using Framework.Settings;

namespace BoostBlasters.UI.MainMenus
{
    public class SettingsMenu : MenuScreen<MainMenu>
    {
        [Header("UI Elements")]

        [SerializeField] private Button m_settingsApplyButton = null;
        [SerializeField] private Button m_settingsUseDefaultsButton = null;
        [SerializeField] private Button m_settingsBackButton = null;
        [SerializeField] private RectTransform m_settingsContent = null;
        [SerializeField] private SettingPanel m_settingPrefab = null;

        [Header("Settings")]

        [SerializeField]
        private SettingPresetGroup m_defaultSettings = null;

        private List<SettingPanel> m_settingPanels = null;

        public override void InitMenu()
        {
            m_settingsBackButton.onClick.AddListener(() => Menu.SetMenu(Menu.Root, TransitionSound.Back));
            m_settingsUseDefaultsButton.onClick.AddListener(() => UseDefaultSettings());
            m_settingsApplyButton.onClick.AddListener(() => ApplySettings());

            m_settingPanels = new List<SettingPanel>();

            foreach (var category in SettingManager.Instance.Catergories)
            {
                foreach (var setting in SettingManager.Instance.GetSettings(category))
                {
                    m_settingPanels.Add(UIHelper.Create(m_settingPrefab, m_settingsContent).Init(setting));
                }
            }

            UIHelper.SetNavigationVertical(m_settingsContent, null, m_settingsApplyButton, null, null);
        }

        protected override void OnResetMenu(bool fullReset)
        {
            RefreshSettings();
        }

        private void UseDefaultSettings()
        {
            m_defaultSettings.Apply();
            SettingManager.Instance.Save();
            RefreshSettings();
        }

        private void ApplySettings()
        {
            m_settingPanels.ForEach(p => p.Apply());
            SettingManager.Instance.Save();
            RefreshSettings();
        }

        private void RefreshSettings()
        {
            m_settingPanels.ForEach(p => p.GetValue());
        }
    }
}
