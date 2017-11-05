using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Framework.Interpolation;

[RequireComponent(typeof(TransformInterpolator))]
public class MemeBoots : MonoBehaviour
{
    [SerializeField]
    private LayerMask m_groundLayers;
    [SerializeField] [Range(0, 1)]
    private float m_minBoostTime = 0.25f;
    
    private Player m_player;
    private Rigidbody m_body;
    private CapsuleCollider m_capsule;
    private RaycastHit[] m_hits;
    private bool m_bothDoubleTap = false;
    private float m_boostDuration = 0;
    private float m_boostEndTime = float.MinValue;
    private float m_boostFactor = 0;
    
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

    private void Awake()
    {
        m_player = GetComponentInParent<Player>();
        m_body = GetComponent<Rigidbody>();
        m_capsule = GetComponent<CapsuleCollider>();

        m_hits = new RaycastHit[20];
    }

    public void UpdateMovement()
    {
        if (m_player.Input.BothDoubleTap)
        {
            m_bothDoubleTap = true;
        }
    }

    public void FixedUpdateMovement(bool acceptInput, bool inPreWarm)
    {
        PlayerConfig config = m_player.Config;

        m_capsule.material = config.PhysicsMat;
        m_body.useGravity = false;
        m_body.solverIterations = 16;
        m_body.solverVelocityIterations = 2;
        m_body.drag = config.LinearDrag;
        m_body.angularDrag = config.AngularDrag;

        m_leftEngine = acceptInput ? m_player.Input.Button2.IsDown : false;
        m_rightEngine = acceptInput ? m_player.Input.Button1.IsDown : false;
        
        if (acceptInput && !inPreWarm)
        {
            if (m_bothDoubleTap)
            {
                if (!m_isBoosting && CanBoost)
                {
                    m_isBoosting = true;
                    m_boostDuration = 0;
                }
            }
            else if (!m_player.Input.BothDoubleTapHeld)
            {
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
        
        m_boostFactor = 1 - Mathf.Clamp01((Time.time - m_boostEndTime) / 0.2f);
        
        float boostStrength = Mathf.Clamp01(Mathf.Exp(-Vector3.Dot(m_body.velocity, transform.forward) / config.BoostSoftCap));
        m_body.AddForce(m_boostFactor * config.BoostAcceleration * boostStrength * Time.deltaTime * transform.forward, ForceMode.Impulse);
        
        m_body.AddForce((1 - m_boostFactor) * config.GravityFac * Physics.gravity);

        Vector3 force = (1 - m_boostFactor) * (config.ForwardAccel * transform.forward + config.VerticalAccel * config.GravityFac * Vector3.up);
        Vector3 forceOffset = config.TurnRatio * transform.right;

        if (m_leftEngine && !inPreWarm)
        {
            AddForce(force, transform.position - forceOffset);
        }
        if (m_rightEngine && !inPreWarm)
        {
            AddForce(force, transform.position + forceOffset);
        }
        Vector3 lowerSphereCenter = transform.TransformPoint(m_capsule.center + (((m_capsule.height / 2) - m_capsule.radius) * Vector3.down));

        int hitCount = Physics.SphereCastNonAlloc(lowerSphereCenter, m_capsule.radius * 0.95f, Vector3.down, m_hits, 0.1f, m_groundLayers);
        m_isGrounded = m_hits.Take(hitCount).Any(h => Mathf.Acos(Vector3.Dot(h.normal, Vector3.up)) * Mathf.Rad2Deg < 10.0f);

        m_bothDoubleTap = false;
    }

    private void AddForce(Vector3 force, Vector3 position)
    {
        m_body.AddForce(force);
        m_body.AddTorque(Vector3.Cross(position - m_body.position, force));
    }
}
