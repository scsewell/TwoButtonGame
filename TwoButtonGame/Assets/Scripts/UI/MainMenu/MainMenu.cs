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

    private CustomInput m_customInput;
    private List<MenuScreen> m_menuScreens;
    private Menu m_activeMenu = Menu.None;
    private AsyncOperation m_loading;
    private float m_menuLoadTime;
    private float m_menuExitTime;
    private bool m_inputMute;

    private List<PlayerBaseInput> m_availableInputs;
    public List<PlayerBaseInput> AvailableInputs
    {
        get { return m_inputMute ? new List<PlayerBaseInput>() : m_availableInputs.Where(i => !ActiveInputs.Contains(i)).ToList(); }
    }

    public List<PlayerBaseInput> ActiveInputs
    {
        get { return m_inputMute ? new List<PlayerBaseInput>() : m_playerSelect.ActiveInputs; }
    }

    private void Awake()
    {
        // Ensure the GameController exists
        Debug.Log(Main.Instance.name);

        m_root          = GetComponentInChildren<RootMenu>();
        m_playerSelect  = GetComponentInChildren<PlayerSelectMenu>();
        m_levelSelect   = GetComponentInChildren<LevelSelectMenu>();
        m_profiles      = GetComponentInChildren<ProfilesMenu>();
        m_replays       = GetComponentInChildren<ReplayMenu>();
        m_settings      = GetComponentInChildren<SettingsMenu>();
        m_credits       = GetComponentInChildren<CreditsMenu>();

        gameObject.AddComponent<CustomInputModule>();
        m_customInput = gameObject.GetComponent<CustomInput>();
        m_customInput.SetInputs(InputManager.Instance.PlayerInputs.ToList());

        m_availableInputs = new List<PlayerBaseInput>(InputManager.Instance.PlayerInputs);

        RaceParameters lastRace = Main.Instance.LastRaceParams;

        m_menuScreens = new List<MenuScreen>(GetComponentsInChildren<MenuScreen>());
        m_menuScreens.ForEach(m => m.InitMenu(lastRace));

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
        Menu previous = m_activeMenu;

        if (previous != menu)
        {
            m_activeMenu = menu;
            
            m_root.enabled          = (menu == Menu.Root);
            m_playerSelect.enabled  = (menu == Menu.PlayerSelect);
            m_levelSelect.enabled   = (menu == Menu.LevelSelect);
            m_profiles.enabled      = (menu == Menu.Profiles);
            m_replays.enabled       = (menu == Menu.Replays);
            m_settings.enabled      = (menu == Menu.Settings);
            m_credits.enabled       = (menu == Menu.Credits);

            m_menuScreens.ForEach(m => m.ResetMenu(previous == Menu.Root));

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

            EventSystem.current.SetSelectedGameObject(null);
            m_inputMute = true;
        }
    }

    private void Update()
    {
        bool useCursor = Input.GetKey(KeyCode.LeftControl);
        Cursor.lockState = true ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = useCursor;

        if (m_availableInputs.Any(i => i.UI_Cancel))
        {
            switch (m_activeMenu)
            {
                case Menu.Loading: break;
                case Menu.PlayerSelect: break;
                case Menu.LevelSelect: SetMenu(Menu.PlayerSelect, true); break;
                default: SetMenu(Menu.Root); break;
            }
        }

        m_menuScreens.ForEach(m => m.UpdateMenu());

        float factor = GetFadeFactor();
        m_fade.color = new Color(0, 0, 0, factor);

        if (m_loading != null && factor >= 1)
        {
            m_loading.allowSceneActivation = true;
        }

        QualitySettings.shadowDistance = 20;

        AudioManager.Instance.Volume = Mathf.MoveTowards(AudioManager.Instance.Volume, 1 - factor, Time.unscaledDeltaTime / 0.35f);
    }

    private void LateUpdate()
    {
        m_inputMute = false;
        m_menuScreens.ForEach(m => m.UpdateGraphics());
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
        LevelConfig levelConfig = m_levelSelect.SelectedLevel;
        int lapCount = m_levelSelect.LapCount;
        
        List<PlayerConfig> playerConfigs = m_playerSelect.SelectedConfigs;
        List<PlayerBaseInput> inputs = m_playerSelect.ActiveInputs;
        List<int> playerIndicies = m_playerSelect.PlayerIndices;

        int humanCount = inputs.Count;
        int aiCount = 0;
        
        for (int i = 0; i < aiCount; i++)
        {
            playerConfigs.Add(Utils.PickRandom(Main.Instance.PlayerConfigs));
        }
        
        RaceParameters raceParams = new RaceParameters(levelConfig, lapCount, humanCount, aiCount, playerConfigs, inputs, playerIndicies);

        m_loading = Main.Instance.LoadRace(raceParams);
        m_menuExitTime = Time.unscaledTime;
        SetMenu(Menu.Loading);
    }
}
