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

        protected override void OnInitialize()
        {
            m_acceptButton.onClick.AddListener(() => Accept());
            m_cancelButton.onClick.AddListener(() => Cancel());
        }

        /// <summary>
        /// Displays a confirmation prompt.
        /// </summary>
        /// <remarks>
        /// This variant will open this menu additively.
        /// </remarks>
        /// <param name="message">The message shown to the user.</param>
        /// <param name="onResponse">The action to complete using the received response.</param>
        public void ConfirmAction(string message, Action<bool> onResponse)
        {
            Menu.Open(this, TransitionSound.Next);

            m_confirmText.text = message;
            m_onResponse = onResponse;
            m_returnToMenu = null;
        }

        /// <summary>
        /// Displays a confirmation prompt.
        /// </summary>
        /// <remarks>
        /// This variant will temporarily switch away from the current menu.
        /// </remarks>
        /// <param name="message">The message shown to the user.</param>
        /// <param name="onResponse">The action to complete using the received response.</param>
        /// <param name="returnToMenu">The menu to open once the reponse is received.</param>
        public void ConfirmAction(string message, Action<bool> onResponse, MenuScreen returnToMenu)
        {
            Menu.SwitchTo(this, TransitionSound.Next);

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
            Complete();

            Menu.Sound.PlaySubmitSound();
            m_onResponse(true);
        }

        private void Cancel()
        {
            Complete();

            Menu.Sound.PlayCancelSound();
            m_onResponse(false);
        }

        private void Complete()
        {
            if (m_returnToMenu != null)
            {
                Menu.SwitchTo(m_returnToMenu, TransitionSound.None);
            }
            else
            {
                Menu.Close(this, TransitionSound.None);
            }
        }
    }
}
