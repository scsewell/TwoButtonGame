using System;

using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI.MainMenus
{
    public class ConfirmMenu : MenuScreen
    {
        [Header("UI Elements")]

        [SerializeField] private Text m_confirmText = null;
        [SerializeField] private Button m_acceptButton = null;
        [SerializeField] private Button m_cancelButton = null;

        private Action<bool> m_onResponse = null;
        private MenuScreen m_returnToMenu = null;

        public override void InitMenu()
        {
            m_acceptButton.onClick.AddListener(() => Accept());
            m_cancelButton.onClick.AddListener(() => Cancel());
        }

        /// <summary>
        /// Opens a confirmation prompt.
        /// </summary>
        /// <param name="message">The message shown to the user.</param>
        /// <param name="onResponse">The action to complete using the received response.</param>
        /// <param name="returnToMenu">The menu to open once the reponse is received.</param>
        public void ConfirmAction(string message, Action<bool> onResponse, MenuScreen returnToMenu)
        {
            Menu.SwitchTo(this);

            m_confirmText.text = message;
            m_onResponse = onResponse;
            m_returnToMenu = returnToMenu;
        }

        public override void Back()
        {
            Cancel();
        }

        private void Accept()
        {
            m_onResponse(true);

            Menu.Sound.PlaySubmitSound();
            Menu.SwitchTo(m_returnToMenu, TransitionSound.None);
        }

        private void Cancel()
        {
            m_onResponse(false);

            Menu.SwitchTo(m_returnToMenu, TransitionSound.Back);
        }
    }
}
