using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MemeBoots))]
[RequireComponent(typeof(PlayerAnimation))]
public class Player : MonoBehaviour
{
    private MemeBoots m_movement;
    private PlayerAnimation m_animation;

    private int m_playerNum;
    public int PlayerNum
    {
        get { return m_playerNum; }
    }

    private int m_waypointsCompleted = 0;
    public int WaypointsCompleted
    {
        get { return m_waypointsCompleted; }
    }

    private Waypoint m_currentWaypoint;
    public Waypoint CurrentWaypoint
    {
        get { return m_currentWaypoint; }
    }

    private bool m_isFinished = false;
    public bool IsFinished { get { return m_isFinished; } }

    private float m_finishTime = float.MaxValue;
    public float FinishTime { get { return m_finishTime; } }

    private RaceManager m_raceManager;

    private void Awake()
    {
        m_movement = GetComponentInChildren<MemeBoots>();
        m_animation = GetComponentInChildren<PlayerAnimation>();

        m_raceManager = Main.Instance.RaceManager;
    }

    public void Init(int playerNum, PlayerInput input, PlayerConfig config)
    {
        m_playerNum = playerNum;

        m_movement.Init(input, config);
        
        m_currentWaypoint = m_raceManager.RacePath.GetWaypoint(m_waypointsCompleted);
    }

    public void MainUpdate(bool canAcceptInput)
    {
        m_movement.Move(!m_isFinished && canAcceptInput);

        bool finished = m_raceManager.RacePath.IsFinished(m_waypointsCompleted);
        if (finished != m_isFinished)
        {
            m_finishTime = Time.time;
            m_isFinished = finished;
        }
    }

    private void Update()
    {
        if (m_animation != null)
        {
            m_animation.UpdateAnimation(m_movement);
        }

        Debug.DrawLine(transform.position, m_currentWaypoint.Position, Color.magenta);
    }

    private void OnTriggerEnter(Collider other)
    {
        Waypoint waypoint = other.GetComponent<Waypoint>();
        if (waypoint == m_currentWaypoint)
        {
            GetNextWaypoint();
        }
    }

    private void GetNextWaypoint()
    {
        m_waypointsCompleted++;
        m_currentWaypoint = m_raceManager.RacePath.GetWaypoint(m_waypointsCompleted);
    }
}
