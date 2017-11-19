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
        [SerializeField] private Button m_showHowToButton;
        [SerializeField] private Button m_openSettingsButton;
        [SerializeField] private Button m_showCreditsButton;
        [SerializeField] private Button m_quitButton;

        public override void InitMenu(RaceParameters lastRace)
        {
            m_playButton.onClick.AddListener(           () => MainMenu.SetMenu(Menu.PlayerSelect));
            //m_showHowToButton.onClick.AddListener(      () => MainMenu.SetMenu(Menu.HowToPlay));
            m_openSettingsButton.onClick.AddListener(   () => MainMenu.SetMenu(Menu.Settings));
            m_showCreditsButton.onClick.AddListener(    () => MainMenu.SetMenu(Menu.Credits));
            m_quitButton.onClick.AddListener(           () => Application.Quit());
        }
    }
}