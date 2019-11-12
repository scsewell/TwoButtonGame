using System;

using UnityEngine;

namespace BoostBlasters.UI
{
    [CreateAssetMenu(fileName = "New MenuSoundConfig", menuName = "BoostBlasters/UI/Menu Sound Config", order = 0)]
    public class MenuSoundConfig : ScriptableObject
    {
        [Serializable]
        public class ClipConfig
        {
            [SerializeField]
            private AudioClip m_clip = null;
            public AudioClip Clip => m_clip;

            [SerializeField]
            [Range(0f, 1f)]
            private float m_volume = 1.0f;
            public float Volume => m_volume;
        }

        [SerializeField]
        private ClipConfig m_selectSound = null;
        public ClipConfig SelectSound => m_selectSound;

        [SerializeField]
        private ClipConfig m_deselectSound = null;
        public ClipConfig DeselectSound => m_deselectSound;

        [SerializeField]
        private ClipConfig m_submitSound = null;
        public ClipConfig SubmitSound => m_submitSound;

        [SerializeField]
        private ClipConfig m_cancelSound = null;
        public ClipConfig CancelSound => m_cancelSound;

        [SerializeField]
        private ClipConfig m_openMenu = null;
        public ClipConfig OpenMenu => m_openMenu;

        [SerializeField]
        private ClipConfig m_nextMenu = null;
        public ClipConfig NextMenu => m_nextMenu;

        [SerializeField]
        private ClipConfig m_backMenu = null;
        public ClipConfig BackMenu => m_backMenu;
    }
}
