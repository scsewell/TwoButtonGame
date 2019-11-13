using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using Framework;
using Framework.AssetBundles;

using BoostBlasters.Players;
using BoostBlasters.Characters;
using BoostBlasters.Races;
using BoostBlasters.Replays;

namespace BoostBlasters.UI.MainMenus
{
    public class MainMenu : MenuBase<MainMenu>
    {
        [Header("UI Elements")]

        [SerializeField] private GameObject m_controls = null;
        [SerializeField] private ControlPanel m_controls1 = null;
        [SerializeField] private ControlPanel m_controls2 = null;
        [SerializeField] private ControlPanel m_controls3 = null;

        [Header("Options")]

        [SerializeField] private AssetBundleMusicReference m_music = null;
        [SerializeField] private GameObject m_background = null;

        [Header("Loading")]

        [SerializeField]
        private Image m_fade = null;
        [SerializeField]
        [Range(0f, 5f)]
        private float m_fadeInDuration = 0.5f;
        [SerializeField]
        [Range(0f, 5f)]
        private float m_fadeOutDuration = 2.5f;
        [SerializeField]
        [Range(0f, 5f)]
        private float m_fadePower = 3.0f;

        public RootMenu Root { get; private set; }
        public PlayerSelectMenu PlayerSelect { get; private set; }
        public LevelSelectMenu LevelSelect { get; private set; }
        public ProfilesMenu Profiles { get; private set; }
        public ReplayMenu Replays { get; private set; }
        public SettingsMenu Settings { get; private set; }
        public CreditsMenu Credits { get; private set; }
        public ProfileNameMenu ProfileName { get; private set; }
        public ConfirmMenu Confirm { get; private set; }

        private bool m_isQuitting = false;
        private float m_menuLoadTime;
        private float m_menuExitTime;

        public List<PlayerBaseInput> AvailableInputs { get; private set; }

        public List<PlayerBaseInput> UnreservedInputs => AvailableInputs.Where(i => !ReservedInputs.Contains(i)).ToList();

        public List<PlayerBaseInput> ReservedInputs => PlayerSelect.ActiveInputs;

        private void Awake()
        {
            InitBase();

            Root = GetComponentInChildren<RootMenu>();
            PlayerSelect = GetComponentInChildren<PlayerSelectMenu>();
            LevelSelect = GetComponentInChildren<LevelSelectMenu>();
            Profiles = GetComponentInChildren<ProfilesMenu>();
            ProfileName = GetComponentInChildren<ProfileNameMenu>();
            Replays = GetComponentInChildren<ReplayMenu>();
            Settings = GetComponentInChildren<SettingsMenu>();
            Credits = GetComponentInChildren<CreditsMenu>();
            Confirm = GetComponentInChildren<ConfirmMenu>();

            AvailableInputs = new List<PlayerBaseInput>();

            switch (Main.Instance.LastRaceType)
            {
                case Main.RaceType.Race:
                    SetMenu(LevelSelect, TransitionSound.None);
                    break;
                case Main.RaceType.Replay:
                    SetMenu(Replays, TransitionSound.None);
                    break;
                default:
                    SetMenu(Root, TransitionSound.None);
                    break;
            }

            m_fade.color = Color.black;
            m_menuLoadTime = Time.time;
            m_background.SetActive(true);

            AudioManager.Instance.PlayMusic(m_music);
        }

        private void Update()
        {
            UpdateBase();

            float factor = GetFadeFactor();
            m_fade.color = new Color(0f, 0f, 0f, factor);

            AudioManager.Instance.Volume = Mathf.MoveTowards(AudioManager.Instance.Volume, 1f - factor, Time.unscaledDeltaTime / 0.35f);
        }

        private void LateUpdate()
        {
            LateUpdateBase((previous) => previous == Root);

            if (ActiveScreen == Root)
            {
                List<PlayerBaseInput> contolInputs = AvailableInputs.Where(i => !i.IsController).ToList();
                m_controls.SetActive(true);
                m_controls1.UpdateUI("Navigate", contolInputs.SelectMany(i => i.SpriteNavigate).ToList());
                m_controls2.UpdateUI("Accept", contolInputs.SelectMany(i => i.SpriteAccept).ToList());
                m_controls3.UpdateUI("Cancel", contolInputs.SelectMany(i => i.SpriteCancel).ToList());
            }
            else
            {
                m_controls.SetActive(false);
            }
        }

        private float GetFadeFactor()
        {
            float fac = 1f - Mathf.Clamp01((Time.time - m_menuLoadTime) / m_fadeInDuration);

            if (m_isQuitting)
            {
                fac = Mathf.Lerp(fac, 1f, Mathf.Clamp01((Time.unscaledTime - m_menuExitTime) / m_fadeOutDuration));
            }

            return Mathf.Sin((Mathf.PI / 2f) * Mathf.Pow(fac, m_fadePower));
        }

        public void LaunchRace()
        {
            int playerCount = PlayerSelect.Configs.Count;
            int aiCount = LevelSelect.AICountSelect.Value;

            RacerConfig[] racers = new RacerConfig[playerCount + aiCount];

            for (int i = 0; i < racers.Length; i++)
            {
                if (i < playerCount)
                {
                    racers[i] = PlayerSelect.Configs[i];
                }
                else
                {
                    racers[i] = RacerConfig.CreateAI(
                        CharacterManager.Characters.PickRandom(),
                        ProfileManager.GetTemporaryProfile($"AI {i + 1}", false)
                    );
                }
            }

            RaceParameters raceParams = new RaceParameters(
                LevelSelect.TrackSelect.Value,
                LevelSelect.LapSelect.Value,
                racers
            );

            m_isQuitting = true;
            m_menuExitTime = Time.unscaledTime;

            Main.Instance.LoadRace(raceParams, () =>
            {
                return GetFadeFactor() >= 0.99f;
            });

            SetMenu(null);
        }

        public async void LaunchReplay(RecordingInfo info)
        {
            m_isQuitting = true;
            m_menuExitTime = Time.unscaledTime;

            Recording recording = await RecordingManager.Instance.LoadReplayAsync(info);

            Main.Instance.LoadRace(recording, () =>
            {
                return GetFadeFactor() >= 0.99f;
            });

            SetMenu(null);
        }
    }
}
