using UnityEngine;

using BoostBlasters.Levels;
using BoostBlasters.Races.Racers;

namespace BoostBlasters.Races
{
    public class RacePath : MonoBehaviour
    {
        [SerializeField]
        private Waypoint[] m_path = null;
        public Waypoint[] Path => m_path;

        [SerializeField]
        private Transform[] m_spawns = null;
        public Transform[] Spawns => m_spawns;

        private int m_laps;
        public int Laps => m_laps;

        private BoostGate[] m_energyGates = null;

        private void Awake()
        {
            m_energyGates = GetComponentsInChildren<BoostGate>();
        }

        public RacePath Init(int laps)
        {
            m_laps = laps;
            return this;
        }

        public void ResetPath()
        {
            foreach (Waypoint waypoint in m_path)
            {
                waypoint.ResetGate();
            }
            foreach (BoostGate energyGate in m_energyGates)
            {
                energyGate.ResetGate();
            }
        }

        public bool IsFinished(int waypointIndex)
        {
            if (m_path.Length > 0)
            {
                return waypointIndex >= (m_path.Length * m_laps);
            }
            return false;
        }

        public int GetCurrentLap(int waypointIndex)
        {
            if (m_path.Length > 0)
            {
                return Mathf.Clamp((waypointIndex / m_path.Length) + 1, 1, m_laps);
            }
            return 0;
        }

        public Waypoint GetWaypoint(int waypointIndex)
        {
            if (m_path.Length > 0 && !IsFinished(waypointIndex))
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
            foreach (BoostGate energyGate in m_energyGates)
            {
                energyGate.UpdateGate();
            }
        }

        public void ResetEnergyGates(Racer player)
        {
            foreach (BoostGate energyGate in m_energyGates)
            {
                energyGate.ResetGate(player);
            }
        }

        private void OnDrawGizmos()
        {
            for (int i = 0; i < m_spawns.Length; i++)
            {
                Transform spawn = m_spawns[i];

                if (spawn == null)
                {
                    continue;
                }

                Gizmos.color = Consts.GetRacerColor(i);

                Gizmos.matrix = spawn.localToWorldMatrix;
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(1.5f, 2, 1.5f));
                Gizmos.matrix = Matrix4x4.identity;

                if (m_path.Length > 0)
                {
                    Waypoint first = m_path[0];
                    if (first != null)
                    {
                        Gizmos.DrawLine(spawn.position, first.Position);
                    }
                }
            }

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
}
