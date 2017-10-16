using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacePath : MonoBehaviour
{
    [SerializeField]
    private Waypoint[] m_path;
    public Waypoint[] Path
    {
        get { return m_path; }
    }

    [SerializeField] [Range(1, 10)]
    private int m_laps = 3;
    public int Laps
    {
        get { return m_laps; }
    }
    
    private void Awake()
    {
    }
    
    private void Update()
    {
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
        return m_path[waypointIndex % m_path.Length];
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < m_path.Length; i++)
        {
            Gizmos.color = Color.yellow;

            Vector3 start = m_path[i].Position;
            Vector3 end = m_path[(i + 1) % m_path.Length].Position;
            Gizmos.DrawLine(start, end);
        }
    }
}
 