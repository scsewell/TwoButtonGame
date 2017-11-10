﻿using System.Collections.Generic;
using UnityEngine;
using Framework.Interpolation;

[RequireComponent(typeof(MemeBoots))]
[RequireComponent(typeof(TransformInterpolator))]
public class Player : MonoBehaviour
{
    private int m_playerNum = -1;
    public int PlayerNum { get { return m_playerNum; } }

    private PlayerConfig m_config;
    public PlayerConfig Config { get { return m_config; } }

    private PlayerInput m_input;
    public PlayerInput Input { get { return m_input; } }

    public Color GetColor()
    {
        return Consts.PLAYER_COLORS[m_playerNum];
    }


    private int m_waypointsCompleted = 0;
    public int WaypointsCompleted { get { return m_waypointsCompleted; } }
    
    private bool m_isFinished = false;
    public bool IsFinished { get { return m_isFinished; } }

    private float m_finishTime = float.MaxValue;
    public float FinishTime { get { return m_finishTime; } }
    
    public Waypoint NextWaypoint
    {
        get { return m_racePath.GetWaypoint(m_waypointsCompleted); }
    }

    public Waypoint SecondNextWaypoint
    {
        get { return m_racePath.GetWaypoint(m_waypointsCompleted + 1); }
    }


    public delegate void EnergyGainedHandler(float total, float delta);
    public event EnergyGainedHandler EnergyGained;

    public float MaxEnergy
    {
        get { return m_config.EnergyCap; }
    }

    private float m_energy = 0;
    public float Energy
    {
        get { return m_energy; }
    }

    private MemeBoots m_movement;
    public MemeBoots Movement { get { return m_movement; } }
    
    private PlayerAnimation m_animation;
    private RacePath m_racePath;
    private Dictionary<BoostGate, int> m_lastUseProgress = new Dictionary<BoostGate, int>();
    private Vector3 m_lastPos;


    private void Awake()
    {
        m_movement = GetComponentInChildren<MemeBoots>();
        m_lastPos = transform.position;
    }

    public Player Init(int playerNum, PlayerInput input, PlayerConfig config)
    {
        m_playerNum = playerNum;
        m_input = input;
        m_config = config;

        m_animation = GetComponentInChildren<PlayerAnimation>();
        m_racePath = Main.Instance.RaceManager.RacePath;
        
        m_energy = 0;

        return this;
    }

    public void FixedUpdatePlayer(bool isAfterIntro, bool isAfterStart)
    {
        m_movement.FixedUpdateMovement(isAfterIntro && !m_isFinished, !isAfterStart);

        if (isAfterStart && !m_isFinished && !m_movement.IsBoosting)
        {
            m_energy = Mathf.Min(m_energy + (m_config.EnergyRechargeRate * Time.deltaTime), MaxEnergy);
        }

        Waypoint wp = NextWaypoint;
        Vector3 disp = transform.position - m_lastPos;
        RaycastHit hit;
        if (wp != null && wp.Trigger.Raycast(new Ray(m_lastPos, disp), out hit, disp.magnitude))
        {
            m_waypointsCompleted++;
        }
        m_lastPos = transform.position;

        bool finished = m_racePath.IsFinished(m_waypointsCompleted);
        if (finished != m_isFinished)
        {
            m_finishTime = Time.time;
            m_isFinished = finished;
        }
    }

    public void UpdatePlayer()
    {
        m_movement.UpdateMovement();

        if (m_animation != null)
        {
            m_animation.UpdateAnimation(m_movement);
        }

        if (NextWaypoint != null)
        {
            Debug.DrawLine(transform.position, NextWaypoint.Position, Color.magenta);
        }
    }

    public void LateUpdatePlayer()
    {
        if (m_animation != null)
        {
            m_animation.LateUpdateAnimation(NextWaypoint);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        BoostGate boostGate = other.GetComponentInParent<BoostGate>();
        if (boostGate != null)
        {
            int lastProgress;
            if (!m_lastUseProgress.TryGetValue(boostGate, out lastProgress))
            {
                lastProgress = int.MinValue;
            }

            if (m_waypointsCompleted != lastProgress)
            {
                OnEnergyGained(boostGate.Energy);
                m_lastUseProgress[boostGate] = m_waypointsCompleted;
            }
        }
    }

    private void OnEnergyGained(float amountGained)
    {
        float delta = Mathf.Min(m_energy + amountGained, MaxEnergy) - m_energy;
        if (delta > 0)
        {
            m_energy += delta;
            if (EnergyGained != null)
            {
                EnergyGained(m_energy, delta);
            }
        }
    }

    public float ConsumeEnergy(float amountLost)
    {
        float delta = Mathf.Max(m_energy - amountLost, 0) - m_energy;
        m_energy += delta;
        return Mathf.Abs(delta);
    }
}
