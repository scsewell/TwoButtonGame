using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Framework.SettingManagement;
using Framework.UI;

public class MainMenu : Menu
{
    [SerializeField] private MusicParams m_music;
    [SerializeField] private GameObject m_background;

    [Header("Root Menu")]
    [SerializeField] private Canvas m_rootMenu;
    [SerializeField] private Button m_playButton;
    [SerializeField] private Button m_showHowToButton;
    [SerializeField] private Button m_openSettingsButton;
    [SerializeField] private Button m_showCreditsButton;
    [SerializeField] private Button m_quitButton;

    [Header("Player Select")]
    [SerializeField] private Canvas m_playerSelectMenu;
    [SerializeField] private RectTransform m_playerSelectContent;
    [SerializeField] private PlayerSelectPanel m_playerSelectPrefab;
    [SerializeField] private GameObject m_continueBar;
    [SerializeField] private Image m_continueBanner;
    [SerializeField] private ControlPanel m_continueControls;
    [SerializeField] private PlayerConfig[] m_playerConfigs;

    private List<PlayerSelectPanel> m_playerSelectPanels = new List<PlayerSelectPanel>();
    private bool m_canContine = false;
    private float m_continueTime;
    private Color m_bannerCol;

    [Header("Level Select")]
    [SerializeField] private Canvas m_levelSelectMenu;
    [SerializeField] private Text m_levelName;
    [SerializeField] private Text m_levelDifficulty;
    [SerializeField] private Image m_levelPreview;
    [SerializeField] private Image m_levelHighlight;
    [SerializeField] private ControlPanel m_levelControls1;
    [SerializeField] private ControlPanel m_levelControls2;
    [SerializeField] private ControlPanel m_levelControls3;
    [SerializeField] private LevelConfig[] m_levelConfigs;

    private int m_selectedLevel = 0;
    
    [Header("Lap Count")]
    [SerializeField] private Canvas m_lapCountMenu;
    [SerializeField] private Text m_lapCountText;
    [SerializeField] private ControlPanel m_lapControls1;
    [SerializeField] private ControlPanel m_lapControls2;
    [SerializeField] private ControlPanel m_lapControls3;
    
    [SerializeField] [Range(1, 10)]
    private int m_maxLapCount = 5;
    [SerializeField] [Range(1, 10)]
    private int m_defaultLapCount = 3;

    private int m_lapCount;

    [Header("How To Play")]
    [SerializeField] private Canvas m_howToMenu;
    [SerializeField] private Button m_howToBackButton;
    [SerializeField] private Button m_howToPreviousButton;
    [SerializeField] private Button m_howToNextButton;

    private int m_currentHowTo = 0;

    [Header("Settings")]
    [SerializeField] private Canvas m_settingsMenu;
    [SerializeField] private Button m_settingsApplyButton;
    [SerializeField] private Button m_settingsUseDefaultsButton;
    [SerializeField] private Button m_settingsBackButton;
    [SerializeField] private RectTransform m_settingsContent;
    [SerializeField] private SettingPanel m_settingPrefab;

    private List<SettingPanel> m_settingPanels = new List<SettingPanel>();

    [Header("Credits")]
    [SerializeField] private Canvas m_creditsMenu;
    [SerializeField] private Button m_creditsBackButton;

    [Header("Loading")]
    [SerializeField]
    private Image m_fade;
    [SerializeField] [Range(0, 5)]
    private float m_fadeInDuration = 0.5f;
    [SerializeField] [Range(0, 5)]
    private float m_fadeOutDuration = 2.5f;
    [SerializeField] [Range(0, 5)]
    private float m_fadePower = 3.0f;

    private AsyncOperation m_loading;
    private float m_loadFadeTime;
    private float m_menuLoadTime = float.MaxValue;

    private enum Menu
    {
        None,
        Root,
        PlayerSelect,
        LevelSelect,
        LapChoose,
        HowToPlay,
        Settings,
        Credits,
        Loading,
    }

    private Menu m_activeMenu = Menu.None;

