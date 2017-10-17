using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectPanel : MonoBehaviour
{
    [SerializeField] private GameObject m_joinTab;
    [SerializeField] private GameObject m_characterSelectTab;
    [SerializeField] private Text m_playerName;
    [SerializeField] private Text m_readyText;
    [SerializeField] private ControlPanel m_joinControls;
    [SerializeField] private ControlPanel m_controls1;
    [SerializeField] private ControlPanel m_controls2;
    [SerializeField] private ControlPanel m_controls3;
    [SerializeField] private Image m_characterPreview;
    [SerializeField] private Image m_characterHighlight;

    public enum State
    {
        Join,
        Select,
        Ready,
    }
    
    private State m_state;
    private bool m_continue;

    private int m_selectedConfig;
    public int SelectedConfig { get { return m_selectedConfig; } }

    private PlayerInput m_input;
    public PlayerInput Input { get { return m_input; } }

    public bool IsJoined { get { return m_state != State.Join; } }
    public bool IsReady { get { return m_state == State.Ready; } }
    public bool CanContinue { get { return m_state == State.Ready || m_state == State.Join; } }
    public bool Continue { get { return m_continue; } }

    public void Awake()
    {
        ResetState(true);
    }

    public void ResetState(bool fullReset)
    {
        m_state = (!fullReset && m_state != State.Join) ? State.Select : State.Join;

        if (fullReset)
        {
            m_selectedConfig = 0;
        }
        
        m_characterHighlight.color = new Color(1, 1, 1, 0);

        UpdateGraphics();
    }

    public void FromConfig(PlayerConfig[] configs, PlayerConfig selectedConfig)
    {
        m_state = State.Select;
        m_selectedConfig = System.Array.IndexOf(configs, selectedConfig);
    }

    public bool UpdatePanel(int playerNum, PlayerInput input, PlayerConfig[] configs, Menu menu)
    {
        m_input = input;

        m_continue = false;
        if (m_input.Button1Up)
        {
            switch (m_state)
            {
                case State.Join: m_state = State.Select; menu.PlaySubmitSound(); break;
                case State.Select: m_state = State.Ready; menu.PlaySubmitSound(); break;
                case State.Ready: if (playerNum == 0) { m_continue = true; } break;
            }
        }

        if (m_input.Button2Up)
        {
            switch (m_state)
            {
                case State.Select:
                    m_selectedConfig = (m_selectedConfig = 1) % configs.Length;
                    m_characterHighlight.color = new Color(1, 1, 1, 0.5f);
                    menu.PlaySelectSound();
                    break;
            }
        }

        bool back = false;
        if (m_input.BothDown)
        {
            switch (m_state)
            {
                case State.Join: back = true; break;
                case State.Select: m_state = State.Join; menu.PlayCancelSound(); break;
                case State.Ready: m_state = State.Select; menu.PlayCancelSound(); break;
            }
        }

        PlayerConfig config = configs[m_selectedConfig];
        m_characterPreview.sprite = config.Preview;
        m_characterHighlight.color = new Color(1, 1, 1, Mathf.Lerp(m_characterHighlight.color.a, 0, Time.unscaledDeltaTime * 12f));

        m_playerName.text = "Player " + (playerNum + 1);
        m_playerName.color = Consts.PLAYER_COLORS[playerNum];

        UpdateGraphics();

        return back;
    }

    private void UpdateGraphics()
    {
        m_joinTab.SetActive(m_state == State.Join);
        m_characterSelectTab.SetActive(m_state != State.Join);
        m_playerName.gameObject.SetActive(m_state != State.Join);
        m_readyText.gameObject.SetActive(m_state == State.Ready);

        if (m_input != null)
        {
            m_joinControls.UpdateUI("Join", m_input.Button1Name);

            switch (m_state)
            {
                case State.Join:
                    m_controls1.SetActive(true);
                    m_controls1.UpdateUI("Back", m_input.ButtonNames);
                    m_controls2.SetActive(false);
                    m_controls3.SetActive(false);
                    break;
                case State.Select:
                    m_controls1.SetActive(true);
                    m_controls1.UpdateUI("Leave", m_input.ButtonNames);
                    m_controls2.SetActive(true);
                    m_controls2.UpdateUI("Next", m_input.Button2Name);
                    m_controls3.SetActive(true);
                    m_controls3.UpdateUI("Ready", m_input.Button1Name);
                    break;
                case State.Ready:
                    m_controls1.SetActive(true);
                    m_controls1.UpdateUI("Cancel", m_input.ButtonNames);
                    m_controls2.SetActive(false);
                    m_controls3.SetActive(false);
                    break;
            }
        }
    }
}
