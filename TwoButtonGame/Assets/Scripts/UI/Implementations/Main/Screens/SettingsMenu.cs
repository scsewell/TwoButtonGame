using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using TMPro;

using Framework.UI;
using Framework.Settings;

namespace BoostBlasters.UI.MainMenus
{
    public class SettingsMenu : MenuScreen<MainMenu>
    {
        [Header("Prefabs")]

        [SerializeField] private Tab m_categoryTabPrefab = null;
        [SerializeField] private SettingPanel m_settingSpinnerPrefab = null;

        [Header("UI Elements")]

        [SerializeField] private Button m_settingsApplyButton = null;
        [SerializeField] private Button m_settingsUseDefaultsButton = null;
        [SerializeField] private Button m_settingsBackButton = null;
        [SerializeField] private TMP_Text m_categoryTitle = null;
        [SerializeField] private TMP_Text m_description = null;
        [SerializeField] private RectTransform m_categoryTabs = null;
        [SerializeField] private RectTransform m_settingsContent = null;

        [Header("Settings")]

        [SerializeField]
        private SettingPresetGroup m_defaultSettings = null;

        private readonly Dictionary<SettingCategory, List<SettingPanel>> m_categoryToSettings = new Dictionary<SettingCategory, List<SettingPanel>>();
        private SettingCategory m_currentCategory = null;


        public override void InitMenu()
        {
            m_settingsApplyButton.onClick.AddListener(() => ApplySettings());
            m_settingsUseDefaultsButton.onClick.AddListener(() => UseDefaultSettings());
            m_settingsBackButton.onClick.AddListener(() => Menu.SetMenu(Menu.Root, TransitionSound.Back));

            foreach (var category in SettingManager.Instance.Catergories)
            {
                // create a tab for the category
                UIHelper.Create(m_categoryTabPrefab, m_categoryTabs, category.name).Init(category.Icon, () =>
                {
                    SetCategory(category);
                });

                // create a transform to hold the setting list for the category
                RectTransform settingCategory = UIHelper.Create(m_settingsContent, category.name);

                var layout = settingCategory.gameObject.AddComponent<VerticalLayoutGroup>();

                layout.spacing = 4f;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = true;

                // create the settings list
                List<SettingPanel> settings = new List<SettingPanel>();

                foreach (var setting in SettingManager.Instance.GetSettings(category))
                {
                    SettingPanel panel = UIHelper.Create(m_settingSpinnerPrefab, settingCategory);
                    panel.Init(setting);

                    settings.Add(panel);
                }

                UIHelper.SetNavigationVertical(new NavConfig()
                {
                    parent = settingCategory,
                    wrap = true,
                });

                m_categoryToSettings.Add(category, settings);

                // hide the settings until the category tab is selected
                settingCategory.gameObject.SetActive(false);
            }

            var tabs = UIHelper.SetNavigationHorizontal(new NavConfig()
            {
                parent = m_categoryTabs,
                wrap = true,
            });

            SecondaryEvents.firstSelectedGameObject = tabs[0].gameObject;
        }

        protected override void OnResetMenu(bool fullReset)
        {
            m_currentCategory = null;

            RefreshSettings();
        }

        protected override void OnUpdate()
        {
            // select the first category if none is selected
            if (m_currentCategory == null && SettingManager.Instance.Catergories.Count > 0)
            {
                SetCategory(SettingManager.Instance.Catergories[0]);
            }
        }

        protected override void OnUpdateGraphics()
        {
            // display the description for the currently selected item
            GameObject selected = PrimaryEvents.currentSelectedGameObject;
            SettingPanel setting = selected.GetComponent<SettingPanel>();

            if (setting != null)
            {
                m_description.text = setting.Setting.Description;
            }
            else
            {
                m_description.text = string.Empty;
            }
        }

        private void SetCategory(SettingCategory category)
        {
            // make sure this is a different category
            if (m_currentCategory == category)
            {
                return;
            }

            // hide the old category
            if (m_currentCategory != null)
            {
                var settings = m_categoryToSettings[m_currentCategory];
                settings[0].transform.parent.gameObject.SetActive(false);
            }

            m_currentCategory = category;

            // show the new category
            m_categoryTitle.text = m_currentCategory.name;

            if (m_currentCategory != null)
            {
                var setting = m_categoryToSettings[m_currentCategory][0];

                // enable the setting group
                setting.transform.parent.gameObject.SetActive(true);

                // prepare the selection
                PrimaryEvents.firstSelectedGameObject = setting.gameObject;
                PrimaryEvents.SetSelectedGameObject(setting.gameObject);
            }
        }

        private void UseDefaultSettings()
        {
            //m_defaultSettings.Apply();
            SettingManager.Instance.Save();
            RefreshSettings();
        }

        private void ApplySettings()
        {
            //m_categoryToSettings.ForEach(p => p.Apply());
            SettingManager.Instance.Save();
            RefreshSettings();
        }

        private void RefreshSettings()
        {
            //m_categoryToSettings.ForEach(p => p.GetValue());
        }
    }
}
