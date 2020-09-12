using BoostBlasters.Levels.Elements;
using BoostBlasters.Races.Racers;

using UnityEngine;

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

        public int Laps { get; private set; }

        private BoostGate[] m_energyGates = null;

        private void Awake() => m_energyGates = GetComponentsInChildren<BoostGate>();

        public RacePath Init(int laps)
        {
            Laps = laps;
            return this;
        }

        public void ResetPath()
        {
            foreach (var waypoint in m_path)
            {
                waypoint.ResetGate();
            }
            foreach (var energyGate in m_energyGates)
            {
                energyGate.ResetGate();
            }
        }

        public bool IsFinished(int waypointIndex)
        {
            if (m_path.Length > 0)
            {
                return waypointIndex >= (m_path.Length * Laps);
            }
            return false;
        }

        public int GetCurrentLap(int waypointIndex)
        {
            if (m_path.Length > 0)
            {
                return Mathf.Clamp((waypointIndex / m_path.Length) + 1, 1, Laps);
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
            foreach (var waypoint in m_path)
            {
                waypoint.FixedUpdateWaypoint();
            }
        }

        public void UpdatePath()
        {
            foreach (var waypoint in m_path)
            {
                waypoint.UpdateWaypoint();
            }
            foreach (var energyGate in m_energyGates)
            {
                //energyGate.UpdateGate();
            }
        }

        public void ResetEnergyGates(Racer player)
        {
            foreach (var energyGate in m_energyGates)
            {
                //energyGate.ResetGate(player);
            }
        }

        private void OnDrawGizmos()
        {
            for (var i = 0; i < m_spawns.Length; i++)
            {
                var spawn = m_spawns[i];

                if (spawn == null)
                {
                    continue;
                }

                Gizmos.color = Color.green;

                Gizmos.matrix = spawn.localToWorldMatrix;
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(1.5f, 2, 1.5f));
                Gizmos.matrix = Matrix4x4.identity;

                if (m_path.Length > 0)
                {
                    var first = m_path[0];
                    if (first != null)
                    {
                        Gizmos.DrawLine(spawn.position, first.Position);
                    }
                }
            }

            for (var i = 0; i < m_path.Length; i++)
            {
                var start = m_path[i];
                var end = m_path[(i + 1) % m_path.Length];

                if (start == null || end == null)
                {
                    continue;
                }

                Gizmos.color = Color.yellow;

                var startPos = start.Position;
                var endPos = end.Position;
                Gizmos.DrawLine(startPos, endPos);
            }
        }
    }
}
