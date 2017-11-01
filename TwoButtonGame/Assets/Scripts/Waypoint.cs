﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Interpolation;

public class Waypoint : MonoBehaviour, OnWillRenderReceiver
{
    [SerializeField]
    private Transform m_gate;

    [Header("Glow")]
    [SerializeField]
    private int m_glowMaterialIndex = -1;
    [SerializeField] [Range(0, 5)]
    private float m_nextGlowIntensity = 2.25f;
    [SerializeField] [Range(0, 5)]
    private float m_secondNextGlowIntensity = 1.4f;
    [SerializeField] [Range(0, 2)]
    private float m_glowChangeTime = 0.5f;

    [Header("Bobbing")]
    [SerializeField] [Range(0, 10)]
    private float m_bobIntensity = 0.3f;
    [SerializeField] [Range(0, 10)]
    private float m_bobFrequency = 0.15f;
    [SerializeField] [Range(0, 1)]
    private float m_bobFrequencyVariance = 0.1f;

    [Header("Movement")]
    [SerializeField]
    private BezierSpline m_path;
    [SerializeField]
    private float m_duration = 10.0f;
    [SerializeField]
    private WrapMode m_wrapMode = WrapMode.Loop;
    [SerializeField]
    private bool m_flipDirection = false;
    [SerializeField] [Range(0, 1)]
    private float m_startOffset = 0;
    [SerializeField] [Range(0.01f, 1)]
    private float m_accelerationSmoothing = 0.135f;

    private RaceManager m_raceManager;
    private TransformInterpolator m_interpolator;
    private GateEngine[] m_engines;
    private Material m_glowMat;
    private Dictionary<Player, float> m_playerToGlow = new Dictionary<Player, float>();
    private float m_bobCycleOffset;
    private float m_bobFreq;
    private Vector3 m_lastPosition;
    private Vector3 m_lastVelocity;
    private Vector3 m_acceleration;

    public Vector3 Position
    {
        get { return m_gate.position; }
    }

    private void Awake()
    {
        m_interpolator = m_gate.GetComponent<TransformInterpolator>();

        Renderer renderer = m_gate.GetComponentInChildren<Renderer>();
        if (renderer != null && m_glowMaterialIndex >= 0)
        {
            m_glowMat = renderer.materials[m_glowMaterialIndex];
        }

        m_engines = m_gate.GetComponentsInChildren<GateEngine>(true);

        foreach (GateEngine engine in m_engines)
        {
            engine.gameObject.SetActive(m_path != null);
        }
    }

    public void Init()
    {
        m_bobCycleOffset = Random.value;
        m_bobFreq = m_bobFrequency * Random.Range(1 - m_bobFrequencyVariance, 1 + m_bobFrequencyVariance);
        
        if (m_path != null)
        {
            m_gate.position = m_path.GetPointWithWaits(GetPathTime(), m_wrapMode);
            m_lastPosition = m_gate.position;
        }

        if (m_interpolator != null)
        {
            m_interpolator.ForgetPreviousValues();
        }
    }

    public void FixedUpdateWaypoint()
    {
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;

        if (m_path != null)
        {
            pos = m_path.GetPointWithWaits(GetPathTime(), m_wrapMode);
            
            Vector3 velocity = (pos - m_lastPosition) / Time.deltaTime;
            Vector3 acceleration = (velocity - m_lastVelocity) / Time.deltaTime;
            m_acceleration = Vector3.Lerp(m_acceleration, acceleration, Time.deltaTime / m_accelerationSmoothing);
            m_lastVelocity = velocity;
            m_lastPosition = pos;

            foreach (GateEngine engine in m_engines)
            {
                engine.UpdateEngine(m_acceleration);
            }
        }

        float time = Main.Instance.RaceManager.GetStartRelativeTime();
        pos.y += m_bobIntensity * Mathf.Sin((time * m_bobFreq + m_bobCycleOffset) * (2 * Mathf.PI));

        m_gate.SetPositionAndRotation(pos, rot);
    }

    private float GetPathTime()
    {
        float t = Main.Instance.RaceManager.GetStartRelativeTime() / m_duration;
        return m_startOffset + (m_flipDirection ? -t : t);
    }

    public void UpdateWaypoint()
    {
        foreach (Player player in Main.Instance.RaceManager.Players)
        {
            float glowFac;
            if (!m_playerToGlow.TryGetValue(player, out glowFac))
            {
                glowFac = 0;
            }

            bool isNextWaypoint = (player.NextWaypoint == this);
            bool isSecondNextWaypoint = (player.SecondNextWaypoint == this);

            float glowTarget = isSecondNextWaypoint ? 1 : (isNextWaypoint ? 2 : 3);
            if (!(glowFac == 0 && glowTarget == 3)  && m_glowChangeTime > 0)
            {
                glowFac = Mathf.MoveTowards(glowFac, glowTarget, Time.deltaTime / m_glowChangeTime);
            }
            else
            {
                glowFac = glowTarget;
            }

            if (glowFac == 3)
            {
                glowFac = 0;
            }

            m_playerToGlow[player] = glowFac;
        }
    }

    public void OnWillRender(Camera cam)
    {
        // make the waypoint glow only for a player's camera if this is the next waypoint
        CameraManager camManager = cam.GetComponent<CameraManager>();

        Color diffuse = new Color(0.35f, 0.35f, 0.35f);
        Color glow = Color.black;

        if (camManager != null && m_glowMat != null)
        {
            float glowFac;
            if (!m_playerToGlow.TryGetValue(camManager.Owner, out glowFac))
            {
                glowFac = 0;
            }
            Color playerColor = camManager.Owner.GetColor();

            diffuse = SampleGradient(glowFac, diffuse, Color.white, playerColor);
            glow = SampleGradient(glowFac, glow, m_secondNextGlowIntensity * Color.white, m_nextGlowIntensity * playerColor);
        }

        m_glowMat.SetColor("_Color", diffuse);
        m_glowMat.SetColor("_EmissionColor", glow);
    }

    private Color SampleGradient(float fac, Color off, Color secondNext, Color next)
    {
        float wrapped = fac % 3;
        if (wrapped <= 1)
        {
            return Color.Lerp(off, secondNext, wrapped);
        }
        else if (wrapped <= 2)
        {
            return Color.Lerp(secondNext, next, wrapped - 1);
        }
        else
        {
            return Color.Lerp(next, off, wrapped - 2);
        }
    }
}
