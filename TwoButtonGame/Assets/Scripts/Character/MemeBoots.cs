using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Framework.Interpolation;

[RequireComponent(typeof(TransformInterpolator))]
public class MemeBoots : MonoBehaviour
{
    public const float TENSOR = 0.4f;

    [SerializeField]
    private LayerMask m_groundLayers;
    [SerializeField]
    private LayerMask m_boundsLayers;
    [SerializeField] [Range(0, 100)]
    private float m_boundsCorrectionStrength = 10.0f;
    [SerializeField] [Range(0, 20)]
    private float m_collisionTorqueIntensity = 0.0f;
    [SerializeField] [Range(0, 1)]
    private float m_minBoostTime = 0.25f;

    [Header("Visualization")]
    [SerializeField]
    private bool m_showPrediction = false;

    private Player m_player;
    private CapsuleCollider m_capsule;
    private Rigidbody m_body;
    private RaycastHit[] m_hits = new RaycastHit[20];
    private float m_boundsEffect = 0;
    private Vector3 m_boundsCorrectDir = Vector3.up;
    private float m_boostDuration = 0;
    private float m_boostEndTime = float.MinValue;
    private float m_boostFactor = 0;
    private bool m_boostPressed = false;

    private TrajectoryVisualization m_trajectoryVisualization;

    public bool CanBoost
    {
        get { return m_player.Energy >= m_player.Config.BoostEnergyUseRate * m_minBoostTime; }
    }

    private bool m_isGrounded = true;
    public bool IsGrounded { get { return m_isGrounded; } }

    private bool m_leftEngine = true;
    public bool LeftEngine { get { return m_leftEngine; } }

    private bool m_rightEngine = true;
    public bool RightEngine { get { return m_rightEngine; } }

    private bool m_isBoosting = false;
    public bool IsBoosting { get { return m_isBoosting; } }

    public float BoostFactor { get { return m_boostFactor; } }

    public Vector3 Velocity
    {
        get { return m_body.velocity; }
    }

    private float m_angVelocity = 0;
    public float AngularVelocity
    {
        get { return m_angVelocity; }
    }

    private void Awake()
    {
        m_player = GetComponentInParent<Player>();
        m_body = GetComponent<Rigidbody>();
        m_capsule = GetComponent<CapsuleCollider>();
    }

    private void OnDestroy()
    {
        if (m_trajectoryVisualization != null)
        {
            m_trajectoryVisualization.EndVisualization();
            m_trajectoryVisualization = null;
        }
    }

