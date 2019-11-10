using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Framework.Interpolation;

using BoostBlasters.Characters;

namespace BoostBlasters.Races.Racers
{
    /// <summary>
    /// Responsible for executing racer movement.
    /// </summary>
    [RequireComponent(typeof(TransformInterpolator))]
    public class RacerMovement : MonoBehaviour
    {
        private const float TENSOR = 0.4f;

        [SerializeField]
        private LayerMask m_groundLayers = Physics.DefaultRaycastLayers;
        [SerializeField]
        private LayerMask m_boundsLayers = Physics.DefaultRaycastLayers;
        [SerializeField]
        [Range(0f, 100f)]
        private float m_boundsCorrectionStrength = 10.0f;
        [SerializeField]
        [Range(0f, 20f)]
        private float m_collisionTorqueIntensity = 0.0f;
        [SerializeField]
        [Range(0f, 20f)]
        private float m_collisionTorqueLimit = 10f;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_minBoostTime = 0.25f;

        [Header("Visualization")]

        [SerializeField]
        private bool m_showPrediction = false;

        private Racer m_player = null;
        private CapsuleCollider m_capsule = null;
        private Rigidbody m_body = null;
        private RaycastHit[] m_hits = new RaycastHit[20];
        private float m_boundsEffect;
        private Vector3 m_boundsCorrectDir;
        private float m_boostDuration;
        private float m_boostEndTime;
        private bool m_boostPressed;

        private TrajectoryVisualization m_trajectoryVisualization;

        public bool CanBoost => m_player.Energy >= m_player.Character.BoostEnergyUseRate * m_minBoostTime;

        private bool m_isGrounded;
        public bool IsGrounded => m_isGrounded;

        private float m_leftEngine;
        public float LeftEngine => m_leftEngine;

        private float m_rightEngine;
        public float RightEngine => m_rightEngine;

        private float m_brake;
        public float Brake => m_brake;

        private bool m_isBoosting;
        public bool IsBoosting => m_isBoosting;

        private float m_boostFactor;
        public float BoostFactor => m_boostFactor;

        public Vector3 Velocity
        {
            get => m_body.velocity;
            set => m_body.velocity = value;
        }

        private float m_angVelocity = 0f;
        public float AngularVelocity
        {
            get => m_angVelocity;
            set => m_angVelocity = value;
        }

        private void Awake()
        {
            m_player = GetComponentInParent<Racer>();
            m_body = GetComponent<Rigidbody>();
            m_capsule = GetComponent<CapsuleCollider>();
        }

        public void ResetMovement()
        {
            m_body.velocity = Vector3.zero;
            m_angVelocity = 0f;

            m_boundsEffect = 0f;
            m_boundsCorrectDir = Vector3.up;
            m_boostDuration = 0f;
            m_boostEndTime = float.MinValue;
            m_boostFactor = 0f;
            m_boostPressed = false;

            m_isGrounded = true;
            m_leftEngine = 0f;
            m_rightEngine = 0f;
            m_brake = 0f;
            m_isBoosting = false;
        }

        private void OnDestroy()
        {
            if (m_trajectoryVisualization != null)
            {
                m_trajectoryVisualization.EndVisualization();
                m_trajectoryVisualization = null;
            }
        }

