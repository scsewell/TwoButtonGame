using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Framework;
using BoostBlasters.MainMenus;

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
    private PlayerSelectMenu m_playerSelect;
    private LevelSelectMenu m_levelSelect;
    private ProfilesMenu m_profiles;
    private ReplayMenu m_replays;
    private SettingsMenu m_settings;
    private CreditsMenu m_credits;

    private ProfileNameMenu m_profileName;
    public ProfileNameMenu ProfileName { get { return m_profileName; } }

    private ConfirmMenu m_confirmMenu;
    public ConfirmMenu ConfirmMenu { get { return m_confirmMenu; } }
    
    private List<MenuScreen> m_menuScreens;
    private Menu m_activeMenu = Menu.None;
    private Menu m_targetMenu;
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

        m_root          = GetComponentInChildren<RootMenu>();
        m_playerSelect  = GetComponentInChildren<PlayerSelectMenu>();
        m_levelSelect   = GetComponentInChildren<LevelSelectMenu>();
        m_profiles      = GetComponentInChildren<ProfilesMenu>();
        m_profileName   = GetComponentInChildren<ProfileNameMenu>();
        m_replays       = GetComponentInChildren<ReplayMenu>();
        m_settings      = GetComponentInChildren<SettingsMenu>();
        m_credits       = GetComponentInChildren<CreditsMenu>();
        m_confirmMenu   = GetComponentInChildren<ConfirmMenu>();

        gameObject.AddComponent<CustomInputModule>();
        CustomInput customInput = gameObject.GetComponent<CustomInput>();
        customInput.SetInputs(InputManager.Instance.PlayerInputs.ToList());

        m_availableInputs = new List<PlayerBaseInput>(InputManager.Instance.PlayerInputs);
        
        m_menuScreens = new List<MenuScreen>(GetComponentsInChildren<MenuScreen>());
        m_menuScreens.ForEach(m => m.InitMenu(Main.Instance.LastRaceParams));

        switch (Main.Instance.LastRaceType)
        {
            case Main.RaceType.Race:    SetMenu(Menu.LevelSelect); break;
            case Main.RaceType.Replay:  SetMenu(Menu.Replays); break;
            default: SetMenu(Menu.Root); break;
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

    public void SetMenu(Menu menu, bool back = false)
    {
        Menu previous = m_targetMenu;

        if (previous != menu)
        {
            m_targetMenu = menu;
            
            if (previous != Menu.None)
            {
                if (back)
                {
                    PlayBackMenuSound();
                }
                else
                {
                    PlayNextMenuSound();
                }
            }
        }
    }

    private void Update()
    {
        bool useCursor = Input.GetKey(KeyCode.LeftControl);
        Cursor.lockState = true ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = useCursor;

        m_menuScreens.ForEach(m => m.UpdateMenu());

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
        if (m_activeMenu != m_targetMenu)
        {
            Menu previous = m_activeMenu;
            m_activeMenu = m_targetMenu;

            m_root.enabled          = (m_activeMenu == Menu.Root);
            m_playerSelect.enabled  = (m_activeMenu == Menu.PlayerSelect);
            m_levelSelect.enabled   = (m_activeMenu == Menu.LevelSelect);
            m_profiles.enabled      = (m_activeMenu == Menu.Profiles);
            m_profileName.enabled   = (m_activeMenu == Menu.ProfileName);
            m_replays.enabled       = (m_activeMenu == Menu.Replays);
            m_settings.enabled      = (m_activeMenu == Menu.Settings);
            m_credits.enabled       = (m_activeMenu == Menu.Credits);
            m_confirmMenu.enabled   = (m_activeMenu == Menu.Confirm);

            EventSystem.current.SetSelectedGameObject(null);

            m_menuScreens.ForEach(m => m.ResetMenu(previous == Menu.Root));
        }
        m_menuScreens.ForEach(m => m.UpdateGraphics());

        FlushSoundQueue();
        
        switch (m_activeMenu)
        {
            case Menu.Root:
                List<PlayerBaseInput> contolInputs = AvailableInputs.Where(i => !i.IsController).ToList();
                m_controls.SetActive(true);
                m_controls1.UpdateUI("Navigate", contolInputs.SelectMany(i => i.SpriteNavigate).ToList());
                m_controls2.UpdateUI("Accept",   contolInputs.SelectMany(i => i.SpriteAccept).ToList());
                m_controls3.UpdateUI("Cancel",   contolInputs.SelectMany(i => i.SpriteCancel).ToList());
                break;
            default:
                m_controls.SetActive(false);
                break;
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
        SetMenu(Menu.Loading);
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
        SetMenu(Menu.Loading);
    }
}
