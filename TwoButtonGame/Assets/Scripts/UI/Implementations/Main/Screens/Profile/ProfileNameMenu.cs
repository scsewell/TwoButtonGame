using System;

using BoostBlasters.Input;
using BoostBlasters.Profiles;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI.MainMenu
{
    public class ProfileNameMenu : MenuScreen
    {
        [Header("UI Elements")]

        [SerializeField] private TextMeshProUGUI m_titleText = null;
        [SerializeField] private TextField m_nameField = null;
        [SerializeField] private Button m_cancelButton = null;
        [SerializeField] private Button m_acceptButton = null;


        private Profile m_profile = null;
        private bool m_isNew = false;
        private Action<Profile> m_onComplete = null;


        protected override void OnInitialize()
        {
            m_cancelButton.onClick.AddListener(() => Cancel());
            m_acceptButton.onClick.AddListener(() => Accept());

            m_nameField.characterLimit = Consts.MAX_PROFILE_NAME_LENGTH;
        }

        /// <summary>
        /// Creates a new profile and ask the user to input a name.
        /// </summary>
        /// <param name="onComplete">The action to take when a name is entered.
        /// <param name="input">The input to drive the menu interaction with.</param>
        /// The returned profile is null if the rename was cancelled.</param>
        public void CreateNew(Action<Profile> onComplete, BaseInput input = null)
        {
            m_profile = ProfileManager.CreateProfile();
            m_isNew = true;
            m_onComplete = onComplete;

            m_titleText.text = "New Profile";
            m_nameField.text = string.Empty;

            Open(input);
        }

        /// <summary>
        /// Asks the user to input a new name for a profile.
        /// </summary>
        /// <param name="profile">The profile to rename.
        /// <param name="onComplete">The action to take when a name is entered.
        /// The returned profile is null if the rename was cancelled.</param>
        /// <param name="input">The input to drive the menu interaction with.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="profile"/> is null.</exception>
        public void Rename(Profile profile, Action<Profile> onComplete, BaseInput input = null)
        {
            m_profile = profile ?? throw new ArgumentNullException(nameof(profile));
            m_isNew = false;
            m_onComplete = onComplete;

            m_titleText.text = "Rename Profile";
            m_nameField.text = profile.Name;

            Open(input);
        }

        private void Open(BaseInput input)
        {
            if (input == null)
            {
                input = InputManager.Global;
            }

            InputManager.Solo = input;
            Menu.Open(this, input, TransitionSound.Next);
        }

        protected override void OnShow()
        {
            m_nameField.ActivateInputField();
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
            var trimmed = m_nameField.text.Trim();

            if (trimmed.Length > 0)
            {
                ProfileManager.RenameProfile(m_profile, trimmed);
                Complete(true);
            }
            else
            {
                Complete(false);
            }
        }

        private void Complete(bool success)
        {
            if (!success && m_isNew)
            {
                ProfileManager.DeleteProfile(m_profile);
            }

            InputManager.Solo = null;
            Menu.Close(this, success ? TransitionSound.Next : TransitionSound.Back);

            m_onComplete?.Invoke(success ? m_profile : null);
        }
    }
}