    private void Awake()
    {
        // Ensure the GameController exists
        Debug.Log(Main.Instance.name);

        m_playButton.onClick.AddListener(() => SetMenu(Menu.PlayerSelect));
        m_showHowToButton.onClick.AddListener(() => SetMenu(Menu.HowToPlay));
        m_openSettingsButton.onClick.AddListener(() => SetMenu(Menu.Settings));
        m_showCreditsButton.onClick.AddListener(() => SetMenu(Menu.Credits));
        m_quitButton.onClick.AddListener(() => Application.Quit());
        
        m_howToBackButton.onClick.AddListener(() => SetMenu(Menu.Root));
        m_howToPreviousButton.onClick.AddListener(() => m_currentHowTo = Mathf.Max(m_currentHowTo - 1, 0));
        m_howToNextButton.onClick.AddListener(() => m_currentHowTo = Mathf.Min(m_currentHowTo + 1, 2));

        m_settingsBackButton.onClick.AddListener(() => SetMenu(Menu.Root));
        m_settingsUseDefaultsButton.onClick.AddListener(() => UseDefaultSettings());
        m_settingsApplyButton.onClick.AddListener(() => ApplySettings());

        m_creditsBackButton.onClick.AddListener(() => SetMenu(Menu.Root));

        CreateSettings();

        for (int i = 0; i < 4; i++)
        {
            PlayerSelectPanel p = Instantiate(m_playerSelectPrefab, m_playerSelectContent);
            p.Init(i, m_playerConfigs);
            m_playerSelectPanels.Add(p);
        }

        m_bannerCol = m_continueBanner.color;

        RaceParameters previousParams = Main.Instance.LastRaceParams;
        if (previousParams != null)
        {
            m_selectedLevel = Array.IndexOf(m_levelConfigs, previousParams.LevelConfig);
            m_lapCount = previousParams.Laps;

            for (int i = 0; i < previousParams.PlayerIndicies.Count; i++)
            {
                m_playerSelectPanels[previousParams.PlayerIndicies[i]].FromConfig(previousParams.PlayerConfigs[i]);
            }

            SetMenu(Menu.PlayerSelect);
        }
        else
        {
            SetMenu(Menu.Root);
            m_lapCount = m_defaultLapCount;
        }

        StartCoroutine(FinishAwake());
    }

    private IEnumerator FinishAwake()
    {
        m_fade.color = new Color(0, 0, 0, 0);
        yield return null;
        m_menuLoadTime = Time.time;
        m_background.SetActive(true);
        AudioManager.Instance.PlayMusic(m_music);
    }

    private void SetMenu(Menu menu, bool back = false)
    {
        Menu previous = m_activeMenu;

        if (previous != menu)
        {
            m_activeMenu = menu;
            
            m_rootMenu.enabled          = (menu == Menu.Root);
            m_playerSelectMenu.enabled  = (menu == Menu.PlayerSelect);
            m_levelSelectMenu.enabled   = (menu == Menu.LevelSelect);
            m_lapCountMenu.enabled      = (menu == Menu.LapChoose);
            m_howToMenu.enabled         = (menu == Menu.HowToPlay);
            m_creditsMenu.enabled       = (menu == Menu.Credits);
            m_settingsMenu.enabled      = (menu == Menu.Settings);
            
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

            switch (m_activeMenu)
            {
                case Menu.PlayerSelect: ResetPlayerSelect(previous == Menu.Root); break;
                case Menu.LevelSelect: ResetLevelSelect(); break;
                case Menu.LapChoose: ResetLapChoose(); break;
                case Menu.HowToPlay: ResetHowToPlay(); break;
                case Menu.Settings: RefreshSettings(); break;
            }

            EventSystem.current.SetSelectedGameObject(null);

            m_playerSelectPanels.ForEach(p => p.SetCameraActive(menu == Menu.PlayerSelect));
        }
    }

    private void Update()
    {
        bool useCursor = Input.GetKey(KeyCode.LeftControl);
        Cursor.lockState = true ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = useCursor;

        // ensure there is always something selected when needed
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null || !selected.activeInHierarchy)
        {
            switch (m_activeMenu)
            {
                case Menu.Root: m_playButton.Select(); break;
                case Menu.HowToPlay: m_howToBackButton.Select(); break;
                case Menu.Settings: m_settingsBackButton.Select(); break;
                case Menu.Credits: m_creditsBackButton.Select(); break;
            }
        }

