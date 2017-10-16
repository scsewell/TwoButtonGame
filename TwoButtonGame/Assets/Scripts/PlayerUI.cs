﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField]
    private Image m_fade;

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

    private CanvasScaler m_canvasScaler;
    private Transform m_arrow;
    private float m_baseScaleFactor;
    private int m_lastRank = 1;
    private int m_lastLap = 1;
    private float m_lapChangeTime = float.NegativeInfinity;

    private void Awake()
    {
        m_canvasScaler = GetComponent<CanvasScaler>();
        m_baseScaleFactor = m_canvasScaler.referenceResolution.y;

        m_arrow = Instantiate(m_arrowPrefab);

        SetAlpha(m_newLapText, 0);
    }

    private void OnDestroy()
    {
        if (m_arrow != null)
        {
            Destroy(m_arrow);
            m_arrow = null;
        }
    }

    public void UpdateUI(Camera cam, Player player)
    {
        // set the canvas scale
        m_canvasScaler.referenceResolution = new Vector2(m_canvasScaler.referenceResolution.x, m_baseScaleFactor / cam.rect.height);

        // fading
        SetAlpha(m_fade, Main.Instance.FadeFactor);

        // start countdown
        float countdown = Main.Instance.CountdownTime;
        m_countdownText.gameObject.SetActive(-1 < countdown && countdown <= 3);
        m_countdownText.text = (countdown > 0) ? Mathf.CeilToInt(countdown).ToString() : "GO!";

        // timer
        float time = Main.Instance.GetTimeSinceStart(player.IsFinished ? player.FinishTime : Time.time);
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time - (minutes * 60));
        int milliseconds = Mathf.FloorToInt((time - seconds - (minutes * 60)) * 100);

        m_timerText.text = string.Format(minutes.ToString() + ":" + seconds.ToString("D2") + ":" + milliseconds.ToString("D2"));

        // player text
        m_playerText.text = "Player " + (player.PlayerNum + 1);
        m_playerText.color = Consts.PLAYER_COLORS[player.PlayerNum];

        int rank = Main.Instance.GetPlayerRank(player);

        RacePath path = Main.Instance.RacePath;
        int lap = path.GetCurrentLap(player.WaypointsCompleted);
        bool finished = player.IsFinished;
        
        m_arrow.gameObject.SetActive(!finished);
        m_rankText.gameObject.SetActive(!finished);
        m_rankSubText.gameObject.SetActive(!finished);
        m_lapText.gameObject.SetActive(!finished);
        m_newLapText.gameObject.SetActive(!finished);

        m_finalRankText.gameObject.SetActive(finished);
        m_finalRankSubText.gameObject.SetActive(finished);

        if (!finished)
        {
            // set the waypoint arrow
            Vector3 arrowTarget = player.CurrentWaypoint.Position;

            Vector3 arrowPos = cam.ViewportToWorldPoint(new Vector3(0.5f, m_arrowPosition, 1));
            Quaternion arrowRot = Quaternion.LookRotation(arrowTarget - player.transform.position);

            m_arrow.position = arrowPos;
            m_arrow.rotation = m_arrowSmoothing > 0 ? Quaternion.Slerp(m_arrow.rotation, arrowRot, Time.deltaTime / m_arrowSmoothing) : arrowRot;
            m_arrow.localScale = m_arrowScale * Vector3.one;

            int baseLayer = 10;
            int arrowLayer = player.PlayerNum + baseLayer;
            m_arrow.gameObject.layer = arrowLayer;

            for (int layer = baseLayer; layer < baseLayer + 4; layer++)
            {
                cam.cullingMask &= ~(1 << layer);
            }

            cam.cullingMask |= (1 << arrowLayer);
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

        if (rank != m_lastRank)
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
