using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InRaceMenu : MonoBehaviour
{
    [SerializeField] private AudioClip m_quit;
    [SerializeField] private AudioClip m_pause;
    [SerializeField] private AudioClip m_resume;
    [SerializeField] private AudioClip m_confirm;
    [SerializeField] private AudioClip m_cancel;
    
    [SerializeField]
    private Canvas m_playerUIParent;
    
    [Header("Fade")]
    [SerializeField]
    private Image m_fade;
    [SerializeField] [Range(0, 10)]
    private float m_finishWait = 1;
    [SerializeField] [Range(0, 5)]
    private float m_finishFadeInTime = 0.5f;

    [Header("Menu")]
    [SerializeField] private Canvas m_menu;
    [SerializeField] private Text m_title;
    [SerializeField] private ControlPanel m_menuControls1;
    [SerializeField] private ControlPanel m_menuControls2;
    
    private List<PlayerUI> m_playerUIs = new List<PlayerUI>();
    private CanvasGroup m_menuGroup;
    private List<KeyCode> m_pauseKeys;
    private List<KeyCode> m_resumeKeys;
    private List<KeyCode> m_confirmKeys;
    private List<KeyCode> m_cancelKeys;
    private List<KeyCode> m_quitKeys;
    private float m_finishTime;
    private bool m_confirmAction = false;

    private void Awake()
    {
        m_menuGroup = m_menu.GetComponent<CanvasGroup>();

        m_pauseKeys = new List<KeyCode>() { KeyCode.Escape, };
        m_resumeKeys = new List<KeyCode>() { KeyCode.Escape, };
        m_quitKeys = new List<KeyCode>() { KeyCode.Q, };
        m_confirmKeys = new List<KeyCode>() { KeyCode.Return, };
        m_cancelKeys = new List<KeyCode>() { KeyCode.Escape, };

        m_menu.enabled = false;
        m_menuGroup.alpha = 1;

        m_fade.enabled = true;
        m_fade.color = new Color(0, 0, 0, 1);
    }

    public InRaceMenu Init(int playerCount)
    {
        CanvasScaler scaler = m_playerUIParent.GetComponent<CanvasScaler>();

        Rect splitscreen = CameraManager.GetSplitscreen(0, playerCount);
        scaler.referenceResolution = new Vector2(scaler.referenceResolution.x / splitscreen.width, scaler.referenceResolution.y / splitscreen.height);

        return this;
    }

    public void AddPlayerUI(PlayerUI playerUI)
    {
        playerUI.transform.SetParent(m_playerUIParent.transform, false);
        m_playerUIs.Add(playerUI);
    }

    public void UpdateUI(RaceManager raceManager, bool isPaused, bool isFinished, bool isQuitting)
    {
        foreach (PlayerUI ui in m_playerUIs)
        {
            ui.UpdateUI();
        }

        m_menu.enabled = (isPaused || isFinished) && !isQuitting;

        if (isFinished)
        {
            m_menuGroup.alpha = Mathf.Clamp01((Time.unscaledTime - (m_finishTime + m_finishWait)) / m_finishFadeInTime);
        }

        m_fade.color = new Color(0, 0, 0, raceManager.FadeFac);

        bool resume = m_resumeKeys.Any(k => Input.GetKeyDown(k));
        bool pause = m_pauseKeys.Any(k => Input.GetKeyDown(k));
        bool quit = m_quitKeys.Any(k => Input.GetKeyDown(k));
        bool confirm = m_confirmKeys.Any(k => Input.GetKeyDown(k));
        bool cancel = m_cancelKeys.Any(k => Input.GetKeyDown(k));

        if (isFinished && quit)
        {
            Quit(raceManager);
        }

        if (!isFinished)
        {
            if (isPaused)
            {
                if (!m_confirmAction)
                {
                    if (resume)
                    {
                        raceManager.Resume();
                    }
                    else if (quit)
                    {
                        m_confirmAction = true;
                        AudioManager.Instance.PlaySound(m_confirm);
                    }
                }
                else
                {
                    if (confirm)
                    {
                        Quit(raceManager);
                    }
                    else if (cancel)
                    {
                        m_confirmAction = false;
                        AudioManager.Instance.PlaySound(m_cancel);
                    }
                }
            }
            else if (pause)
            {
                raceManager.Pause();
            }
        }

        m_menuControls1.SetActive(isPaused);
        if (isPaused)
        {
            if (!m_confirmAction)
            {
                m_title.text = "Paused";
                m_menuControls1.UpdateUI("Resume", m_resumeKeys.Select(k => PlayerInput.GetName(k)).ToList());
                m_menuControls2.UpdateUI("End Race", m_quitKeys.Select(k => PlayerInput.GetName(k)).ToList());
            }
            else
            {
                m_title.text = "Quit?";
                m_menuControls1.UpdateUI("Cancel", m_cancelKeys.Select(k => PlayerInput.GetName(k)).ToList());
                m_menuControls2.UpdateUI("OK", m_confirmKeys.Select(k => PlayerInput.GetName(k)).ToList());
            }
        }
        else if (isFinished)
        {
            m_title.text = "Race Over";
            m_menuControls2.UpdateUI("End Race", m_quitKeys.Select(k => PlayerInput.GetName(k)).ToList());
        }
    }

    private void Quit(RaceManager raceManager)
    {
        AudioManager.Instance.PlaySound(m_quit);
        raceManager.Quit();
    }

    public void OnPause()
    {
        AudioManager.Instance.PlaySound(m_pause);
    }

    public void OnResume()
    {
        AudioManager.Instance.PlaySound(m_resume);
    }

    public void OnFinish()
    {
        m_finishTime = Time.unscaledTime;
    }
}