using BoostBlasters.Races;

using Framework.Audio;

using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI.MainMenu
{
    public class MainMenu : MenuBase
    {
        [Header("Options")]

        [SerializeField]
        private AssetBundleMusicReference m_music = null;

        [Header("Fading")]

        [SerializeField]
        private Image m_fade = null;
        [SerializeField]
        [Range(0f, 5f)]
        private float m_fadeInDuration = 0.5f;
        [SerializeField]
        [Range(0f, 5f)]
        private float m_fadeOutDuration = 2.5f;


        private bool m_leavingMenu = false;
        private float m_menuLoadTime;
        private float m_menuExitTime;


        protected override void Start()
        {
            base.Start();

            var raceParams = Main.Instance.LastRaceParams;

            if (raceParams != null)
            {
                Get<LevelSelectMenu>().Open(raceParams, TransitionSound.None);
            }
            else
            {
                SwitchTo<RootMenu>(TransitionSound.None);
            }

            m_fade.color = Color.black;
            m_menuLoadTime = Time.unscaledTime;

            AudioManager.Instance.PlayMusic(m_music);
        }

        protected override void Update()
        {
            base.Update();

            var factor = GetFadeFactor();
            m_fade.color = new Color(0f, 0f, 0f, factor);

            AudioManager.Instance.Volume = Mathf.MoveTowards(AudioManager.Instance.Volume, 1f - factor, Time.unscaledDeltaTime / 0.35f);
        }

        private float GetFadeFactor()
        {
            var fac = 1f - Mathf.Clamp01((Time.unscaledTime - m_menuLoadTime) / m_fadeInDuration);

            if (m_leavingMenu)
            {
                fac = Mathf.Lerp(fac, 1f, Mathf.Clamp01((Time.unscaledTime - m_menuExitTime) / m_fadeOutDuration));
            }

            return Mathf.SmoothStep(fac, 1f, 0f);
        }

        /// <summary>
        /// Close the main menu scene and enter a race.
        /// </summary>
        /// <param name="raceParams">The configuration of the race.</param>
        public void LaunchRace(RaceParameters raceParams)
        {
            m_leavingMenu = true;
            m_menuExitTime = Time.unscaledTime;

            Main.Instance.LoadRace(raceParams, () =>
            {
                return GetFadeFactor() >= 0.99f;
            });

            CloseAll(TransitionSound.Next);
        }
    }
}
