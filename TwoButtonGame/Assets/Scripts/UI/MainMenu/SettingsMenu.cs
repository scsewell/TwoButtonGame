using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Framework.UI;
using Framework.SettingManagement;

namespace BoostBlasters.MainMenus
{
    public class SettingsMenu : MenuScreen
    {
        [Header("UI Elements")]
        [SerializeField] private Button m_settingsApplyButton;
        [SerializeField] private Button m_settingsUseDefaultsButton;
        [SerializeField] private Button m_settingsBackButton;
        [SerializeField] private RectTransform m_settingsContent;
        [SerializeField] private SettingPanel m_settingPrefab;

        private List<SettingPanel> m_settingPanels;

        public override void InitMenu(RaceParameters lastRace)
        {
            m_settingsBackButton.onClick.AddListener(       () => MainMenu.SetMenu(Menu.Root));
            m_settingsUseDefaultsButton.onClick.AddListener(() => UseDefaultSettings());
            m_settingsApplyButton.onClick.AddListener(      () => ApplySettings());

            m_settingPanels = new List<SettingPanel>();

            Settings settings = SettingManager.Instance.Settings;
            foreach (string category in settings.Categories)
            {
                foreach (ISetting setting in settings.CategoryToSettings[category])
                {
                    if (setting.DisplayOptions != null)
                    {
                        Func<ISetting> getSetting = () => SettingManager.Instance.Settings.GetSetting(setting.Name);
                        m_settingPanels.Add(UIHelper.Create(m_settingPrefab, m_settingsContent).Init(getSetting));
                    }
                }
            }

            Navigation explicitNav = new Navigation();
            explicitNav.mode = Navigation.Mode.Explicit;

            Navigation bottomNav = explicitNav;
            bottomNav.selectOnDown = m_settingsApplyButton;

            Selectable lastSetting = UIHelper.SetNavigationVertical(m_settingsContent, explicitNav, explicitNav, bottomNav).LastOrDefault();
            Navigation tempNav;

            tempNav = m_settingsApplyButton.navigation;
            tempNav.selectOnUp = lastSetting;
            m_settingsApplyButton.navigation = tempNav;
        }

        protected override void OnResetMenu(bool fullReset)
        {
            RefreshSettings();
        }

        private void UseDefaultSettings()
        {
            SettingManager.Instance.UseDefaults();
            SettingManager.Instance.Save();
            SettingManager.Instance.Apply();
            RefreshSettings();
        }

        private void ApplySettings()
        {
            m_settingPanels.ForEach(p => p.Apply());
            SettingManager.Instance.Save();
            SettingManager.Instance.Apply();
            RefreshSettings();
        }

        private void RefreshSettings()
        {
            m_settingPanels.ForEach(p => p.GetValue());
        }
    }
}