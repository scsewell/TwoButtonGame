using System.Collections.Generic;
using System.Linq;

using BoostBlasters.Characters;
using BoostBlasters.Players;
using BoostBlasters.Races;
using BoostBlasters.Replays;

using Framework;
using Framework.AssetBundles;

using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI.MainMenus
{
    public class MainMenu : MenuBase
    {
        [Header("Options")]

        [SerializeField] private AssetBundleMusicReference m_music = null;

        [Header("Loading")]

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

        public List<PlayerBaseInput> UnreservedInputs => new List<PlayerBaseInput>().Where(i => !ReservedInputs.Contains(i)).ToList();
        public List<PlayerBaseInput> ReservedInputs => Get<PlayerSelectMenu>().ActiveInputs;

        protected override void Start()
        {
            base.Start();

            switch (Main.Instance.LastRaceType)
            {
                case Main.RaceType.Race:
                    SwitchTo<LevelSelectMenu>(TransitionSound.None);
                    break;
                case Main.RaceType.Replay:
                    SwitchTo<ReplayMenu>(TransitionSound.None);
                    break;
                default:
                    SwitchTo<RootMenu>(TransitionSound.None);
                    break;
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

        public void LaunchRace()
        {
            var playerSelect = Get<PlayerSelectMenu>();
            var levelSelect = Get<LevelSelectMenu>();

            var playerCount = playerSelect.Configs.Count;
            var aiCount = levelSelect.AICountSelect.Value;

            var racers = new RacerConfig[playerCount + aiCount];

            for (var i = 0; i < racers.Length; i++)
            {
                if (i < playerCount)
                {
                    racers[i] = playerSelect.Configs[i];
                }
                else
                {
                    racers[i] = RacerConfig.CreateAI(
                        CharacterManager.Characters.PickRandom(),
                        ProfileManager.CreateTemporaryProfile($"AI {i + 1}", false)
                    );
                }
            }

            var raceParams = new RaceParameters(
                levelSelect.TrackSelect.Value,
                levelSelect.LapSelect.Value,
                racers
            );

            m_leavingMenu = true;
            m_menuExitTime = Time.unscaledTime;

            Main.Instance.LoadRace(raceParams, () =>
            {
                return GetFadeFactor() >= 0.99f;
            });

            CloseAll(TransitionSound.Next);
        }

        public async void LaunchReplay(RecordingInfo info)
        {
            m_leavingMenu = true;
            m_menuExitTime = Time.unscaledTime;

            var recording = await RecordingManager.LoadReplayAsync(info);

            Main.Instance.LoadRace(recording, () =>
            {
                return GetFadeFactor() >= 0.99f;
            });

            CloseAll(TransitionSound.Next);
        }
    }
}