        public void FixedUpdateMovement(Inputs input, bool acceptInput, bool inPreWarm)
        {
            Character config = m_player.Character;
            ConfigurePhysics(config);

            if (acceptInput && !inPreWarm)
            {
                if (input.Boost)
                {
                    if (!m_isBoosting)
                    {
                        if (CanBoost)
                        {
                            m_isBoosting = true;
                            m_boostDuration = 0;
                        }
                        else if (!m_boostPressed)
                        {
                            m_player.OnEnergyUseFail();
                        }
                        m_boostPressed = true;
                    }
                }
                else
                {
                    m_boostPressed = false;
                    if (m_boostDuration >= m_minBoostTime)
                    {
                        m_isBoosting = false;
                    }
                }
            }
            else
            {
                m_isBoosting = false;
            }

            if (m_isBoosting)
            {
                m_player.ConsumeEnergy(config.BoostEnergyUseRate * Time.deltaTime);
                m_boostDuration += Time.deltaTime;
                m_boostEndTime = Time.time;

                if (m_player.Energy <= 0f)
                {
                    m_isBoosting = false;
                }
            }

            GetEngines(input, acceptInput, out m_leftEngine, out m_rightEngine, out m_brake);

            Vector3 force = Vector3.zero;

            RaycastHit boundsHit;
            bool outOfBounds = Physics.Raycast(transform.position, -transform.position, out boundsHit, transform.position.magnitude, m_boundsLayers);
            if (outOfBounds)
            {
                m_boundsCorrectDir = boundsHit.normal;
                force += m_boundsCorrectionStrength * -boundsHit.normal;
            }
            m_boundsEffect = Mathf.MoveTowards(m_boundsEffect, outOfBounds ? 1f : 0f, Time.deltaTime / 0.2f);

            m_boostFactor = 1f - Mathf.Clamp01((Time.time - m_boostEndTime) / 0.2f);
            float boostStrength = Mathf.Clamp01(Mathf.Exp(-Vector3.Dot(m_body.velocity, transform.forward) / config.BoostSoftCap));
            Vector3 boostForce = m_boostFactor * config.BoostAcceleration * boostStrength * transform.forward;
            force += ApplyBoundsCorrection(boostForce);

            Vector3 gravity = (1f - m_boostFactor) * config.GravityFac * Physics.gravity;
            force += ApplyBoundsCorrection(gravity);

            float torque = 0f;
            if (!inPreWarm)
            {
                Vector3 engineForeForce = config.ForwardAccel * transform.forward;
                Vector3 engineUpForce = config.VerticalAccel * config.GravityFac * Vector3.up;

                Vector3 engineForce = (1f - m_boostFactor) * (engineForeForce + engineUpForce);
                Vector3 forceOffset = config.TurnRatio * transform.right;

                Vector3 linearForce = Vector3.zero;

                linearForce += m_leftEngine * engineForce;
                torque += m_leftEngine * Vector3.Dot(Vector3.Cross(-forceOffset, engineForce), Vector3.up);

                linearForce += m_rightEngine * engineForce;
                torque += m_rightEngine * Vector3.Dot(Vector3.Cross(forceOffset, engineForce), Vector3.up);

                force += ApplyBoundsCorrection(linearForce);
            }

            m_body.velocity = StepVelocity(m_body.velocity, force, m_body.mass, config.LinearDrag + (m_brake * config.BrakeDrag), Time.deltaTime);
            m_angVelocity = StepAngularVelocity(m_angVelocity, torque, TENSOR, config.AngularDrag, Time.deltaTime);

            transform.Rotate(Vector3.up, 180 * m_angVelocity * Time.deltaTime);

            // Check if touching the ground
            Vector3 lowerSphereCenter = transform.TransformPoint(m_capsule.center + (((m_capsule.height / 2f) - m_capsule.radius) * Vector3.down));

            int hitCount = Physics.SphereCastNonAlloc(lowerSphereCenter, m_capsule.radius * 0.8f, Vector3.down, m_hits, m_capsule.radius * 0.22f, m_groundLayers);

            m_isGrounded = false;
            for (int i = 0; i < hitCount; i++)
            {
                if (Vector3.Angle(m_hits[i].normal, Vector3.up) < 20.0f)
                {
                    m_isGrounded = true;
                    break;
                }
            }

            // Visualizations
            if (Application.isEditor && m_showPrediction && m_trajectoryVisualization == null)
            {
                m_trajectoryVisualization = new TrajectoryVisualization(true, false);
            }
            else if (!m_showPrediction && m_trajectoryVisualization != null)
            {
                m_trajectoryVisualization.EndVisualization();
                m_trajectoryVisualization = null;
            }
            if (m_trajectoryVisualization != null)
            {
                m_trajectoryVisualization.FixedMemeUpdateTrajectory(this, m_body.position, m_body.velocity, transform.rotation.eulerAngles.y, m_angVelocity, input);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            // apply torque from collisions if desired, since we disable the lock phsyics engine rotation
            if (m_collisionTorqueIntensity > 0f)
            {
                float torque = 0f;

                Vector3 impulse = collision.impulse / collision.contactCount;

                for (int i = 0; i < collision.contactCount; i++)
                {
                    ContactPoint contact = collision.GetContact(i);
                    Vector3 disp = contact.point - m_body.position;

                    torque += Vector3.Cross(disp, impulse).y;
                }

                m_angVelocity += Mathf.Clamp(m_collisionTorqueIntensity * torque, -m_collisionTorqueLimit, m_collisionTorqueLimit) * Time.deltaTime;
            }
        }

        private void ConfigurePhysics(Character config)
        {
            m_body.solverIterations = 12;
            m_body.solverVelocityIterations = 2;
            m_body.useGravity = false;
            m_body.drag = 0f;
            m_body.angularDrag = 0f;
            m_capsule.material = config.PhysicsMat;
        }

        public void PredictStep(int steps, List<Vector3> pos, Vector3 velocity, List<float> rot, float angularVelocity, Inputs input, float deltaTime)
        {
            Character config = m_player.Character;

            GetEngines(input, true, out float left, out float right, out float brake);

            for (int i = 0; i < steps; i++)
            {
                Vector3 forward = Quaternion.Euler(0f, rot.Last(), 0f) * Vector3.forward;

                Vector3 force = Vector3.zero;
                float torque = 0f;

                if (input.Boost)
                {
                    float boostStrength = Mathf.Clamp01(Mathf.Exp(-Vector3.Dot(velocity, forward) / config.BoostSoftCap));
                    force += config.BoostAcceleration * boostStrength * forward;
                }
                else
                {
                    force += config.GravityFac * Physics.gravity;

                    Vector3 engineForeForce = config.ForwardAccel * forward;
                    Vector3 engineUpForce = config.VerticalAccel * config.GravityFac * Vector3.up;

                    Vector3 engineForce = engineForeForce + engineUpForce;
                    Vector3 forceOffset = config.TurnRatio * Vector3.Cross(Vector3.up, forward).normalized;

                    force += left * engineForce;
                    torque += left * Vector3.Dot(Vector3.Cross(-forceOffset, engineForce), Vector3.up);

                    force += right * engineForce;
                    torque += right * Vector3.Dot(Vector3.Cross(forceOffset, engineForce), Vector3.up);
                }

                velocity = StepVelocity(velocity, force, m_body.mass, m_player.Character.LinearDrag + (brake * config.BrakeDrag), deltaTime);
                angularVelocity = StepAngularVelocity(angularVelocity, torque, TENSOR, m_player.Character.AngularDrag, deltaTime);

                pos.Add(pos.Last() + (velocity * deltaTime));
                rot.Add(rot.Last() + (180f * angularVelocity * deltaTime));
            }
        }

        private static Vector3 StepVelocity(Vector3 velocity, Vector3 force, float mass, float drag, float deltaTime)
        {
            velocity = PhysicsUtils.ApplyForce(velocity, force, mass, deltaTime);
            velocity = PhysicsUtils.ApplyDamping(velocity, drag, deltaTime);
            return velocity;
        }

        private static float StepAngularVelocity(float angularVelocity, float torque, float tensor, float drag, float deltaTime)
        {
            angularVelocity = PhysicsUtils.ApplyTorque(angularVelocity, torque, tensor, deltaTime);
            angularVelocity = PhysicsUtils.ApplyAngularDamping(angularVelocity, drag, deltaTime);
            return angularVelocity;
        }

        private Vector3 ApplyBoundsCorrection(Vector3 force)
        {
            float outwardDot = Vector3.Dot(force, m_boundsCorrectDir);
            if (outwardDot > 0f)
            {
                float effectIntensity = 1f - (0.5f * Mathf.Cos(m_boundsEffect * Mathf.PI) + 0.5f);
                return force - (effectIntensity * m_boundsCorrectDir * outwardDot);
            }
            return force;
        }

        private void GetEngines(Inputs input, bool acceptInput, out float left, out float right, out float brake)
        {
            Vector2 v = Vector2.ClampMagnitude(new Vector2(input.H, input.V), 1);
            left = acceptInput ? Mathf.Clamp01(Mathf.Sqrt(Mathf.Clamp01(v.y) + Mathf.Clamp01(v.x))) : 0;
            right = acceptInput ? Mathf.Clamp01(Mathf.Sqrt(Mathf.Clamp01(v.y) + Mathf.Clamp01(-v.x))) : 0;
            brake = acceptInput && !input.Boost ? Mathf.Clamp01(-v.y) : 0;
        }
    }
}
