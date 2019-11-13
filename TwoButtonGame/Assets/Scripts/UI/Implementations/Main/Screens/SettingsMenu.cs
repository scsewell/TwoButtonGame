using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Framework.UI;
using Framework.Settings;

namespace BoostBlasters.UI.MainMenus
{
    public class SettingsMenu : MenuScreen<MainMenu>
    {
        [SerializeField] private SettingPanel m_settingPanelPrefab = null;
        [SerializeField] private Transform m_categoryPanelPrefab = null;

        [Header("UI Elements")]

        [SerializeField] private Button m_settingsApplyButton = null;
        [SerializeField] private Button m_settingsUseDefaultsButton = null;
        [SerializeField] private Button m_settingsBackButton = null;
        [SerializeField] private RectTransform m_settingsContent = null;

        [Header("Settings")]

        [SerializeField]
        private SettingPresetGroup m_defaultSettings = null;

        private readonly List<SettingPanel> m_settingPanels = new List<SettingPanel>();


        public override void InitMenu()
        {
            m_settingsApplyButton.onClick.AddListener(() => ApplySettings());
            m_settingsUseDefaultsButton.onClick.AddListener(() => UseDefaultSettings());
            m_settingsBackButton.onClick.AddListener(() => Menu.SetMenu(Menu.Root, TransitionSound.Back));

            foreach (var category in SettingManager.Instance.Catergories)
            {
                UIHelper.Create(m_categoryPanelPrefab, m_settingsContent).GetComponentInChildren<TMP_Text>().text = category.name;

                foreach (var setting in SettingManager.Instance.GetSettings(category))
                {
                    m_settingPanels.Add(UIHelper.Create(m_settingPanelPrefab, m_settingsContent).Init(setting));
                }
            }

            var selectables = UIHelper.SetNavigationVertical(new NavConfig()
            {
                parent = m_settingsContent,
                down = m_settingsApplyButton,
                wrap = true,
            });

            UIHelper.SetNavigationHorizontal(new NavConfig()
            {
                parent = m_settingsApplyButton.transform.parent,
                up = selectables.Last(),
                verticalSelect = m_settingsApplyButton,
            });
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
