﻿using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using Framework.UI;
using Framework.SettingManagement;

namespace BoostBlasters.UI.MainMenus
{
    public class SettingsMenu : MenuScreen
    {
        [Header("UI Elements")]

        [SerializeField] private Button m_settingsApplyButton = null;
        [SerializeField] private Button m_settingsUseDefaultsButton = null;
        [SerializeField] private Button m_settingsBackButton = null;
        [SerializeField] private RectTransform m_settingsContent = null;
        [SerializeField] private SettingPanel m_settingPrefab = null;

        private List<SettingPanel> m_settingPanels = null;

        public override void InitMenu()
        {
            MainMenu menu = (MainMenu)Menu;

            m_settingsBackButton.onClick.AddListener(() => Menu.SetMenu(menu.Root, MenuBase.TransitionSound.Back));
            m_settingsUseDefaultsButton.onClick.AddListener(() => UseDefaultSettings());
            m_settingsApplyButton.onClick.AddListener(() => ApplySettings());

            m_settingPanels = new List<SettingPanel>();

            SettingCollection settings = SettingManager.Instance.Settings;
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

            UIHelper.SetNavigationVertical(m_settingsContent, null, m_settingsApplyButton, null, null);
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
