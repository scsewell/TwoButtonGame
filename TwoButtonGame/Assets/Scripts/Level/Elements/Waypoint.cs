using System.Collections.Generic;

using UnityEngine;

using Framework.Interpolation;

using BoostBlasters.Races.Racers;
using BoostBlasters.Levels.Paths;

namespace BoostBlasters.Levels
{
    /// <summary>
    /// A gate that must be passed though when racing.
    /// </summary>
    public class Waypoint : MonoBehaviour, IOnWillRenderReceiver
    {
        [SerializeField]
        private Transform m_gate = null;
        [SerializeField]
        private Collider m_trigger = null;

        [Header("Glow")]

        [SerializeField]
        private int m_glowMaterialIndex = -1;
        [SerializeField]
        [Range(0f, 5f)]
        private float m_nextGlowIntensity = 2.25f;
        [SerializeField]
        [Range(0f, 5f)]
        private float m_secondNextGlowIntensity = 1.4f;
        [SerializeField]
        [Range(0f, 2f)]
        private float m_glowChangeTime = 0.5f;

        [Header("Bobbing")]

        [SerializeField]
        [Range(0, 10)]
        private float m_bobIntensity = 0.3f;
        [SerializeField]
        [Range(0, 10)]
        private float m_bobFrequency = 0.15f;
        [SerializeField]
        [Range(0, 1)]
        private float m_bobFrequencyVariance = 0.1f;

        [Header("Movement")]

        [SerializeField]
        private BezierSpline m_path = null;
        [SerializeField]
        private float m_duration = 10.0f;
        [SerializeField]
        private PathWrapMode m_wrapMode = PathWrapMode.Loop;
        [SerializeField]
        private bool m_flipDirection = false;
        [SerializeField]
        [Range(0, 1)]
        private float m_startOffset = 0;

        private TransformInterpolator m_interpolator;
        private float m_bobCycleOffset;
        private float m_bobFreq;
        private GateEngine[] m_engines;
        private Material m_glowMat;
        private Dictionary<Racer, float> m_playerToGlow = new Dictionary<Racer, float>();
        private Vector3 m_lastPosition;
        private Vector3 m_lastVelocity;
        private Vector3 m_engineForce;

        public Vector3 Position => m_gate.position;

        public Collider Trigger => m_trigger;

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

            m_bobCycleOffset = Random.value;
            m_bobFreq = m_bobFrequency * Random.Range(1 - m_bobFrequencyVariance, 1 + m_bobFrequencyVariance);
        }

        public void ResetGate()
        {
            if (m_path != null)
            {
                m_gate.position = m_path.GetPointWithWaits(GetPathTime(), m_wrapMode);
            }

            m_lastPosition = m_gate.position;
            m_lastVelocity = Vector3.zero;
            m_engineForce = Vector3.zero;

            if (m_interpolator != null)
            {
                m_interpolator.ForgetPreviousValues();
            }

            foreach (Racer player in Main.Instance.RaceManager.Racers)
            {
                m_playerToGlow[player] = 0f;
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

                m_engineForce = Vector3.Lerp(m_engineForce, Vector3.Lerp(velocity, acceleration, 0.5f), Time.deltaTime / 0.25f);

                foreach (GateEngine engine in m_engines)
                {
                    engine.UpdateEngine(m_engineForce);
                }

                m_lastVelocity = velocity;
                m_lastPosition = pos;
            }

            float time = Main.Instance.RaceManager.GetStartRelativeTime();
            pos.y += m_bobIntensity * Mathf.Sin((time * m_bobFreq + m_bobCycleOffset) * (2f * Mathf.PI));

            m_gate.SetPositionAndRotation(pos, rot);
        }

        private float GetPathTime()
        {
            float t = Main.Instance.RaceManager.GetStartRelativeTime() / m_duration;
            return m_startOffset + (m_flipDirection ? -t : t);
        }

        public void UpdateWaypoint()
        {
            foreach (Racer player in Main.Instance.RaceManager.Racers)
            {
                float glowFac = m_playerToGlow[player];

                bool isNextWaypoint = (player.NextWaypoint == this);
                bool isSecondNextWaypoint = (player.SecondNextWaypoint == this);

                float glowTarget = isSecondNextWaypoint ? 1f : (isNextWaypoint ? 2f : 3f);
                if (!(glowFac == 0f && glowTarget == 3f) && m_glowChangeTime > 0f)
                {
                    glowFac = Mathf.MoveTowards(glowFac, glowTarget, Time.deltaTime / m_glowChangeTime);
                }
                else
                {
                    glowFac = glowTarget;
                }

                if (glowFac == 3f)
                {
                    glowFac = 0f;
                }

                m_playerToGlow[player] = glowFac;
            }
        }

        public void OnWillRender(Camera cam)
        {
            if (m_glowMat != null)
            {
                // make the waypoint glow only for a player's camera if this is the next waypoint
                RacerCamera racerCam = cam.GetComponentInParent<RacerCamera>();

                Color diffuse = new Color(0.35f, 0.35f, 0.35f);
                Color glow = new Color(1, 0.41f, 0) * 1.75f;

                if (racerCam != null)
                {
                    float glowFac;
                    if (!m_playerToGlow.TryGetValue(racerCam.Owner, out glowFac))
                    {
                        glowFac = 0;
                    }
                    Color playerColor = racerCam.Owner.GetColor();

                    diffuse = SampleGradient(glowFac, diffuse, Color.white, playerColor);
                    glow = SampleGradient(glowFac, Color.black, m_secondNextGlowIntensity * Color.white, m_nextGlowIntensity * playerColor);
                }

                m_glowMat.SetColor("_Color", diffuse);
                m_glowMat.SetColor("_EmissionColor", glow);
            }
        }

        private Color SampleGradient(float fac, Color off, Color secondNext, Color next)
        {
            float wrapped = fac % 3f;
            if (wrapped <= 1f)
            {
                return Color.Lerp(off, secondNext, wrapped);
            }
            else if (wrapped <= 2f)
            {
                return Color.Lerp(secondNext, next, wrapped - 1f);
            }
            else
            {
                return Color.Lerp(next, off, wrapped - 2f);
            }
        }
    }
}
