using System;
using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.Menus
{
    public class ConfirmMenu : MenuScreen
    {
        [Header("UI Elements")]
        [SerializeField] private Text m_confirmText;
        [SerializeField] private Button m_acceptButton;
        [SerializeField] private Button m_cancelButton;

        private Action<bool> m_onResponse;
        private MenuScreen m_returnToMenu;

        public override void InitMenu()
        {
            m_acceptButton.onClick.AddListener(() => Accept());
            m_cancelButton.onClick.AddListener(() => Cancel());
        }

        public void ConfirmAction(string text, Action<bool> onResponse, MenuScreen returnToMenu)
        {
            Menu.SetMenu(((MainMenu)Menu).Confirm);

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

            Menu.PlaySubmitSound();
            Menu.SetMenu(m_returnToMenu);
        }

        private void Cancel()
        {
            m_onResponse(false);

            Menu.SetMenu(m_returnToMenu, MenuBase.TransitionSound.Back);
        }
    }
}
