using System.Collections.Generic;

using UnityEngine;

using BoostBlasters.Levels;

namespace BoostBlasters.Races.Racers
{
    /// <summary>
    /// Generates movement input using AI.
    /// </summary>
    public class AIInputProvider : IInputProvider
    {
        private const int PREDICTION_STEPS = 100;
        private const float PREDICTION_DELTA_TIME_SCALE = 2.0f;


        private Racer m_player;
        private Inputs m_input;
        private List<Vector3> m_positions = new List<Vector3>(PREDICTION_STEPS);
        private List<float> m_rotations = new List<float>(PREDICTION_STEPS);
        private float m_lastBoostTime;


        public AIInputProvider(Racer player)
        {
            m_player = player;

            ResetProvider();
        }

        public void ResetProvider()
        {
            m_lastBoostTime = float.MinValue;
        }

        public Inputs GetInput()
        {
            return m_input;
        }

        public void FixedUpdateProvider()
        {
            bool goooo = false;
            if (goooo)
            {
                m_input = new Inputs();

                Waypoint next = m_player.NextWaypoint;
                if (next != null)
                {
                    Vector3 position = m_player.transform.position;
                    float rotation = m_player.transform.rotation.eulerAngles.y;

                    float deltaTime = PREDICTION_DELTA_TIME_SCALE * Time.fixedDeltaTime;

                    Inputs bestInputs = m_input;
                    float bestValue = float.MinValue;

                    m_positions.Clear();
                    m_rotations.Clear();

                    m_positions.Add(position);
                    m_rotations.Add(rotation);

                    m_player.Movement.PredictStep(PREDICTION_STEPS, m_positions, m_player.Movement.Velocity, m_rotations, m_player.Movement.AngularVelocity, m_input, deltaTime);

                    for (int i = 1; i < m_positions.Count; i++)
                    {
                        Vector3 disp = next.Position - m_positions[i];
                        Vector3 vel = (m_positions[i] - m_positions[i - 1]) / deltaTime;
                        float angVel = (m_rotations[i] - m_rotations[i - 1]) / deltaTime;

                        float distFac = (disp.magnitude / 200) * Mathf.Lerp(1, 3, i / m_positions.Count);

                        float velDot = Vector3.Dot(vel, disp.normalized) / 100.0f;
                        float velFac = Mathf.Sign(velDot) * Mathf.Pow(Mathf.Abs(velDot), 4);
                        float angVelFac = Mathf.Lerp(0, (Mathf.Abs(angVel) / 3.0f), velFac);

                        float value = velFac - angVelFac - distFac;
                        if (bestValue < value)
                        {
                            bestValue = value;
                            bestInputs = m_input;
                        }
                    }
                    m_input = bestInputs;
                }
            }
            else
            {
                m_input = new Inputs();

                Waypoint next = m_player.NextWaypoint;
                if (next != null)
                {
                    Vector3 disp = next.Position - m_player.transform.position;
                    Vector3 fore = m_player.transform.forward;

                    Vector3 foreCross = Vector3.Cross(disp, fore);
                    float facing = Mathf.Acos(Vector3.Dot(Vector3.ProjectOnPlane(disp, Vector3.up).normalized, fore)) * Mathf.Rad2Deg;

                    if (facing > Mathf.Lerp(10.0f, 0.5f, (disp.magnitude + 1.0f) / 150f))
                    {
                        m_input.H = -foreCross.y;
                    }
                    else
                    {
                        if (disp.magnitude > 65 && Mathf.Abs(disp.normalized.y) < 0.1f && Time.time - m_lastBoostTime > 0.5f)
                        {
                            m_input.Boost = true;
                            m_lastBoostTime = Time.time;
                        }

                        if (disp.y > -4)
                        {
                            m_input.V = 1;
                        }
                    }
                }
            }
        }

        public void LateUpdateProvider()
        {
        }
    }
}
