using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Framework.Interpolation;

[RequireComponent(typeof(TransformInterpolator))]
public class MemeBoots : MonoBehaviour
{
    [SerializeField]
    private LayerMask m_groundLayers;

    private PlayerInput m_input;
    private PlayerConfig m_bootConfig;
    private Rigidbody m_body;
    private CapsuleCollider m_capsule;
    private RaycastHit[] m_hits;
    private bool m_doubleTap = false;
    private Vector3 m_boostDirection;
    private float m_boostEndTime = float.MinValue;
    private float m_boostFactor = 0;

    public Vector3 Velocity
    {
        get { return m_body.velocity; }
    }

    private bool m_isGrounded = true;
    public bool IsGrounded { get { return m_isGrounded; } }

    private bool m_leftBoost = true;
    public bool LeftBoost { get { return m_leftBoost; } }

    private bool m_rightBoost = true;
    public bool RightBoost { get { return m_rightBoost; } }

    public float BoostFactor { get { return m_boostFactor; } }

    private void Awake()
    {
        m_body = GetComponent<Rigidbody>();
        m_capsule = GetComponent<CapsuleCollider>();

        m_hits = new RaycastHit[20];
    }

    public void Init(PlayerInput input, PlayerConfig bootConfig)
    {
        m_input = input;
        m_bootConfig = bootConfig;
        
        m_body.useGravity = false;
    }

    public void UpdateMovement()
    {
        if (!m_doubleTap)
        {
            m_doubleTap = m_input.BothDoubleTap;
        }
    }

    public void Move(bool acceptInput, bool inPreWarm)
    {
        m_capsule.material = m_bootConfig.PhysicsMat;
        m_body.drag = m_bootConfig.LinearDrag;
        m_body.angularDrag = m_bootConfig.AngularDrag;

        m_leftBoost = acceptInput ? m_input.Button2.IsDown : false;
        m_rightBoost = acceptInput ? m_input.Button1.IsDown : false;

        if (m_doubleTap && acceptInput && !inPreWarm)
        {
            float boostDuration = 0.6f;
            m_boostDirection = transform.forward;
            m_boostEndTime = Time.time + boostDuration;
        }
        m_doubleTap = false;

        m_boostFactor = 1 - Mathf.Clamp01((Time.time - m_boostEndTime) / 0.25f);
        
        m_body.AddForce(m_boostDirection * m_boostFactor * 65 * Time.deltaTime, ForceMode.Impulse);
        
        m_body.AddForce((1 - m_boostFactor) * m_bootConfig.GravityFac * Physics.gravity);

        Vector3 force = (1 - m_boostFactor) * (m_bootConfig.ForwardAccel * transform.forward + m_bootConfig.VerticalAccel * m_bootConfig.GravityFac * Vector3.up);
        Vector3 forceOffset = m_bootConfig.TurnRatio * transform.right;

        if (m_leftBoost && !inPreWarm)
        {
            AddForce(force, transform.position - forceOffset);
        }
        if (m_rightBoost && !inPreWarm)
        {
            AddForce(force, transform.position + forceOffset);
        }
        
        Vector3 lowerSphereCenter = transform.TransformPoint(m_capsule.center + (((m_capsule.height / 2) - m_capsule.radius) * Vector3.down));

        int hitCount = Physics.SphereCastNonAlloc(lowerSphereCenter, m_capsule.radius * 0.95f, Vector3.down, m_hits, 0.1f, m_groundLayers);
        m_isGrounded = m_hits.Take(hitCount).Any(h => Mathf.Acos(Vector3.Dot(h.normal, Vector3.up)) * Mathf.Rad2Deg < 10.0f);
    }

    private void AddForce(Vector3 force, Vector3 position)
    {
        m_body.AddForce(force);
        m_body.AddTorque(Vector3.Cross(position - m_body.position, force));
    }

    private void OnTriggerEnter(Collider other)
    {
        BoostGate boostGate = other.GetComponentInParent<BoostGate>();
        if (boostGate != null)
        {
            m_boostDirection = (boostGate.Target != null) ? (boostGate.Target.position - transform.position).normalized : other.transform.forward;
            m_boostEndTime = Mathf.Max(Time.time + boostGate.Duration, m_boostEndTime);
        }
    }
}
