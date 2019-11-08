using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using Framework;
using Framework.Audio;
using Framework.AssetBundles;

using BoostBlasters.Players;
using BoostBlasters.Characters;
using BoostBlasters.Races;
using BoostBlasters.Replays;

namespace BoostBlasters.UI.MainMenus
{
    public class MainMenu : MenuBase
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

        private RootMenu m_root;
        public RootMenu Root => m_root;

        private PlayerSelectMenu m_playerSelect;
        public PlayerSelectMenu PlayerSelect => m_playerSelect;

        private LevelSelectMenu m_levelSelect;
        public LevelSelectMenu LevelSelect => m_levelSelect;

        private ProfilesMenu m_profiles;
        public ProfilesMenu Profiles => m_profiles;

        private ReplayMenu m_replays;
        public ReplayMenu Replays => m_replays;

        private SettingsMenu m_settings;
        public SettingsMenu Settings => m_settings;

        private CreditsMenu m_credits;
        public CreditsMenu Credits => m_credits;

        private ProfileNameMenu m_profileName;
        public ProfileNameMenu ProfileName => m_profileName;

        private ConfirmMenu m_confirm;
        public ConfirmMenu Confirm => m_confirm;

        private bool m_isQuitting;
        private float m_menuLoadTime;
        private float m_menuExitTime;

        private List<PlayerBaseInput> m_availableInputs;
        public List<PlayerBaseInput> AvailableInputs => m_availableInputs;

        public List<PlayerBaseInput> UnreservedInputs => m_availableInputs.Where(i => !ReservedInputs.Contains(i)).ToList();

        public List<PlayerBaseInput> ReservedInputs => m_playerSelect.ActiveInputs;

        private void Awake()
        {
            InitBase(InputManager.Instance.PlayerInputs.ToList());

            m_root = GetComponentInChildren<RootMenu>();
            m_playerSelect = GetComponentInChildren<PlayerSelectMenu>();
            m_levelSelect = GetComponentInChildren<LevelSelectMenu>();
            m_profiles = GetComponentInChildren<ProfilesMenu>();
            m_profileName = GetComponentInChildren<ProfileNameMenu>();
            m_replays = GetComponentInChildren<ReplayMenu>();
            m_settings = GetComponentInChildren<SettingsMenu>();
            m_credits = GetComponentInChildren<CreditsMenu>();
            m_confirm = GetComponentInChildren<ConfirmMenu>();

            m_availableInputs = new List<PlayerBaseInput>(InputManager.Instance.PlayerInputs);

            switch (Main.Instance.LastRaceType)
            {
                case Main.RaceType.Race:
                    SetMenu(m_levelSelect, TransitionSound.None);
                    break;
                case Main.RaceType.Replay:
                    SetMenu(m_replays, TransitionSound.None);
                    break;
                default:
                    SetMenu(m_root, TransitionSound.None);
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
            LateUpdateBase((previous) => previous == m_root);

            if (ActiveMenu == m_root)
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

        public void LaunchReplay(RecordingInfo info)
        {
            m_isQuitting = true;
            m_menuExitTime = Time.unscaledTime;

            Main.Instance.LoadRace(RecordingManager.Instance.LoadReplay(info), () =>
            {
                return GetFadeFactor() >= 0.99f;
            });

            SetMenu(null);
        }

        public void LaunchRace()
        {
            List<Character> characters = new List<Character>(m_playerSelect.CharacterConfigs);
            List<Profile> playerProfiles = new List<Profile>(m_playerSelect.PlayerProfiles);
            List<PlayerBaseInput> inputs = m_playerSelect.ActiveInputs;

            int humanCount = inputs.Count;
            int aiCount = m_levelSelect.AICountSelect.Value;

            for (int i = 0; i < aiCount; i++)
            {
                characters.Add(CharacterManager.Characters.PickRandom());
                playerProfiles.Add(ProfileManager.GetGuestProfile($"AI {i + 1}", false));
            }

            RaceParameters raceParams = new RaceParameters(
                m_levelSelect.TrackSelect.Value,
                m_levelSelect.LapSelect.Value,
                humanCount,
                aiCount,
                characters,
                playerProfiles,
                inputs,
                m_playerSelect.PlayerIndices
            );

            m_isQuitting = true;
            m_menuExitTime = Time.unscaledTime;

            Main.Instance.LoadRace(raceParams, () =>
            {
                return GetFadeFactor() >= 0.99f;
            });

            SetMenu(null);
        }
    }
}
