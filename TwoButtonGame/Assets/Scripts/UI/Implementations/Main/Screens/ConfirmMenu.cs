using System;

using UnityEngine;
using UnityEngine.UI;

using TMPro;
using BoostBlasters.Input;

namespace BoostBlasters.UI.MainMenu
{
    public class ConfirmMenu : MenuScreen
    {
        [Header("UI Elements")]

        [SerializeField] private TextMeshProUGUI m_promt = null;
        [SerializeField] private Button m_accept = null;
        [SerializeField] private Button m_cancel = null;


        private Action<bool> m_onResponse = null;


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
        /// <param name="input">The input to drive the menu interaction with.</param>
        public void ConfirmAction(string message, Action<bool> onResponse, BaseInput input = null)
        {
            m_promt.text = message;
            m_onResponse = onResponse;

            if (input == null)
            {
                input = InputManager.Global;
            }

            InputManager.Solo = input;
            Menu.Open(this, input, TransitionSound.Next);
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

            InputManager.Solo = null;
            Menu.Close(this, sound);

            m_onResponse?.Invoke(accept);
        }
    }
}