        switch (m_activeMenu)
        {
            case Menu.PlayerSelect: UpdatePlayerSelect(); break;
            case Menu.LevelSelect: UpdateLevelSelect(); break;
            case Menu.LapChoose: UpdateLapChoose(); break;
            case Menu.HowToPlay: UpdateHowToPlay(); break;
        }

        float factor = GetFadeFactor();
        m_fade.color = new Color(0, 0, 0, factor);

        if (m_loading != null && factor >= 1)
        {
            m_loading.allowSceneActivation = true;
        }

        AudioManager.Instance.Volume = Mathf.MoveTowards(AudioManager.Instance.Volume, 1 - factor, Time.unscaledDeltaTime / 0.35f);
    }

    private void ResetPlayerSelect(bool fullReset)
    {
        m_playerSelectPanels.ForEach(p => p.ResetState(fullReset));
        m_continueBar.SetActive(false);
    }

    private void UpdatePlayerSelect()
    {
        int playerNum = 0;
        for (int i = 0; i < 4; i++)
        {
            bool back = m_playerSelectPanels[i].UpdatePanel(playerNum, this);

            if (m_playerSelectPanels[i].IsJoined)
            {
                playerNum++;
            }
            if (back)
            {
                SetMenu(Menu.Root, true);
            }
        }

        bool canContinue = m_playerSelectPanels.All(p => p.CanContinue) && m_playerSelectPanels.Count(p => p.IsReady) > 0;
        if (m_canContine != canContinue)
        {
            m_continueTime = Time.unscaledTime;
        }
        m_canContine = canContinue;

        m_continueBar.SetActive(canContinue);
        m_continueBanner.color = Color.Lerp(Color.white, m_bannerCol, (Time.unscaledTime - m_continueTime) / 0.5f);

        if (canContinue)
        {
            PlayerSelectPanel firstPlayer = m_playerSelectPanels.First(p => p.IsReady);

            m_continueControls.UpdateUI("Continue", firstPlayer.Input.Button1Name);

            if (firstPlayer.Continue)
            {
                SetMenu(Menu.LevelSelect);
            }
        }
    }
    
    private void ResetLevelSelect()
    {
        m_levelHighlight.color = new Color(1, 1, 1, 0);
        UpdateLevelSelectGraphics();
    }

    private void UpdateLevelSelect()
    {
        List<PlayerSelectPanel> readyPlayers = m_playerSelectPanels.Where(p => p.IsReady).ToList();
        PlayerInput input = readyPlayers.First().Input;

        if (input.Button1Up)
        {
            SetMenu(Menu.LapChoose);
        }
        else if (input.Button2Up)
        {
            m_selectedLevel = (m_selectedLevel += 1) % m_levelConfigs.Length;
            m_lapCount = m_defaultLapCount;
            m_levelHighlight.color = new Color(1, 1, 1, 0.5f);
            PlaySelectSound();
        }
        else if (input.BothDown)
        {
            SetMenu(Menu.PlayerSelect, true);
        }

        UpdateLevelSelectGraphics();
    }

    private void UpdateLevelSelectGraphics()
    {
        LevelConfig config = m_levelConfigs[m_selectedLevel];
        m_levelName.text = config.Name;
        m_levelDifficulty.text = config.LevelDifficulty.ToString();
        m_levelPreview.sprite = config.Preview;
        m_levelHighlight.color = new Color(1, 1, 1, Mathf.Lerp(m_levelHighlight.color.a, 0, Time.unscaledDeltaTime * 16f));

        if (m_playerSelectPanels.Any(p => p.IsReady))
        {
            PlayerInput input = m_playerSelectPanels.Where(p => p.IsReady).First().Input;
            m_levelControls1.UpdateUI("Accept", input.Button1Name);
            m_levelControls2.UpdateUI("Next", input.Button2Name);
            m_levelControls3.UpdateUI("Back", input.ButtonNames);
        }
    }

    private void ResetLapChoose()
    {
        UpdateLapChooseGraphics();
    }

    private void UpdateLapChoose()
    {
        List<PlayerSelectPanel> readyPlayers = m_playerSelectPanels.Where(p => p.IsReady).ToList();
        PlayerInput input = readyPlayers.First().Input;

        if (input.Button1Up)
        {
            LaunchRace();
        }
        else if (input.Button2Up)
        {
            m_lapCount = Mathf.Max((m_lapCount += 1) % (m_maxLapCount + 1), 1);
            PlaySelectSound();
        }
        else if (input.BothDown)
        {
            SetMenu(Menu.LevelSelect, true);
        }

        UpdateLapChooseGraphics();
    }

    private void UpdateLapChooseGraphics()
    {
        m_lapCountText.text = "Laps: " + m_lapCount;
        
        if (m_playerSelectPanels.Any(p => p.IsReady))
        {
            PlayerInput input = m_playerSelectPanels.Where(p => p.IsReady).First().Input;
            m_lapControls1.UpdateUI("Accept", input.Button1Name);
            m_lapControls2.UpdateUI("Change", input.Button2Name);
            m_lapControls3.UpdateUI("Back", input.ButtonNames);
        }
    }

    private void ResetHowToPlay()
    {
        m_currentHowTo = 0;

        UpdateHowToPlay();
    }

    private void UpdateHowToPlay()
    {
    }

    private void CreateSettings()
    {
        Settings settings = SettingManager.Instance.Settings;
        foreach (string category in settings.Categories)
        {
            foreach (ISetting setting in settings.CategoryToSettings[category])
            {
                if (setting.DisplayOptions != null)
                {
                    Func<ISetting> getSetting = () => SettingManager.Instance.Settings.GetSetting(setting.Name);
                    m_settingPanels.Add(UIHelper.Create(m_settingPrefab, m_settingsContent).Init(getSetting));
                }
            }
        }

        Navigation explicitNav = new Navigation();
        explicitNav.mode = Navigation.Mode.Explicit;

        Navigation bottomNav = explicitNav;
        bottomNav.selectOnDown = m_settingsApplyButton;

        Selectable lastSetting = UIHelper.SetNavigationVertical(m_settingsContent, explicitNav, explicitNav, bottomNav).LastOrDefault();
        Navigation tempNav;

        tempNav = m_settingsApplyButton.navigation;
        tempNav.selectOnUp = lastSetting;
        m_settingsApplyButton.navigation = tempNav;
    }

    private void UseDefaultSettings()
    {
        SettingManager.Instance.UseDefaults();
        SettingManager.Instance.Save();
        SettingManager.Instance.Apply();
        RefreshSettings();
    }

    private void ApplySettings()
    {
        m_settingPanels.ForEach(p => p.Apply());
        SettingManager.Instance.Save();
        SettingManager.Instance.Apply();
        RefreshSettings();
    }

    private void RefreshSettings()
    {
        m_settingPanels.ForEach(p => p.GetValue());
    }

    private float GetFadeFactor()
    {
        float fac = 1 - Mathf.Clamp01((Time.time - m_menuLoadTime) / m_fadeInDuration);
        if (m_loading != null)
        {
            fac = Mathf.Lerp(fac, 1, Mathf.Clamp01((Time.unscaledTime - m_loadFadeTime) / m_fadeOutDuration));
        }
        return Mathf.Sin((Mathf.PI / 2) * Mathf.Pow(fac, m_fadePower));
    }

    private void LaunchRace()
    {
        LevelConfig levelConfig = m_levelConfigs[m_selectedLevel];

        List<int> playerIndicies = new List<int>();
        for (int i = 0; i < 4; i++)
        {
            if (m_playerSelectPanels[i].IsReady)
            {
                playerIndicies.Add(i);
            }
        }
        
        List<PlayerConfig> playerConfigs = m_playerSelectPanels.Where(p => p.IsReady)
            .Select(p => m_playerConfigs[p.SelectedConfig]).ToList();

        RaceParameters raceParams = new RaceParameters(levelConfig, m_lapCount, playerIndicies, playerConfigs);

        m_loading = Main.Instance.LoadRace(raceParams);
        m_loadFadeTime = Time.unscaledTime;
        SetMenu(Menu.Loading);
    }
}