    public void FixedUpdateMovement(MovementInputs input, bool acceptInput, bool inPreWarm)
    {
        PlayerConfig config = m_player.Config;

        ConfigurePhysics(config);
        
        m_leftEngine = acceptInput ? input.left : false;
        m_rightEngine = acceptInput ? input.right : false;
        
        if (acceptInput && !inPreWarm)
        {
            if (input.boost)
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

            if (m_player.Energy <= 0)
            {
                m_isBoosting = false;
            }
        }
        
        Vector3 force = Vector3.zero;

        RaycastHit boundsHit;
        bool outOfBounds = Physics.Raycast(transform.position, -transform.position, out boundsHit, transform.position.magnitude, m_boundsLayers);
        if (outOfBounds)
        {
            m_boundsCorrectDir = boundsHit.normal;
            force += m_boundsCorrectionStrength * -boundsHit.normal;
        }
        m_boundsEffect = Mathf.MoveTowards(m_boundsEffect, outOfBounds ? 1 : 0, Time.deltaTime / 0.2f);

        m_boostFactor = 1 - Mathf.Clamp01((Time.time - m_boostEndTime) / 0.2f);
        float boostStrength = Mathf.Clamp01(Mathf.Exp(-Vector3.Dot(m_body.velocity, transform.forward) / config.BoostSoftCap));
        Vector3 boostForce = m_boostFactor * config.BoostAcceleration * boostStrength * transform.forward;
        force += ApplyBoundsCorrection(boostForce);

        Vector3 gravity = (1 - m_boostFactor) * config.GravityFac * Physics.gravity;
        force += ApplyBoundsCorrection(gravity);

        float torque = 0;
        if (!inPreWarm)
        {
            Vector3 engineForeForce = config.ForwardAccel * transform.forward;
            Vector3 engineUpForce = config.VerticalAccel * config.GravityFac * Vector3.up;

            Vector3 engineForce = (1 - m_boostFactor) * (engineForeForce + engineUpForce);
            Vector3 forceOffset = config.TurnRatio * transform.right;

            Vector3 linearForce = Vector3.zero;
            if (m_leftEngine)
            {
                linearForce += engineForce;
                if (!m_rightEngine)
                {
                    torque += Vector3.Dot(Vector3.Cross(-forceOffset, engineForce), Vector3.up);
                }
            }
            if (m_rightEngine)
            {
                linearForce += engineForce;
                if (!m_leftEngine)
                {
                    torque += Vector3.Dot(Vector3.Cross(forceOffset, engineForce), Vector3.up);
                }
            }

            force += ApplyBoundsCorrection(linearForce);
        }

        m_body.velocity = StepVelocity(m_body.velocity, force, m_body.mass, config.LinearDrag, Time.deltaTime);
        m_angVelocity = StepAngularVelocity(m_angVelocity, torque, TENSOR, config.AngularDrag, Time.deltaTime);
        
        transform.Rotate(Vector3.up, 180 * m_angVelocity * Time.deltaTime);

        // Check if touching the ground
        Vector3 lowerSphereCenter = transform.TransformPoint(m_capsule.center + (((m_capsule.height / 2) - m_capsule.radius) * Vector3.down));

        int hitCount = Physics.SphereCastNonAlloc(lowerSphereCenter, m_capsule.radius * 0.8f, Vector3.down, m_hits, 0.1f, m_groundLayers);

        m_isGrounded = false;
        for (int i = 0; i < hitCount; i++)
        {
            if (Mathf.Acos(Vector3.Dot(m_hits[i].normal, Vector3.up)) * Mathf.Rad2Deg < 10.0f)
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
            m_trajectoryVisualization.FixedMemeUpdateTrajectory(this, m_body.position, m_body.velocity, transform.rotation.eulerAngles.y, m_angVelocity, m_leftEngine, m_rightEngine, m_isBoosting);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (m_collisionTorqueIntensity > 0)
        {
            ContactPoint[] contacts = collision.contacts;
            float torque = 0;
            foreach (ContactPoint c in contacts)
            {
                torque += Vector3.Dot(Vector3.Cross(c.point - m_body.position, collision.impulse / contacts.Length), Vector3.up);
            }
            m_angVelocity += m_collisionTorqueIntensity * Mathf.Sign(torque) * Mathf.Min(Mathf.Abs(torque), 25) * Time.deltaTime;
        }
    }

    private void ConfigurePhysics(PlayerConfig config)
    {
        m_body.solverIterations = 12;
        m_body.solverVelocityIterations = 2;
        m_body.useGravity = false;
        m_body.drag = 0;
        m_body.angularDrag = 0;
        m_capsule.material = config.PhysicsMat;
    }
    
    public void PredictStep(int steps, List<Vector3> pos, Vector3 velocity, List<float> rot, float angularVelocity, bool leftEngine, bool rightEngine, bool boost, float deltaTime)
    {
        PlayerConfig config = m_player.Config;

        for (int i = 0; i < steps; i++)
        {
            Vector3 forward = Quaternion.Euler(0, rot.Last(), 0) * Vector3.forward;

            Vector3 force = Vector3.zero;
            float torque = 0;

            if (boost)
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

                if (leftEngine)
                {
                    force += engineForce;
                    if (!rightEngine)
                    {
                        torque += Vector3.Dot(Vector3.Cross(-forceOffset, engineForce), Vector3.up);
                    }
                }
                if (rightEngine)
                {
                    force += engineForce;
                    if (!leftEngine)
                    {
                        torque += Vector3.Dot(Vector3.Cross(forceOffset, engineForce), Vector3.up);
                    }
                }
            }

            velocity = StepVelocity(velocity, force, m_body.mass, m_player.Config.LinearDrag, deltaTime);
            angularVelocity = StepAngularVelocity(angularVelocity, torque, TENSOR, m_player.Config.AngularDrag, deltaTime);

            pos.Add(pos.Last() + (velocity * deltaTime));
            rot.Add(rot.Last() + (180 * angularVelocity * deltaTime));
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
        if (outwardDot > 0)
        {
            float effectIntensity = 1 - (0.5f * Mathf.Cos(m_boundsEffect * Mathf.PI) + 0.5f);
            return force - (effectIntensity * m_boundsCorrectDir * outwardDot);
        }
        return force;
    }
}
