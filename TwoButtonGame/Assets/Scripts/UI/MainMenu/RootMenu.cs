using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.Menus
{
    public class RootMenu : MenuScreen
    {
        [Header("UI Elements")]
        [SerializeField] private Button m_playButton;
        [SerializeField] private Button m_tutorialButton;
        [SerializeField] private Button m_profilesButton;
        [SerializeField] private Button m_replaysButton;
        [SerializeField] private Button m_settingsButton;
        [SerializeField] private Button m_creditsButton;
        [SerializeField] private Button m_quitButton;

        public override void InitMenu()
        {
            MainMenu menu = (MainMenu)Menu;

            m_playButton.onClick.AddListener(           () => Menu.SetMenu(menu.PlayerSelect));
            //m_showHowToButton.onClick.AddListener(      () => MainMenu.SetMenu(Menu.HowToPlay));
            m_profilesButton.onClick.AddListener(       () => Menu.SetMenu(menu.Profiles));
            m_replaysButton.onClick.AddListener(        () => Menu.SetMenu(menu.Replays));
            m_settingsButton.onClick.AddListener(       () => Menu.SetMenu(menu.Settings));
            m_creditsButton.onClick.AddListener(        () => Menu.SetMenu(menu.Credits));
            m_quitButton.onClick.AddListener(           () => Application.Quit());
        }
    }
}