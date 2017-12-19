using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using BoostBlasters.Menus;

public class InRaceMenu : MenuBase
{
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
    [SerializeField] [Range(0, 5)]
    private float m_slideOutTime = 1.0f;
    [SerializeField] [Range(0, 5)]
    private float m_slideDutation = 0.5f;
    
    private float m_introSkipTime;

    [Header("UI Elements")]
    [SerializeField] private Canvas m_playerUIParent;
    [SerializeField] private ControlPanel m_menuControls1;
    [SerializeField] private ControlPanel m_menuControls2;
    [SerializeField] private Image m_fade;
    
    [Header("Fade")]
    [SerializeField] [Range(0, 10)]
    private float m_finishWait = 1;
    [SerializeField] [Range(0, 5)]
    private float m_finishFadeInTime = 0.5f;
    
    private GameMenuRoot m_root;
    public GameMenuRoot Root { get { return m_root; } }

    private GameMenuFinished m_finish;
    public GameMenuFinished Finish { get { return m_finish; } }

    private List<PlayerBaseInput> m_activeInputs;
    public List<PlayerBaseInput> ActiveInputs { get { return m_activeInputs; } }

    private List<PlayerUI> m_playerUIs;
    private float m_finishTime;
    
    public InRaceMenu Init(RaceParameters raceParameters)
    {
        m_activeInputs = raceParameters.Inputs;
        InitBase(InputManager.Instance.PlayerInputs.ToList());

        m_root = GetComponentInChildren<GameMenuRoot>();
        m_finish = GetComponentInChildren<GameMenuFinished>();

        m_playerUIs = new List<PlayerUI>();
        CanvasScaler scaler = m_playerUIParent.GetComponent<CanvasScaler>();
        Rect splitscreen = CameraManager.GetSplitscreen(0, raceParameters.HumanCount);
        scaler.referenceResolution = new Vector2(scaler.referenceResolution.x / splitscreen.width, scaler.referenceResolution.y / splitscreen.height);
        
        LevelConfig config = raceParameters.LevelConfig;
        m_trackName.text = config.Name;
        m_songName.text = "\"" + config.Music.Name + "\" - " + config.Music.Artist;
        
        m_fade.enabled = true;
        m_fade.color = new Color(0, 0, 0, 1);

        return this;
    }

    public void ResetUI()
    {
        m_playerUIs.ForEach(ui => ui.ResetPlayerUI());
    }

    public void AddPlayerUI(PlayerUI playerUI)
    {
        playerUI.transform.SetParent(m_playerUIParent.transform, false);
        m_playerUIs.Add(playerUI);
    }

    public void UpdateUI(bool showPlayerUI, bool isPaused, bool isFinished, bool isQuitting)
    {
        UpdateBase();

        m_playerUIParent.enabled = showPlayerUI;
        if (m_playerUIParent.enabled)
        {
            foreach (PlayerUI ui in m_playerUIs)
            {
                ui.UpdateUI();
            }
        }

        RaceManager raceManager = Main.Instance.RaceManager;

        m_titleCardMenu.enabled = Time.time <= raceManager.TimeIntroEnd;
        if (m_titleCardMenu.enabled)
        {
            float inTime = raceManager.TimeRaceLoad + m_slideInWait;
            float outTime = Mathf.Min(raceManager.TimeRaceLoad + raceManager.TimeIntroDuration - m_slideOutTime, raceManager.TimeIntroSkip);

            SetIntroCardPos(m_cardUpper, inTime, 0.0f, outTime, 0.2f);
            SetIntroCardPos(m_cardLower, inTime, 0.1f, outTime, 0.1f);
            SetIntroCardPos(m_cardBottom, inTime, 0.3f, outTime, 0.0f);

            m_skipControls.UpdateUI("Skip", m_activeInputs.SelectMany(i => i.SpriteAccept).ToList());

            if (ActiveMenu == null)
            {
                foreach (PlayerBaseInput input in Inputs)
                {
                    if (input.UI_Accept && raceManager.SkipIntro())
                    {
                        PlayNextMenuSound();
                        break;
                    }
                }
            }
        }

        if (Inputs.Any(i => i.UI_Menu) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isFinished)
            {
                if (isPaused)
                {
                    SetMenu(null, TransitionSound.Back);
                }
                else
                {
                    SetMenu(Root, TransitionSound.Open);
                }
            }
            else if (ActiveMenu == null)
            {
                SetMenu(Finish, TransitionSound.Open);
            }
        }

        if (!isFinished)
        {
            if (ActiveMenu == null && !isQuitting)
            {
                raceManager.Resume();
            }
            else
            {
                raceManager.Pause();
            }
        }

        /*
        float menuAlpha = 1;
        if (isFinished)
        {
            menuAlpha = Mathf.Clamp01((Time.unscaledTime - (m_finishTime + m_finishWait)) / m_finishFadeInTime);
        }
        m_menuGroup.alpha = menuAlpha * (isPaused ? 1 : (1 - fade));
        */
    }

    public void LateUpdateUI()
    {
        LateUpdateBase((previous) => true);
        
        m_fade.color = new Color(0, 0, 0, Main.Instance.RaceManager.GetFadeFactor(false));
    }

    private void SetIntroCardPos(CanvasGroup card, float inTime, float inOffset, float outTime, float outOffset)
    {
        float time = Mathf.Clamp01((Time.time - (inTime + inOffset)) / m_slideDutation) * (1 - Mathf.Clamp01((Time.time - (outTime + outOffset)) / m_slideDutation));
        float fac = (0.5f * Mathf.Cos(time * Mathf.PI)) + 0.5f;

        RectTransform rt = card.GetComponent<RectTransform>();
        float x = -(rt.rect.width + 300) * fac;
        rt.anchoredPosition = new Vector2(x, rt.anchoredPosition.y);

        card.alpha = 1 - fac;
    }

    public void OnFinish()
    {
        m_finishTime = Time.unscaledTime;
    }
}