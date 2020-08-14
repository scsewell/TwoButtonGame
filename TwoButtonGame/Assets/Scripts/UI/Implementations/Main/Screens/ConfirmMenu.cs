using System;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace BoostBlasters.UI.MainMenus
{
    public class ConfirmMenu : MenuScreen
    {
        [Header("UI Elements")]

        [SerializeField] private TextMeshProUGUI m_promt = null;
        [SerializeField] private Button m_accept = null;
        [SerializeField] private Button m_cancel = null;


        private Action<bool> m_onResponse = null;
        private MenuScreen m_returnToMenu = null;


        protected override void OnInitialize()
        {
            m_accept.onClick.AddListener(() => Accept());
            m_cancel.onClick.AddListener(() => Cancel());
        }

        /// <summary>
        /// Displays a confirmation prompt.
        /// </summary>
        /// <param name="message">The message shown to the user.</param>
        /// <param name="onResponse">The action to complete using the received response.</param>
        /// <param name="returnToMenu">The menu to open once the reponse is received.</param>
        public void ConfirmAction(string message, Action<bool> onResponse, MenuScreen returnToMenu)
        {
            Menu.Open(this, TransitionSound.Next);

            m_promt.text = message;
            m_onResponse = onResponse;
            m_returnToMenu = returnToMenu;
        }

        public override void Back()
        {
            Cancel();
        }

        private void Cancel()
        {
            Complete(false);
        }

        private void Accept()
        {
            Complete(true);
        }

        private void Complete(bool accept)
        {
            var sound = accept ? TransitionSound.Next : TransitionSound.Back;

            Menu.Close(this, sound);
            Menu.Open(m_returnToMenu, sound);

            m_onResponse?.Invoke(accept);
        }
    }
}
