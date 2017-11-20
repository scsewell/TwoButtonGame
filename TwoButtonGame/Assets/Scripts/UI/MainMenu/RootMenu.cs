using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.MainMenus
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

        public override void InitMenu(RaceParameters lastRace)
        {
            m_playButton.onClick.AddListener(           () => MainMenu.SetMenu(Menu.PlayerSelect));
            //m_showHowToButton.onClick.AddListener(      () => MainMenu.SetMenu(Menu.HowToPlay));
            m_profilesButton.onClick.AddListener(       () => MainMenu.SetMenu(Menu.Profiles));
            m_replaysButton.onClick.AddListener(        () => MainMenu.SetMenu(Menu.Replays));
            m_settingsButton.onClick.AddListener(       () => MainMenu.SetMenu(Menu.Settings));
            m_creditsButton.onClick.AddListener(        () => MainMenu.SetMenu(Menu.Credits));
            m_quitButton.onClick.AddListener(           () => Application.Quit());
        }
    }
}