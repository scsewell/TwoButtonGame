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
    private int m_playerConfig;
    private bool m_continue;

    public bool IsJoined { get { return m_state != State.Join; } }
    public bool IsReady { get { return m_state == State.Ready; } }
    public bool CanContinue { get { return m_state == State.Ready || m_state == State.Join; } }
    public bool Continue { get { return m_continue; } }

    public void Awake()
    {
        ResetState();
    }

    public void ResetState()
    {
        m_state = State.Join;
        m_playerConfig = 0;
    }

    public bool UpdatePanel(int playerNum, PlayerInput input, PlayerConfig[] configs, Menu menu)
    {
        m_continue = false;
        if (input.Button1Up)
        {
            switch (m_state)
            {
                case State.Join: m_state = State.Select; menu.PlaySubmitSound(); break;
                case State.Select: m_state = State.Ready; menu.PlaySubmitSound(); break;
                case State.Ready: m_continue = true; break;
            }
        }

        if (input.Button2Up)
        {
            switch (m_state)
            {
                case State.Select:
                    m_playerConfig = (m_playerConfig = 1) % configs.Length;
                    m_characterHighlight.color = new Color(1, 1, 1, 0.7f);
                    menu.PlaySelectSound();
                    break;
            }
        }

        PlayerConfig config = configs[m_playerConfig];
        m_characterPreview.sprite = config.Preview;
        m_characterHighlight.color = new Color(1, 1, 1, Mathf.Lerp(m_characterHighlight.color.a, 0, Time.deltaTime * 8f));

        bool back = false;
        if (input.BothDown)
        {
            switch (m_state)
            {
                case State.Join: back = true; break;
                case State.Select: m_state = State.Join; menu.PlayCancelSound(); break;
                case State.Ready: m_state = State.Select; menu.PlayCancelSound(); break;
            }
        }

        m_joinTab.SetActive(m_state == State.Join);
        m_characterSelectTab.SetActive(m_state != State.Join);
        m_playerName.gameObject.SetActive(m_state != State.Join);
        m_readyText.gameObject.SetActive(m_state == State.Ready);

        m_joinControls.UpdateUI("Join", input.Button1Name);

        switch (m_state)
        {
            case State.Join:
                m_controls1.SetActive(true);
                m_controls1.UpdateUI("Back", input.ButtonNames);
                m_controls2.SetActive(false);
                m_controls3.SetActive(false);
                break;
            case State.Select:
                m_controls1.SetActive(true);
                m_controls1.UpdateUI("Leave", input.ButtonNames);
                m_controls2.SetActive(true);
                m_controls2.UpdateUI("Next", input.Button2Name);
                m_controls3.SetActive(true);
                m_controls3.UpdateUI("Ready", input.Button1Name);
                break;
            case State.Ready:
                m_controls1.SetActive(true);
                m_controls1.UpdateUI("Cancel", input.ButtonNames);
                m_controls2.SetActive(false);
                m_controls3.SetActive(false);
                break;
        }

        m_playerName.text = "Player " + (playerNum + 1);
        m_playerName.color = Consts.PLAYER_COLORS[playerNum];

        return back;
    }
}
