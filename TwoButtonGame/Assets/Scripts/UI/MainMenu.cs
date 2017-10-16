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
    [SerializeField] private Image m_levelPreview;
    [SerializeField] private Image m_levelHighlight;
    [SerializeField] private PlayerConfig[] m_levelConfigs;

    private enum Menu
    {
        None,
        Root,
        PlayerSelect,
        LevelSelect,
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
        if (m_activeMenu != Menu.None)
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

        if (menu != m_activeMenu)
        {
            m_activeMenu = menu;

            m_rootMenu.enabled = (menu == Menu.Root);
            m_playerSelectMenu.enabled = (menu == Menu.PlayerSelect);

            switch (menu)
            {
                case Menu.PlayerSelect: m_playerSelectPanels.ForEach(p => p.ResetState()); break;
            }

            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void Update()
    {
        bool useCursor = Input.GetKey(KeyCode.LeftControl);
        Cursor.lockState = true ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = useCursor;
        
        if (Input.GetButtonDown("Cancel"))
        {
            Menu menu = m_activeMenu;

            switch (m_activeMenu)
            {
                case Menu.Root: SetMenu(Menu.Root); break;
            }

            if (menu != m_activeMenu)
            {
                PlayCancelSound();
            }
        }

        // ensure there is always something selected so that controllers can always be used
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
        }
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
}
