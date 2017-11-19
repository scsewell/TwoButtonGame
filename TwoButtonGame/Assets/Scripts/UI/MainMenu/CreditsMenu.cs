using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.MainMenus
{
    public class CreditsMenu : MenuScreen
    {
        [Header("UI Elements")]
        [SerializeField] private Button m_creditsBackButton;

        public override void InitMenu(RaceParameters lastRace)
        {
            m_creditsBackButton.onClick.AddListener(() => MainMenu.SetMenu(Menu.Root));
        }
    }
}
