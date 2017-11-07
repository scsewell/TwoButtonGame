﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Player Indicator")]
    [SerializeField]
    private RectTransform m_playerIndicatorPrefab;
    [SerializeField]
    private RectTransform m_playerIndicatorParent;
    [SerializeField] [Range(0, 2)]
    private float m_indicatorVerticalOffset = 0.85f;
    [SerializeField] [Range(0, 50)]
    private float m_indicatorNearFadeStart = 2.0f;
    [SerializeField] [Range(0, 50)]
    private float m_indicatorNearFadeEnd = 4.0f;
    [SerializeField] [Range(0, 1000)]
    private float m_indicatorFarFadeStart = 200.0f;
    [SerializeField] [Range(0, 1000)]
    private float m_indicatorFarFadeEnd = 250.0f;
    [SerializeField] [Range(0, 2)]
    private float m_indicatorMinSize = 0.5f;
    [SerializeField] [Range(0, 2)]
    private float m_indicatorMaxSize = 1.0f;
    [SerializeField] [Range(0, 1)]
    private float m_indicatorMaxAlpha = 1.0f;

    private Dictionary<Player, RectTransform> m_playerToIndicators;

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

    private Transform m_arrow;

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

    private int m_lastRank = 1;

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

    private int m_lastLap = 1;
    private float m_lapChangeTime = float.NegativeInfinity;

    [Header("Energy Bar")]
    [SerializeField]
    private GameObject m_energyBar;
    [SerializeField]
    private Image m_energyBarBorder;
    [SerializeField]
    private Image m_energyBarFill;
    [SerializeField]
    private Image m_energyBarHighlight;
    [SerializeField] [Range(0.01f, 1)]
    private float m_energyHighlightDuration = 0.35f;
    [SerializeField] [Range(0.01f, 1)]
    private float m_energyCantBoostAlpha = 0.35f;

    private float m_energyGainTime = float.NegativeInfinity;

    private Player m_player;
    private CameraManager m_camera;
    private RaceManager m_raceManager;

    private void Awake()
    {
        m_arrow = Instantiate(m_arrowPrefab);

        SetAlpha(m_newLapText, 0);
    }

    public PlayerUI Init(Player player, CameraManager cam, int playerCount)
    {
        m_player = player;
        m_camera = cam;
        
        m_raceManager = Main.Instance.RaceManager;

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
        m_playerToIndicators = new Dictionary<Player, RectTransform>();

        m_playerText.text = "Player " + (player.PlayerNum + 1);
        m_playerText.color = Color.Lerp(player.GetColor(), Color.white, 0.35f);
        
        SetArrow(m_player.NextWaypoint, 0);

        player.EnergyGained += Player_EnergyGained;

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
        float time = Mathf.Max(m_raceManager.GetStartRelativeTime(m_player.IsFinished ? m_player.FinishTime : Time.time), 0);
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time - (minutes * 60));
        int milliseconds = Mathf.FloorToInt((time - seconds - (minutes * 60)) * 100);

        m_timerText.text = string.Format(minutes.ToString() + ":" + seconds.ToString("D2") + ":" + milliseconds.ToString("D2"));
        
        int rank = m_raceManager.GetPlayerRank(m_player);
        bool isSolo = m_raceManager.PlayerCount == 1;

        RacePath path = m_raceManager.RacePath;
        Waypoint waypoint = m_player.NextWaypoint;
        int lap = path.GetCurrentLap(m_player.WaypointsCompleted);
        bool finished = m_player.IsFinished;
        
        m_arrow.gameObject.SetActive(!finished && waypoint != null);
        m_rankText.gameObject.SetActive(!finished && !isSolo);
        m_rankSubText.gameObject.SetActive(!finished && !isSolo);
        m_newLapText.gameObject.SetActive(!finished);
        m_energyBar.SetActive(!finished);

        m_finalRankText.gameObject.SetActive(finished && !isSolo);
        m_finalRankSubText.gameObject.SetActive(finished && !isSolo);

        if (m_arrow.gameObject.activeInHierarchy)
        {
            SetArrow(waypoint, m_arrowSmoothing);
        }
        
        foreach (Player player in m_raceManager.Players.OrderBy(p => Vector3.Distance(m_camera.transform.position, p.transform.position)))
        {
            if (player != m_player)
            {
                RectTransform indicator;
                if (!m_playerToIndicators.TryGetValue(player, out indicator))
                {
                    indicator = Instantiate(m_playerIndicatorPrefab, m_playerIndicatorParent);
                    indicator.SetAsFirstSibling();
                    indicator.pivot = new Vector2(0.5f, 0);

                    foreach (Graphic g in indicator.GetComponentsInChildren<Graphic>())
                    {
                        g.color = player.GetColor();
                    }

                    m_playerToIndicators.Add(player, indicator);
                }

                Vector3 camPos = m_camera.transform.position;
                Vector3 indicatorPos = player.transform.position + m_indicatorVerticalOffset * Vector3.up;
                Vector3 viewportPoint = m_camera.Camera.WorldToViewportPoint(indicatorPos);

                bool active = 0 < viewportPoint.z && !finished && !player.IsFinished;

                Vector3 inticatorDisp = indicatorPos - camPos;
                float distance = inticatorDisp.magnitude;
                Ray ray = new Ray(camPos, inticatorDisp);
                RaycastHit hit;

                if (active && !Physics.Raycast(ray, distance) && !m_arrow.GetComponent<Collider>().Raycast(ray, out hit, distance))
                {
                    indicator.gameObject.SetActive(true);
                    indicator.SetAsFirstSibling();
                    indicator.anchoredPosition = Vector2.zero;
                    indicator.anchorMin = viewportPoint;
                    indicator.anchorMax = viewportPoint;

                    float farFade = Mathf.Clamp01((distance - m_indicatorFarFadeStart) / (m_indicatorFarFadeEnd - m_indicatorFarFadeStart));
                    float nearFade = Mathf.Clamp01((distance - m_indicatorNearFadeStart) / (m_indicatorNearFadeEnd - m_indicatorNearFadeStart));
                    float fade = Mathf.Min(nearFade, 1 - farFade);
                    SetAlpha(indicator.GetComponentInChildren<Graphic>(), Mathf.Lerp(0, m_indicatorMaxAlpha, fade));

                    float scaleFac = 1 - Mathf.Clamp01((distance - m_indicatorNearFadeEnd) / (m_indicatorFarFadeStart - m_indicatorNearFadeEnd));
                    float scale = Mathf.Lerp(m_indicatorMinSize, m_indicatorMaxSize, scaleFac);
                    indicator.localScale = scale * Vector3.one;
                }
                else
                {
                    indicator.gameObject.SetActive(false);
                }
            }
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

        if (rank != m_lastRank && m_raceManager.GetStartRelativeTime() > 0)
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

            m_newLapText.enabled = (m_newLapText.color.a > 0);
        }

        m_lastRank = rank;
        m_lastLap = lap;

        // energy bar
        float energyFill = m_player.Energy / m_player.MaxEnergy;
        m_energyBarFill.fillAmount = energyFill;
        m_energyBarHighlight.fillAmount = energyFill;

        SetColorNoAlpha(m_energyBarFill, Color.Lerp(m_player.GetColor(), Color.white, 0.35f));
        SetAlpha(m_energyBarFill, m_player.Movement.CanBoost || m_player.Movement.IsBoosting ? 1 : m_energyCantBoostAlpha);

        SetAlpha(m_energyBarHighlight, 1 - Mathf.Clamp01((Time.time - m_energyGainTime) / m_energyHighlightDuration));
        
        SetColorNoAlpha(m_energyBarBorder, Color.Lerp(m_player.GetColor(), Color.white * 0.75f, m_player.Movement.IsBoosting ? 0 : 0.75f));
    }

    private void Player_EnergyGained(float total, float delta)
    {
        m_energyGainTime = Time.time;
    }

    private void SetArrow(Waypoint waypoint, float smoothing)
    {
        if (waypoint != null)
        {
            Vector3 arrowPos = m_camera.Camera.ViewportToWorldPoint(new Vector3(0.5f, m_arrowPosition, 1));
            Quaternion arrowRot = Quaternion.LookRotation(waypoint.Position - m_player.transform.position);

            m_arrow.position = arrowPos;
            m_arrow.rotation = m_arrowSmoothing > 0 ? Quaternion.Slerp(m_arrow.rotation, arrowRot, Time.deltaTime / smoothing) : arrowRot;
            m_arrow.localScale = m_arrowScale * Vector3.one;
        }
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
