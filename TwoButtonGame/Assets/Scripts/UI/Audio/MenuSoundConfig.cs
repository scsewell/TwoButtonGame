using System;

using UnityEngine;

namespace BoostBlasters.UI
{
    /// <summary>
    /// An asset that configures the sounds used for UI events in a menu.
    /// </summary>
    [CreateAssetMenu(fileName = "New MenuSoundConfig", menuName = "BoostBlasters/UI/Menu Sound Config", order = 0)]
    public class MenuSoundConfig : ScriptableObject
    {
        [Serializable]
        public class Config
        {
            [SerializeField]
            [Tooltip("The audio clip to play for this interaction.")]
            private AudioClip m_clip = null;

            /// <summary>
            /// The audio clip to play for this interaction.
            /// </summary>
            public AudioClip Clip => m_clip;

            [SerializeField]
            [Tooltip("The volume used when playing the clip.")]
            [Range(0f, 1f)]
            private float m_volume = 1.0f;

            /// <summary>
            /// The volume used when playing the clip.
            /// </summary>
            public float Volume => m_volume;
        }

        [SerializeField]
        [Tooltip("The sound played when a selectable UI element receives focus.")]
        private Config m_selectSound = null;

        /// <summary>
        /// The sound played when a selectable UI element receives focus.
        /// </summary>
        public Config SelectSound => m_selectSound;

        [SerializeField]
        [Tooltip("The sound played when a selectable UI element loses focus.")]
        private Config m_deselectSound = null;

        /// <summary>
        /// The sound played a selectable UI element element loses focus.
        /// </summary>
        public Config DeselectSound => m_deselectSound;

        [SerializeField]
        [Tooltip("The sound played when a user presses the submit button.")]
        private Config m_submitSound = null;

        /// <summary>
        /// The sound played when the user presses the submit button.
        /// </summary>
        public Config SubmitSound => m_submitSound;

        [SerializeField]
        [Tooltip("The sound played when a user presses the cancel button.")]
        private Config m_cancelSound = null;

        /// <summary>
        /// The sound played when the user presses the cancel button.
        /// </summary>
        public Config CancelSound => m_cancelSound;

        [SerializeField]
        [Tooltip("The sound played when a menu is first opened.")]
        private Config m_openMenu = null;

        /// <summary>
        /// The sound played when a menu is first opened.
        /// </summary>
        public Config OpenMenu => m_openMenu;

        [SerializeField]
        [Tooltip("The sound played when advancing to a new menu screen.")]
        private Config m_nextMenu = null;

        /// <summary>
        /// The sound played when advancing to a new menu screen.
        /// </summary>
        public Config NextMenu => m_nextMenu;

        [SerializeField]
        [Tooltip("The sound played when going back out of a menu screen.")]
        private Config m_backMenu = null;

        /// <summary>
        /// The sound played when going back out of a menu screen.
        /// </summary>
        public Config BackMenu => m_backMenu;
    }
}
