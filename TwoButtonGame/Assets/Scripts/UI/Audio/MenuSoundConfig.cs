using UnityEngine;

namespace BoostBlasters.UI
{
    [CreateAssetMenu(fileName = "New MenuSoundConfig", menuName = "UI/Menu Sound Config", order = 10)]
    public class MenuSoundConfig : ScriptableObject
    {
        [SerializeField]
        private AudioClip m_selectSound = null;
        public AudioClip SelectSound => m_selectSound;

        [SerializeField]
        private AudioClip m_deselectSound = null;
        public AudioClip DeselectSound => m_deselectSound;

        [SerializeField]
        private AudioClip m_submitSound = null;
        public AudioClip SubmitSound => m_submitSound;

        [SerializeField]
        private AudioClip m_cancelSound = null;
        public AudioClip CancelSound => m_cancelSound;

        [SerializeField]
        private AudioClip m_openMenu = null;
        public AudioClip OpenMenu => m_openMenu;

        [SerializeField]
        private AudioClip m_nextMenu = null;
        public AudioClip NextMenu => m_nextMenu;

        [SerializeField]
        private AudioClip m_backMenu = null;
        public AudioClip BackMenu => m_backMenu;
    }
}
