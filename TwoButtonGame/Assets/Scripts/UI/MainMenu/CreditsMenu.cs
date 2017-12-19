using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.Menus
{
    public class CreditsMenu : MenuScreen
    {
        [Header("UI Elements")]
        [SerializeField] private Button m_creditsBackButton;

        public override void InitMenu()
        {
            m_creditsBackButton.onClick.AddListener(() => Menu.SetMenu(((MainMenu)Menu).Root, MenuBase.TransitionSound.Back));
        }
    }
}
