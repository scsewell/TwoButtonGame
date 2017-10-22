using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Countdown")]
    [SerializeField]
    private Text m_countdownText;

    [Header("Timer")]
    [SerializeField]
    private Text m_timerText;

    [Header("Player")]
    [SerializeField]
    private Text m_playerText;

    [Header("Waypoint Arrow")]
    [SerializeField]
    private Transform m_arrowPrefab;
    [SerializeField] [Range(0, 1)]
    private float m_arrowPosition = 0.85f;
    [SerializeField] [Range(0, 1)]
    private float m_arrowScale = 0.1f;
    [SerializeField] [Range(0, 1)]
    private float m_arrowSmoothing = 0.2f;

    [Header("Rank Text")]
    [SerializeField]
    private RectTransform m_rankRect;
    [SerializeField]
    private Text m_rankText;
    [SerializeField]
    private Text m_rankSubText;
    [SerializeField]
    private Text m_finalRankText;
    [SerializeField]
    private Text m_finalRankSubText;
    [SerializeField]
    private Color[] m_rankColors;
    [SerializeField] [Range(0.01f, 4)]
    private float m_finalRankFadeTime = 1.0f;

    [Header("Lap Text")]
    [SerializeField]
    private Text m_lapText;
    [SerializeField]
    private Text m_newLapText;
    [SerializeField] [Range(0, 10)]
    private float m_newLapDuration = 2.5f;
    [SerializeField] [Range(0, 0.5f)]
    private float m_newLapSizeMagnitude = 0.5f;
    [SerializeField] [Range(1, 8)]
    private float m_newLapSizeFrequency = 4.0f;
    [SerializeField] [Range(0.01f, 1)]
    private float m_newLapAlphaSmoothing = 0.25f;

    private Player m_player;
    private CameraManager m_camera;
    private RaceManager m_raceManager;
    private Transform m_arrow;
    private int m_lastRank = 1;
    private int m_lastLap = 1;
    private float m_lapChangeTime = float.NegativeInfinity;

    private void Awake()
    {
        m_arrow = Instantiate(m_arrowPrefab);

        m_raceManager = Main.Instance.RaceManager;

        SetAlpha(m_newLapText, 0);
    }

    public PlayerUI Init(Player player, CameraManager cam, int playerCount)
    {
        m_player = player;
        m_camera = cam;

        int baseLayer = 8;
        int uiLayer = player.PlayerNum + baseLayer;
        
        gameObject.GetComponentsInChildren<Transform>().ToList().ForEach(t => t.gameObject.layer = uiLayer);
        m_arrow.GetComponentsInChildren<Transform>().ToList().ForEach(t => t.gameObject.layer = uiLayer);

        cam.Camera.cullingMask |= (1 << uiLayer);

        RectTransform rt = GetComponent<RectTransform>();
        Rect splitscreen = CameraManager.GetSplitscreen(player.PlayerNum, playerCount);

        rt.localScale = Vector3.one;
        rt.anchorMin = new Vector2(splitscreen.x, splitscreen.y);
        rt.anchorMax = new Vector2(splitscreen.x + splitscreen.width, splitscreen.y + splitscreen.height);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;

        // Set up UI
        m_playerText.text = "Player " + (player.PlayerNum + 1);
        m_playerText.color = Consts.PLAYER_COLORS[player.PlayerNum];

        return this;
    }

    private void OnDestroy()
    {
        if (m_arrow != null)
        {
            Destroy(m_arrow);
            m_arrow = null;
        }
    }

    public void UpdateUI()
    {
        // start countdown
        float countdown = m_raceManager.CountdownTime;
        m_countdownText.gameObject.SetActive(-1 < countdown && countdown <= 3);

        if (m_countdownText.isActiveAndEnabled)
        {
            m_countdownText.text = (countdown > 0) ? Mathf.CeilToInt(countdown).ToString() : "GO!";
        }

        // timer
        float time = m_raceManager.GetTimeSinceStart(m_player.IsFinished ? m_player.FinishTime : Time.time);
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time - (minutes * 60));
        int milliseconds = Mathf.FloorToInt((time - seconds - (minutes * 60)) * 100);

        m_timerText.text = string.Format(minutes.ToString() + ":" + seconds.ToString("D2") + ":" + milliseconds.ToString("D2"));
        
        int rank = m_raceManager.GetPlayerRank(m_player);
        bool isSolo = m_raceManager.PlayerCount == 1;

        RacePath path = m_raceManager.RacePath;
        Waypoint waypoint = m_player.CurrentWaypoint;
        int lap = path.GetCurrentLap(m_player.WaypointsCompleted);
        bool finished = m_player.IsFinished;
        
        m_arrow.gameObject.SetActive(!finished && waypoint != null);
        m_rankText.gameObject.SetActive(!finished && !isSolo);
        m_rankSubText.gameObject.SetActive(!finished && !isSolo);
        m_newLapText.gameObject.SetActive(!finished);

        m_finalRankText.gameObject.SetActive(finished && !isSolo);
        m_finalRankSubText.gameObject.SetActive(finished && !isSolo);

        if (m_arrow.gameObject.activeInHierarchy)
        {
            Vector3 arrowPos = m_camera.Camera.ViewportToWorldPoint(new Vector3(0.5f, m_arrowPosition, 1));
            Quaternion arrowRot = Quaternion.LookRotation(waypoint.Position - m_player.transform.position);

            m_arrow.position = arrowPos;
            m_arrow.rotation = m_arrowSmoothing > 0 ? Quaternion.Slerp(m_arrow.rotation, arrowRot, Time.deltaTime / m_arrowSmoothing) : arrowRot;
            m_arrow.localScale = m_arrowScale * Vector3.one;
        }

        // set rank text
        Color rankColor = m_rankColors[rank - 1];
        SetColorNoAlpha(m_rankText, rankColor);
        SetColorNoAlpha(m_rankSubText, rankColor);
        SetColorNoAlpha(m_finalRankText, rankColor);
        SetColorNoAlpha(m_finalRankSubText, rankColor);
        
        SetAlpha(m_finalRankText, finished ? Mathf.MoveTowards(m_finalRankText.color.a, 1, Time.deltaTime / m_finalRankFadeTime) : 0);
        SetAlpha(m_finalRankSubText, m_finalRankText.color.a);

        m_rankText.text = rank.ToString();
        m_finalRankText.text = rank.ToString();

        string subText = "th";
        switch (rank)
        {
            case 1: subText = "st"; break;
            case 2: subText = "nd"; break;
            case 3: subText = "rd"; break;
        }

        m_rankSubText.text = subText;
        m_finalRankSubText.text = subText;

        if (rank != m_lastRank && m_raceManager.GetTimeSinceStart(Time.time) > 0)
        {
            m_rankRect.localScale = 1.2f * Vector3.one;
        }
        else
        {
            m_rankRect.localScale = Vector3.Lerp(m_rankRect.localScale, Vector3.one, Time.deltaTime * 3);
        }

        // set lap text
        m_lapText.text = "LAP " + lap + "/" + path.Laps;

        RectTransform newLapRT = m_newLapText.GetComponent<RectTransform>();
        if (lap != m_lastLap)
        {
            newLapRT.localScale = 1.2f * Vector3.one;
            SetAlpha(m_newLapText, 0);

            int remainingLaps = path.Laps - (lap - 1);
            switch (remainingLaps)
            {
                case 1: m_newLapText.text = "Final Lap"; break;
                default: m_newLapText.text = remainingLaps + " Laps Left"; break;
            }

            m_lapChangeTime = Time.time;
        }
        else
        {
            float targetScale = 1 + (m_newLapSizeMagnitude * (0.5f * Mathf.Cos(Time.time * m_newLapSizeFrequency) + 0.5f));
            newLapRT.localScale = Vector3.Lerp(newLapRT.localScale, targetScale * Vector3.one, Time.deltaTime * 4);

            bool activated = Time.time - m_lapChangeTime < m_newLapDuration;

            SetAlpha(m_newLapText, Mathf.Lerp(m_newLapText.color.a, activated ? 1 : 0, Time.deltaTime / m_newLapAlphaSmoothing));
        }

        m_lastRank = rank;
        m_lastLap = lap;
    }

    private void SetColorNoAlpha(Graphic g, Color c)
    {
        g.color = new Color(c.r, c.g, c.b, g.color.a);
    }

    private void SetAlpha(Graphic g, float alpha)
    {
        Color col = g.color;
        col.a = alpha;
        g.color = col;
    }
}
