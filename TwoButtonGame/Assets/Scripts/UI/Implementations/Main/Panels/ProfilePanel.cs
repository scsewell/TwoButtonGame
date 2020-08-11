using System;

using BoostBlasters.Profiles;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI.MainMenus
{
    public class ProfilePanel : MonoBehaviour
    {
        [Header("UI Elements")]

        [SerializeField] private TextMeshProUGUI m_text;

        private Button m_button;
        private Action<ProfilePanel> m_onSelect;

        /// <summary>
        /// The uses of a profile panel.
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// The panel is selected to create a new persistent profile.
            /// </summary>
            AddNew,
            /// <summary>
            /// The panel is selected to use a temporary profile.
            /// </summary>
            Guest,
            /// <summary>
            /// The panel is selected to choose a persistent profile.
            /// </summary>
            Profile,
        }

        /// <summary>
        /// The use of this profile panel.
        /// </summary>
        public Mode CurrentMode { get; private set; }

        /// <summary>
        /// The profile assigned to this panel, or null if <see cref="CurrentMode"/> is
        /// not set to <see cref="Mode.Profile"/>;
        /// </summary>
        public Profile Profile { get; private set; }

        /// <summary>
        /// Is this panel selectable.
        /// </summary>
        public bool Interactable
        {
            get => m_button.interactable;
            set => m_button.interactable = value;
        }

        private void Awake()
        {
            m_button = GetComponentInChildren<Button>();
            m_button.onClick.AddListener(() => OnClick());
        }

        /// <summary>
        /// Configues this panel.
        /// </summary>
        /// <param name="mode">The intended use of this profile panel.</param>
        /// <param name="profile">If <paramref name="mode"/> is <see cref="Mode.Profile"/>,
        /// the profile to show or null to disable the panel.</param>
        /// <param name="onSelect">The action taken when this panel is selected.</param>
        public void Init(Mode mode, Profile profile, Action<ProfilePanel> onSelect)
        {
            CurrentMode = mode;
            m_onSelect = onSelect;

            if (mode == Mode.AddNew)
            {
                Profile = null;
                m_text.fontStyle = FontStyles.Bold;
                m_text.text = "Create New";
            }
            else if (mode == Mode.Guest)
            {
                Profile = null;
                m_text.fontStyle = FontStyles.Bold;
                m_text.text = "Guest";
            }
            else if (profile != null)
            {
                Profile = profile;
                m_text.fontStyle = FontStyles.Normal;
                m_text.text = profile.Name;
            }

            gameObject.SetActive(profile != null || mode != Mode.Profile);
        }

        private void OnClick()
        {
            m_onSelect?.Invoke(this);
        }
    }
}
