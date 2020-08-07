﻿using System;
using System.Collections.Generic;

using Framework.Settings;
using Framework.UI;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI.MainMenus
{
    public class SettingsMenu : MenuScreen
    {
        [Header("Prefabs")]

        [SerializeField] private Tab m_tabPrefab = null;
        [SerializeField] private Dropdown m_dropdownPrefab = null;
        [SerializeField] private Spinner m_spinnerPrefab = null;
        [SerializeField] private Slider m_sliderPrefab = null;
        [SerializeField] private Button m_buttonPrefab = null;

        [Header("UI Elements")]

        [SerializeField] private TMP_Text m_categoryTitle = null;
        [SerializeField] private TMP_Text m_description = null;
        [SerializeField] private RectTransform m_categoryTabs = null;
        [SerializeField] private RectTransform m_settingsContent = null;

        [Header("Settings")]

        [SerializeField]
        private List<CategoryDefaults> m_defaultSettings = null;

        [Serializable]
        private struct CategoryDefaults
        {
            [SerializeField]
            private SettingCategory m_category;

            [SerializeField]
            private SettingPresetGroup m_preset;

            public SettingCategory Category => m_category;
            public SettingPresetGroup Preset => m_preset;
        }

        private readonly Dictionary<SettingCategory, List<SettingPanel>> m_categoryToSettings = new Dictionary<SettingCategory, List<SettingPanel>>();
        private SettingCategory m_currentCategory = null;


        protected override void OnInitialize()
        {
            foreach (var category in SettingManager.Instance.Catergories)
            {
                // create a tab for the category
                var tab = UIHelper.Create(m_tabPrefab, m_categoryTabs, category.name);
                tab.Icon = category.Icon;
                tab.Selected += () => SetCategory(category);

                // create a transform to hold the setting list for the category
                var settingCategory = UIHelper.Create(m_settingsContent, category.name);

                var layout = settingCategory.gameObject.AddComponent<VerticalLayoutGroup>();

                layout.spacing = 4f;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = true;

                // create the settings list
                var settings = new List<SettingPanel>();

                foreach (var setting in SettingManager.Instance.GetSettings(category))
                {
                    GameObject go;

                    switch (setting.DisplayMode)
                    {
                        case SettingDisplayMode.Dropdown:
                            go = UIHelper.Create(m_dropdownPrefab, settingCategory, setting.name).gameObject;
                            break;
                        case SettingDisplayMode.Slider:
                            go = UIHelper.Create(m_sliderPrefab, settingCategory, setting.name).gameObject;
                            break;
                        case SettingDisplayMode.Spinner:
                            go = UIHelper.Create(m_spinnerPrefab, settingCategory, setting.name).gameObject;
                            break;
                        default:
                            Debug.LogError($"Setting \"{setting}\" uses unsupported display mode {setting.DisplayMode}!");
                            continue;
                    }

                    var panel = go.AddComponent<SettingPanel>().Init(setting);
                    settings.Add(panel);
                }

                // add a button to restore defaults for this category
                UIHelper.AddSpacer(settingCategory, 10f);

                var defaultsButton = UIHelper.Create(m_buttonPrefab, settingCategory);
                defaultsButton.GetComponentInChildren<TMP_Text>().text = "Defaults";
                defaultsButton.onClick.AddListener(() =>
                {
                    ApplyDefaults(category);
                });

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

            SecondarySelection.DefaultSelectionOverride = tabs[0].gameObject;
        }

        protected override void OnHide()
        {
            // save the settings when leaving the menu
            SettingManager.Instance.Save();

            SetCategory(null);
        }

        protected override void OnUpdate()
        {
            // select the first category if none is selected
            if (m_currentCategory == null && SettingManager.Instance.Catergories.Count > 0)
            {
                SetCategory(SettingManager.Instance.Catergories[0]);
            }
        }

        protected override void OnUpdateVisuals()
        {
            // display the description for the currently selected item
            var selected = PrimarySelection.Current;

            if (selected)
            {
                var setting = selected.GetComponent<SettingPanel>();

                if (setting != null)
                {
                    m_description.text = setting.Setting.Description;
                }
                else if (selected.GetComponent<Button>() != null)
                {
                    m_description.text = "Restore the default settings.";
                }
                else
                {
                    m_description.text = string.Empty;
                }
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
                PrimarySelection.Current = null;

                var settings = m_categoryToSettings[m_currentCategory];
                settings[0].transform.parent.gameObject.SetActive(false);
            }

            m_currentCategory = category;

            // show the new category
            m_categoryTitle.text = m_currentCategory != null ? m_currentCategory.name : null;

            if (m_currentCategory != null)
            {
                // load the category's settings
                var settings = m_categoryToSettings[m_currentCategory];

                foreach (var setting in settings)
                {
                    setting.LoadValue();
                }

                // enable the setting group
                var first = settings[0];
                first.transform.parent.gameObject.SetActive(true);

                // prepare the selection
                PrimarySelection.DefaultSelectionOverride = first.gameObject;
                PrimarySelection.SelectDefault();
            }
        }

        private void ApplyDefaults(SettingCategory category)
        {
            if (category == null)
            {
                return;
            }

            foreach (var categoryDefaults in m_defaultSettings)
            {
                if (categoryDefaults.Category == category)
                {
                    // appy the defaults settings
                    categoryDefaults.Preset.Apply();

                    // refresh the displayed values
                    var settings = m_categoryToSettings[category];

                    foreach (var setting in settings)
                    {
                        setting.LoadValue();
                    }

                    return;
                }
            }
        }
    }
}
