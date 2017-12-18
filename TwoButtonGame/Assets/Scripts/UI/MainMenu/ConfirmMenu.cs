using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace BoostBlasters.MainMenus
{
    public class ConfirmMenu : MenuScreen
    {
        [Header("UI Elements")]
        [SerializeField] private Text m_confirmText;
        [SerializeField] private Button m_acceptButton;
        [SerializeField] private Button m_cancelButton;

        private Action<bool> m_onResponse;
        private Menu m_returnToMenu;

        public override void InitMenu(RaceParameters lastRace)
        {
            m_acceptButton.onClick.AddListener(() => Accept());
            m_cancelButton.onClick.AddListener(() => Cancel());
        }

        public void ConfirmAction(string text, Action<bool> onResponse, Menu returnToMenu)
        {
            MainMenu.SetMenu(Menu.Confirm);

            m_confirmText.text = text;
            m_onResponse = onResponse;
            m_returnToMenu = returnToMenu;
        }

        protected override void OnBack()
        {
            Cancel();
        }

        private void Accept()
        {
            m_onResponse(true);

            MainMenu.PlaySubmitSound();
            MainMenu.SetMenu(m_returnToMenu);
        }

        private void Cancel()
        {
            m_onResponse(false);

            MainMenu.SetMenu(m_returnToMenu, true);
        }
    }
}
