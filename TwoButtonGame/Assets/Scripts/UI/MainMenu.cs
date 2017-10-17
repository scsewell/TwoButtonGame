using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : Menu
{
    [Header("Root Menu")]
    [SerializeField] private Canvas m_rootMenu;
    [SerializeField] private Button m_playButton;
    [SerializeField] private Button m_showHowToButton;
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
    [SerializeField] private Image m_levelPreview;
    [SerializeField] private Image m_levelHighlight;
    [SerializeField] private ControlPanel m_levelControls1;
    [SerializeField] private ControlPanel m_levelControls2;
    [SerializeField] private ControlPanel m_levelControls3;
    [SerializeField] private LevelConfig[] m_levelConfigs;

    private int m_selectedLevel = 0;
    
    [Header("How To Play")]
    [SerializeField] private Canvas m_howToMenu;
    [SerializeField] private Button m_howToBackButton;
    [SerializeField] private GameObject m_howToPreviousPanel;
    [SerializeField] private Button m_howToPreviousButton;
    [SerializeField] private GameObject m_howToNextPanel;
    [SerializeField] private Button m_howToNextButton;

    private int m_currentHowTo = 0;

    [Header("Credits")]
    [SerializeField] private Canvas m_creditsMenu;
    [SerializeField] private Button m_creditsBackButton;

    [Header("Loading")]
    [SerializeField]
    private Image m_fade;
    [SerializeField] [Range(0, 5)]
    private float m_loadFadeDuration = 1.0f;
    [SerializeField] [Range(0, 5)]
    private float m_fadePower = 3.0f;

    private AsyncOperation m_loading;
    private float m_loadFadeTime = float.MaxValue;

    private enum Menu
    {
        None,
        Root,
        PlayerSelect,
        LevelSelect,
        HowToPlay,
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
        m_showCreditsButton.onClick.AddListener(() => SetMenu(Menu.Credits));
        m_quitButton.onClick.AddListener(() => Application.Quit());
        
        m_howToBackButton.onClick.AddListener(() => SetMenu(Menu.Root));
        m_howToPreviousButton.onClick.AddListener(() => m_currentHowTo = Mathf.Max(m_currentHowTo - 1, 0));
        m_howToNextButton.onClick.AddListener(() => m_currentHowTo = Mathf.Min(m_currentHowTo + 1, 2));

        m_creditsBackButton.onClick.AddListener(() => SetMenu(Menu.Root));

        for (int i = 0; i < 4; i++)
        {
            m_playerSelectPanels.Add(Instantiate(m_playerSelectPrefab, m_playerSelectContent));
        }

        m_bannerCol = m_continueBanner.color;

        RaceParameters previousParams = Main.Instance.LastRaceParams;
        if (previousParams != null)
        {
            m_selectedLevel = System.Array.IndexOf(m_levelConfigs, previousParams.LevelConfig);

            for (int i = 0; i < previousParams.PlayerIndicies.Count; i++)
            {
                m_playerSelectPanels[previousParams.PlayerIndicies[i]].FromConfig(m_playerConfigs, previousParams.PlayerConfigs[i]);
            }

            SetMenu(Menu.PlayerSelect);
        }
        else
        {
            SetMenu(Menu.Root);
        }
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
            m_howToMenu.enabled         = (menu == Menu.HowToPlay);
            m_creditsMenu.enabled       = (menu == Menu.Credits);

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
                case Menu.HowToPlay: ResetHowToPlay(); break;
            }

            EventSystem.current.SetSelectedGameObject(null);
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
                case Menu.Credits: m_creditsBackButton.Select(); break;
            }
        }

        switch (m_activeMenu)
        {
            case Menu.PlayerSelect: UpdatePlayerSelect(); break;
            case Menu.LevelSelect: UpdateLevelSelect(); break;
            case Menu.HowToPlay: UpdateHowToPlay(); break;
            case Menu.Loading: UpdateLoading(); break;
        }

        AudioListener.volume = Mathf.MoveTowards(AudioListener.volume, 1 - GetFadeFactor(), Time.unscaledDeltaTime / 0.35f);
    }

    private void ResetPlayerSelect(bool fullReset)
    {
        m_playerSelectPanels.ForEach(p => p.ResetState(fullReset));
        m_continueBar.SetActive(false);
    }

    private void UpdatePlayerSelect()
    {
        int players = m_playerSelectPanels.Count(p => p.IsJoined);

        int playerNum = 0;
        for (int i = 0; i < 4; i++)
        {
            PlayerInput input = InputManager.Instance.PlayerInputs[i];
            
            bool back = m_playerSelectPanels[i].UpdatePanel(playerNum, input, m_playerConfigs, this);

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
            LevelConfig levelConfig = m_levelConfigs[m_selectedLevel];

            List<int> playerIndicies = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                if (m_playerSelectPanels[i].IsReady)
                {
                    playerIndicies.Add(i);
                }
            }

            List<PlayerConfig> playerConfigs = readyPlayers.Select(p => m_playerConfigs[p.SelectedConfig]).ToList();

            RaceParameters raceParams = new RaceParameters(levelConfig, playerIndicies, playerConfigs);

            m_loading = Main.Instance.LoadRace(raceParams);
            m_loadFadeTime = Time.unscaledTime;
            SetMenu(Menu.Loading);
        }
        else if (input.Button2Up)
        {
            m_selectedLevel = (m_selectedLevel = 1) % m_levelConfigs.Length;
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
        m_levelPreview.sprite = config.Preview;
        m_levelHighlight.color = new Color(1, 1, 1, Mathf.Lerp(m_levelHighlight.color.a, 0, Time.unscaledDeltaTime * 12f));

        if (m_playerSelectPanels.Any(p => p.IsReady))
        {
            PlayerInput input = m_playerSelectPanels.Where(p => p.IsReady).First().Input;
            m_levelControls1.UpdateUI("Accept", input.Button1Name);
            m_levelControls2.UpdateUI("Next", input.Button2Name);
            m_levelControls3.UpdateUI("Back", input.ButtonNames);
        }
    }

    private void UpdateLoading()
    {
        if (m_loadFadeTime == float.MaxValue && m_loading.progress >= 0.9f)
        {
            m_loadFadeTime = Time.unscaledTime;
        }

        float factor = GetFadeFactor();
        m_fade.color = new Color(0, 0, 0, factor);

        if (factor >= 1)
        {
            m_loading.allowSceneActivation = true;
        }
    }

    private float GetFadeFactor()
    {
        float baseFac = Mathf.Clamp01((Time.unscaledTime - m_loadFadeTime) / m_loadFadeDuration);
        return m_loadFadeTime != float.MaxValue ? Mathf.Sin((Mathf.PI / 2) * Mathf.Pow(baseFac, m_fadePower)) : 0;
    }

    private void ResetHowToPlay()
    {
        m_currentHowTo = 0;

        UpdateHowToPlay();
    }

    private void UpdateHowToPlay()
    {
        m_howToPreviousPanel.SetActive(m_currentHowTo > 0);

        Navigation nav = m_howToBackButton.navigation;
        nav.selectOnLeft = m_howToPreviousPanel.activeSelf ? m_howToPreviousButton : null;
        nav.selectOnRight = m_howToNextPanel.activeSelf ? m_howToNextButton : null;
        m_howToBackButton.navigation = nav;
    }
}
