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
    [SerializeField] private LevelConfig[] m_levelConfigs;

    private int m_selectedLevel = 0;
    
    [Header("Loading")]
    [SerializeField]
    private Image m_fade;
    [SerializeField] [Range(0, 5)]
    private float m_loadFadeDuration = 1.0f;

    private AsyncOperation m_loading;
    private float m_loadFadeTime = float.MaxValue;

    private enum Menu
    {
        None,
        Root,
        PlayerSelect,
        LevelSelect,
        Loading,
    }

    private Menu m_activeMenu = Menu.None;

    private void Awake()
    {
        m_playButton.onClick.AddListener(() => SetMenu(Menu.PlayerSelect));
        m_quitButton.onClick.AddListener(() => Application.Quit());

        for (int i = 0; i < 4; i++)
        {
            m_playerSelectPanels.Add(Instantiate(m_playerSelectPrefab, m_playerSelectContent));
        }

        m_bannerCol = m_continueBanner.color;
    }

    private void Start()
    {
        SetMenu(Menu.Root);
    }

    private void SetMenu(Menu menu, bool back = false)
    {
        Menu previous = m_activeMenu;

        if (previous != menu)
        {
            m_activeMenu = menu;

            if (previous != Menu.None)
            {
                if (back) {
                    PlayBackMenuSound();
                } else {
                    PlayNextMenuSound();
                }
            }

            m_rootMenu.enabled = (menu == Menu.Root);
            m_playerSelectMenu.enabled = (menu == Menu.PlayerSelect);
            m_levelSelectMenu.enabled = (menu == Menu.LevelSelect);

            switch (m_activeMenu)
            {
                case Menu.PlayerSelect: ResetPlayerSelect(previous != Menu.LevelSelect); break;
                case Menu.LevelSelect: ResetLevelSelect(); break;
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
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            switch (m_activeMenu)
            {
                case Menu.Root: m_playButton.Select(); break;
            }
        }

        switch (m_activeMenu)
        {
            case Menu.PlayerSelect: UpdatePlayerSelect(); break;
            case Menu.LevelSelect: UpdateLevelSelect(); break;
            case Menu.Loading: UpdateLoading(); break;
        }
    }

    private void ResetPlayerSelect(bool fullReset)
    {
        m_playerSelectPanels.ForEach(p => p.ResetState(fullReset));
        m_continueBar.SetActive(false);
    }

    private void ResetLevelSelect()
    {
        m_selectedLevel = 0;
        m_levelHighlight.color = new Color(1, 1, 1, 0);
        UpdateLevelSelectGraphics();
    }

    private void UpdatePlayerSelect()
    {
        int players = m_playerSelectPanels.Count(p => p.IsJoined);

        int playerNum = 0;
        List<string> continueButtons = new List<string>();

        for (int i = 0; i < 4; i++)
        {
            PlayerInput input = InputManager.Instance.PlayerInputs[i];
            
            bool back = m_playerSelectPanels[i].UpdatePanel(playerNum, input, m_playerConfigs, this);

            if (m_playerSelectPanels[i].IsJoined)
            {
                playerNum++;
            }
            if (m_playerSelectPanels[i].IsReady)
            {
                continueButtons.Add(input.Button1Name);
            }
            if (back)
            {
                SetMenu(Menu.Root, true);
            }
        }

        m_continueControls.UpdateUI("Continue", continueButtons);

        bool canContinue = m_playerSelectPanels.All(p => p.CanContinue) && m_playerSelectPanels.Count(p => p.IsReady) > 0;
        if (m_canContine != canContinue)
        {
            m_continueTime = Time.time;
        }
        m_canContine = canContinue;

        m_continueBar.SetActive(canContinue);
        m_continueBanner.color = Color.Lerp(Color.white, m_bannerCol, (Time.time - m_continueTime) / 0.5f);
        
        if (m_playerSelectPanels.Any(p => p.Continue) && canContinue)
        {
            SetMenu(Menu.LevelSelect);
        }
    }

    private void UpdateLevelSelect()
    {
        PlayerInput[] inputs = InputManager.Instance.PlayerInputs;

        if (inputs.Any(i => i.Button1Up))
        {
            List<PlayerSelectPanel> readyPlayers = m_playerSelectPanels.Where(p => p.IsReady).ToList();

            List<PlayerConfig> playerConfigs = readyPlayers.Select(p => m_playerConfigs[p.SelectedConfig]).ToList();
            List<PlayerInput> playerInputs = readyPlayers.Select(p => p.Input).ToList();
            
            m_loading = Main.Instance.LoadRace(m_levelConfigs[m_selectedLevel], playerConfigs, playerInputs);
            m_loadFadeTime = Time.time;
            SetMenu(Menu.Loading);
        }
        else if (inputs.Any(i => i.Button2Up))
        {
            m_selectedLevel = (m_selectedLevel = 1) % m_levelConfigs.Length;
            m_levelHighlight.color = new Color(1, 1, 1, 0.5f);
            PlaySelectSound();
        }
        else if (inputs.Any(i => i.BothDown))
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
        m_levelHighlight.color = new Color(1, 1, 1, Mathf.Lerp(m_levelHighlight.color.a, 0, Time.deltaTime * 10f));
    }

    private void UpdateLoading()
    {
        if (m_loadFadeTime == float.MaxValue && m_loading.progress >= 0.9f)
        {
            m_loadFadeTime = Time.time;
        }

        float factor = (Time.time - m_loadFadeTime) / m_loadFadeDuration;
        m_fade.color = new Color(0, 0, 0, Mathf.Clamp01(factor));

        if (factor > 1)
        {
            m_loading.allowSceneActivation = true;
        }
    }
}
