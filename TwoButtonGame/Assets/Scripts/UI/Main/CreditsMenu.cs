using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI.MainMenus
{
    public class CreditsMenu : MenuScreen
    {
        [Header("UI Elements")]

        [SerializeField] private Button m_creditsBackButton = null;

        public override void InitMenu()
        {
            m_creditsBackButton.onClick.AddListener(() => Menu.SetMenu(((MainMenu)Menu).Root, MenuBase.TransitionSound.Back));
        }
    }
}
