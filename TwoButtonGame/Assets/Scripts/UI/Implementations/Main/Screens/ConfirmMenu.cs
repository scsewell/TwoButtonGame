using System;

using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI.MainMenus
{
    public class ConfirmMenu : MenuScreen<MainMenu>
    {
        [Header("UI Elements")]

        [SerializeField] private Text m_confirmText = null;
        [SerializeField] private Button m_acceptButton = null;
        [SerializeField] private Button m_cancelButton = null;

        private Action<bool> m_onResponse = null;
        private MenuScreen<MainMenu> m_returnToMenu = null;

        public override void InitMenu()
        {
            m_acceptButton.onClick.AddListener(() => Accept());
            m_cancelButton.onClick.AddListener(() => Cancel());
        }

        public void ConfirmAction(string text, Action<bool> onResponse, MenuScreen<MainMenu> returnToMenu)
        {
            Menu.SetMenu(Menu.Confirm);

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

            Menu.Sound.PlaySubmitSound();
            Menu.SetMenu(m_returnToMenu);
        }

        private void Cancel()
        {
            m_onResponse(false);

            Menu.SetMenu(m_returnToMenu, TransitionSound.Back);
        }
    }
}
