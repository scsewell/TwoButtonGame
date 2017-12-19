using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Framework;
using BoostBlasters.Menus;

public class MainMenu : MenuBase
{
    [Header("UI Elements")]
    [SerializeField] private GameObject m_controls;
    [SerializeField] private ControlPanel m_controls1;
    [SerializeField] private ControlPanel m_controls2;
    [SerializeField] private ControlPanel m_controls3;

    [Header("Options")]
    [SerializeField] private MusicParams m_music;
    [SerializeField] private GameObject m_background;
    
    [Header("Loading")]
    [SerializeField]
    private Image m_fade;
    [SerializeField] [Range(0, 5)]
    private float m_fadeInDuration = 0.5f;
    [SerializeField] [Range(0, 5)]
    private float m_fadeOutDuration = 2.5f;
    [SerializeField] [Range(0, 5)]
    private float m_fadePower = 3.0f;

    private RootMenu m_root;
    public RootMenu Root { get { return m_root; } }

    private PlayerSelectMenu m_playerSelect;
    public PlayerSelectMenu PlayerSelect { get { return m_playerSelect; } }

    private LevelSelectMenu m_levelSelect;
    public LevelSelectMenu LevelSelect { get { return m_levelSelect; } }

    private ProfilesMenu m_profiles;
    public ProfilesMenu Profiles { get { return m_profiles; } }

    private ReplayMenu m_replays;
    public ReplayMenu Replays { get { return m_replays; } }

    private SettingsMenu m_settings;
    public SettingsMenu Settings { get { return m_settings; } }

    private CreditsMenu m_credits;
    public CreditsMenu Credits { get { return m_credits; } }

    private ProfileNameMenu m_profileName;
    public ProfileNameMenu ProfileName { get { return m_profileName; } }

    private ConfirmMenu m_confirm;
    public ConfirmMenu Confirm { get { return m_confirm; } }
    
    private AsyncOperation m_loading;
    private float m_menuLoadTime;
    private float m_menuExitTime;

    private List<PlayerBaseInput> m_availableInputs;
    public List<PlayerBaseInput> AvailableInputs
    {
        get { return m_availableInputs; }
    }

    public List<PlayerBaseInput> UnreservedInputs
    {
        get { return m_availableInputs.Where(i => !ReservedInputs.Contains(i)).ToList(); }
    }

    public List<PlayerBaseInput> ReservedInputs
    {
        get { return m_playerSelect.ActiveInputs; }
    }

    private void Awake()
    {
        // Ensure the GameController exists
        Debug.Log(Main.Instance.name);

        InitBase(InputManager.Instance.PlayerInputs.ToList());

        m_root          = GetComponentInChildren<RootMenu>();
        m_playerSelect  = GetComponentInChildren<PlayerSelectMenu>();
        m_levelSelect   = GetComponentInChildren<LevelSelectMenu>();
        m_profiles      = GetComponentInChildren<ProfilesMenu>();
        m_profileName   = GetComponentInChildren<ProfileNameMenu>();
        m_replays       = GetComponentInChildren<ReplayMenu>();
        m_settings      = GetComponentInChildren<SettingsMenu>();
        m_credits       = GetComponentInChildren<CreditsMenu>();
        m_confirm   = GetComponentInChildren<ConfirmMenu>();
        
        m_availableInputs = new List<PlayerBaseInput>(InputManager.Instance.PlayerInputs);
        
        switch (Main.Instance.LastRaceType)
        {
            case Main.RaceType.Race:    SetMenu(m_levelSelect, TransitionSound.None); break;
            case Main.RaceType.Replay:  SetMenu(m_replays, TransitionSound.None); break;
            default:                    SetMenu(m_root, TransitionSound.None); break;
        }

        StartCoroutine(FinishAwake());
    }

    private IEnumerator FinishAwake()
    {
        m_fade.color = Color.black;
        m_menuLoadTime = float.MaxValue;
        yield return null;
        m_menuLoadTime = Time.time;
        m_background.SetActive(true);
        AudioManager.Instance.PlayMusic(m_music);
    }
    
    private void Update()
    {
        UpdateBase();

        float factor = GetFadeFactor();
        m_fade.color = new Color(0, 0, 0, factor);

        QualitySettings.shadowDistance = 20;

        AudioManager.Instance.Volume = Mathf.MoveTowards(AudioManager.Instance.Volume, 1 - factor, Time.unscaledDeltaTime / 0.35f);

        if (m_loading != null && factor >= 1)
        {
            m_loading.allowSceneActivation = true;
        }
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
        float fac = 1 - Mathf.Clamp01((Time.time - m_menuLoadTime) / m_fadeInDuration);
        if (m_loading != null)
        {
            fac = Mathf.Lerp(fac, 1, Mathf.Clamp01((Time.unscaledTime - m_menuExitTime) / m_fadeOutDuration));
        }
        return Mathf.Sin((Mathf.PI / 2) * Mathf.Pow(fac, m_fadePower));
    }

    public void LaunchReplay(ReplayInfo info)
    {
        m_loading = Main.Instance.LoadRace(ReplayManager.Instance.LoadReplay(info));
        m_menuExitTime = Time.unscaledTime;
        SetMenu(null);
    }

    public void LaunchRace()
    {
        List<PlayerConfig> playerConfigs = new List<PlayerConfig>(m_playerSelect.CharacterConfigs);
        List<PlayerProfile> playerProfiles = new List<PlayerProfile>(m_playerSelect.PlayerProfiles);
        List<PlayerBaseInput> inputs = m_playerSelect.ActiveInputs;

        int humanCount = inputs.Count;
        int aiCount = m_levelSelect.AICountSelect.Value;
        
        for (int i = 0; i < aiCount; i++)
        {
            playerConfigs.Add(Utils.PickRandom(Main.Instance.PlayerConfigs));
            playerProfiles.Add(PlayerProfileManager.Instance.GetGuestProfile("AI " + (i + 1), false));
        }
        
        RaceParameters raceParams = new RaceParameters(
            m_levelSelect.TrackSelect.Value,
            m_levelSelect.LapSelect.Value,
            humanCount,
            aiCount,
            playerConfigs,
            playerProfiles,
            inputs,
            m_playerSelect.PlayerIndices
            );

        m_loading = Main.Instance.LoadRace(raceParams);
        m_menuExitTime = Time.unscaledTime;
        SetMenu(null);
    }
}
