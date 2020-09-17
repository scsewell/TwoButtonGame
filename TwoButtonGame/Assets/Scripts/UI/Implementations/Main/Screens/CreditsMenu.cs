using Framework.AssetBundles;
using Framework.Audio;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI.MainMenu
{
    public class CreditsMenu : MenuScreen
    {
        [Header("Prefabs")]

        [SerializeField]
        private TextMeshProUGUI m_musicAttributionPrefab = null;

        [Header("UI Elements")]

        [SerializeField]
        private VerticalLayoutGroup m_musicCredits = null;
        [SerializeField]
        private Scrollbar m_scrollbar = null;

        [Header("Options")]

        [SerializeField]
        private bool m_useAutoScroll = false;
        [SerializeField]
        [Range(0f, 0.1f)]
        private float m_scrollSpeed = 0.025f;

        private bool m_autoScroll;

        protected override void OnInitialize()
        {
            CreateMusicAttributions();
        }

        protected override void OnShow()
        {
            m_scrollbar.value = 1f;
            m_autoScroll = m_useAutoScroll;
        }

        protected override void OnUpdate()
        {
            if (m_autoScroll)
            {
                m_scrollbar.value -= Time.deltaTime * m_scrollSpeed;

                if (m_scrollbar.value < 0f)
                {
                    Back();
                }
            }
        }

        private async void CreateMusicAttributions()
        {
            var allMusic = await AssetBundleManager.LoadAssetsAsync<Music>(Bundles.MUSIC);

            foreach (var music in allMusic)
            {
                var attribution = Instantiate(m_musicAttributionPrefab, m_musicCredits.transform);
                attribution.text = new string(music.Attribution.ToCharArray());
            }
        }
    }
}
