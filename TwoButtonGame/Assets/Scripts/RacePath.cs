using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacePath : MonoBehaviour
{
    [SerializeField]
    private Waypoint[] m_path;
    public Waypoint[] Path { get { return m_path; } }

    [SerializeField] [Range(1, 10)]
    private int m_laps = 3;
    public int Laps { get { return m_laps; } }

    [SerializeField]
    private Transform[] m_spawns;
    public Transform[] Spawns { get { return m_spawns; } }

    private void Awake()
    {
        if (Main.Instance.LastRaceParams == null)
        {
            Main.Instance.LoadMainMenu();
        }
    }

    public bool IsFinished(int waypointIndex)
    {
        return waypointIndex >= (m_path.Length * m_laps);
    }

    public int GetCurrentLap(int waypointIndex)
    {
        return Mathf.Clamp((waypointIndex / m_path.Length) + 1, 1, m_laps);
    }

    public Waypoint GetWaypoint(int waypointIndex)
    {
        if (!IsFinished(waypointIndex))
        {
            return m_path[waypointIndex % m_path.Length];
        }
        return null;
    }

    public void FixedUpdatePath()
    {
        foreach (Waypoint waypoint in m_path)
        {
            waypoint.FixedUpdateWaypoint();
        }
    }

    public void UpdatePath()
    {
        foreach (Waypoint waypoint in m_path)
        {
            waypoint.UpdateWaypoint();
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < m_path.Length; i++)
        {
            Waypoint start = m_path[i];
            Waypoint end = m_path[(i + 1) % m_path.Length];

            if (start == null || end == null)
            {
                continue;
            }
            
            Gizmos.color = Color.yellow;

            Vector3 startPos = start.Position;
            Vector3 endPos = end.Position;
            Gizmos.DrawLine(startPos, endPos);
        }
    }
}