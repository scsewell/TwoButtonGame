using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MemeBoots))]
public class Player : MonoBehaviour
{
    private MemeBoots m_movement;

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

    private void Awake()
    {
        m_movement = GetComponentInChildren<MemeBoots>();
    }

    public void Init(int playerNum, PlayerInput input, PlayerConfig config)
    {
        m_playerNum = playerNum;

        m_movement.Init(input, config);
        
        m_currentWaypoint = Main.Instance.RacePath.GetWaypoint(m_waypointsCompleted);
    }

    public void MainUpdate()
    {
        bool finished = Main.Instance.RacePath.IsFinished(m_waypointsCompleted);
        if (finished != m_isFinished)
        {
            m_finishTime = Time.time;
            m_isFinished = finished;
        }

        m_movement.Move(!m_isFinished && Main.Instance.CountdownTime <= 0);
    }

    private void Update()
    {
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
        m_currentWaypoint = Main.Instance.RacePath.GetWaypoint(m_waypointsCompleted);
    }
}
