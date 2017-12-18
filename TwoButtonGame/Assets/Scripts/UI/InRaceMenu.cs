using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Framework.UI;

public class InRaceMenu : MenuBase
{
    [Header("Audio")]
    [SerializeField] [Range(0, 1)]
    private float m_quitVolume = 1.0f;
    [SerializeField] [Range(0, 1)]
    private float m_pauseVolume = 1.0f;
    [SerializeField] [Range(0, 1)]
    private float m_resumeVolume = 1.0f;

    [Header("Intro Panel")]
    [SerializeField] private Canvas m_titleCardMenu;
    [SerializeField] private Text m_trackName;
    [SerializeField] private Text m_songName;
    [SerializeField] private ControlPanel m_skipControls;
    [SerializeField] private CanvasGroup m_cardUpper;
    [SerializeField] private CanvasGroup m_cardLower;
    [SerializeField] private CanvasGroup m_cardBottom;

    [SerializeField] [Range(0, 5)]
    private float m_slideInWait = 1.0f;
    [SerializeField] [Range(0, 10)]
    private float m_titleHoldTime = 5.0f;
    [SerializeField] [Range(0, 5)]
    private float m_slideDutation = 0.5f;
    
    private float m_introSkipTime;

    [Header("Menu")]
    [SerializeField] private Canvas m_menu;
    [SerializeField] private Text m_title;
    [SerializeField] private RectTransform m_mainButtons;
    [SerializeField] private Button m_resumeButton;
    [SerializeField] private Button m_restartButton;
    [SerializeField] private Button m_quitButton;
    [SerializeField] private ControlPanel m_menuControls1;
    [SerializeField] private ControlPanel m_menuControls2;
    [SerializeField] [Range(0, 10)]
    private float m_finishWait = 1;
    [SerializeField] [Range(0, 5)]
    private float m_finishFadeInTime = 0.5f;

    private CanvasGroup m_menuGroup;

    [Header("Other")]
    [SerializeField]
    private Canvas m_playerUIParent;
    [SerializeField]
    private Image m_fade;

    private List<PlayerUI> m_playerUIs;
    private List<PlayerBaseInput> m_inputs;
    private float m_finishTime;

    private void Awake()
    {
        m_menuGroup = m_menu.GetComponentInChildren<CanvasGroup>();

        m_resumeButton.onClick.AddListener(() => Main.Instance.RaceManager.Resume());
        m_restartButton.onClick.AddListener(() => Main.Instance.RaceManager.ResetRace(0));
        m_quitButton.onClick.AddListener(() => Quit());

        UIHelper.SetNavigationVertical(m_mainButtons, null, null, null, null, true);

        m_menu.enabled = false;
        m_menuGroup.alpha = 1;

        m_fade.enabled = true;
        m_fade.color = new Color(0, 0, 0, 1);

        m_playerUIs = new List<PlayerUI>();
    }

    public InRaceMenu Init(RaceParameters raceParameters)
    {
        m_inputs = raceParameters.Inputs;

        CanvasScaler scaler = m_playerUIParent.GetComponent<CanvasScaler>();

        Rect splitscreen = CameraManager.GetSplitscreen(0, raceParameters.HumanCount);
        scaler.referenceResolution = new Vector2(scaler.referenceResolution.x / splitscreen.width, scaler.referenceResolution.y / splitscreen.height);

        LevelConfig config = raceParameters.LevelConfig;
        m_trackName.text = config.Name;
        m_songName.text = "\"" + config.Music.Name + "\" - " + config.Music.Artist;

        gameObject.AddComponent<CustomInputModule>();
        CustomInput customInput = gameObject.GetComponent<CustomInput>();
        customInput.SetInputs(m_inputs);

        return this;
    }

    public void AddPlayerUI(PlayerUI playerUI)
    {
        playerUI.transform.SetParent(m_playerUIParent.transform, false);
        m_playerUIs.Add(playerUI);
    }

    public void UpdateUI(RaceManager raceManager, bool showPlayerUI, bool isPaused, bool isFinished, bool isQuitting, float fade)
    {
        m_fade.color = new Color(0, 0, 0, fade);

        m_playerUIParent.enabled = showPlayerUI;
        if (m_playerUIParent.enabled)
        {
            foreach (PlayerUI ui in m_playerUIs)
            {
                ui.UpdateUI();
            }
        }

        bool menuButton = m_inputs.Any(i => i.UI_Menu) || Input.GetKeyDown(KeyCode.Escape);
        
        if (!isFinished && menuButton)
        {
            if (isPaused)
            {
                raceManager.Resume();
            }
            else
            {
                raceManager.Pause();
            }
        }

        m_menu.enabled = (isPaused || isFinished) && !isQuitting;

        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null)
        {
            selected = m_menu.enabled ? m_resumeButton.gameObject : null;
            EventSystem.current.SetSelectedGameObject(selected);
        }

        float menuAlpha = 1;
        if (isFinished)
        {
            menuAlpha = Mathf.Clamp01((Time.unscaledTime - (m_finishTime + m_finishWait)) / m_finishFadeInTime);
        }
        m_menuGroup.alpha = menuAlpha * (isPaused ? 1 : (1 - fade));


        m_titleCardMenu.enabled = Time.time <= raceManager.TimeIntroEnd;
        if (m_titleCardMenu.enabled)
        {
            float inTime = raceManager.TimeRaceLoad + m_slideInWait;
            float outTime = Mathf.Min(inTime + m_titleHoldTime, raceManager.TimeIntroSkip);

            SetIntroCardPos(m_cardUpper, inTime, outTime, 0);
            SetIntroCardPos(m_cardLower, inTime, outTime, 0.1f);
            SetIntroCardPos(m_cardBottom, inTime, Mathf.Min(float.MaxValue, raceManager.TimeIntroSkip), 0.3f);

            m_skipControls.UpdateUI("Skip", m_inputs.SelectMany(i => i.SpriteAccept).ToList());

            if (!m_menu.enabled)
            {
                foreach (PlayerBaseInput input in m_inputs)
                {
                    if (input.UI_Accept)
                    {
                        raceManager.SkipIntro();
                        PlayNextMenuSound();
                    }
                }
            }
        }

        FlushSoundQueue();
    }

    private void SetIntroCardPos(CanvasGroup card, float inTime, float outTime, float timeOffset)
    {
        float time = Mathf.Clamp01((Time.time - (inTime + timeOffset)) / m_slideDutation) - Mathf.Clamp01((Time.time - (outTime + timeOffset)) / m_slideDutation);
        float fac = (0.5f * Mathf.Cos(time * Mathf.PI)) + 0.5f;

        RectTransform rt = card.GetComponent<RectTransform>();
        float x = -(rt.rect.width + 300) * fac;
        rt.anchoredPosition = new Vector2(x, rt.anchoredPosition.y);

        card.alpha = 1 - fac;
    }
    
    private void Quit()
    {
        Main.Instance.RaceManager.Quit();
        PlayNextMenuSound(m_quitVolume);
    }

    public void OnPause()
    {
        PlayOpenMenuSound(m_pauseVolume);
    }

    public void OnResume()
    {
        PlayBackMenuSound(m_resumeVolume);
    }

    public void OnFinish()
    {
        m_finishTime = Time.unscaledTime;
    }
}