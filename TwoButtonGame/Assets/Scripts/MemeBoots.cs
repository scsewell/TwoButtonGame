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

    public Vector3 Velocity
    {
        get { return m_body.velocity; }
    }

    private bool m_isGrounded = true;
    public bool IsGrounded { get { return m_isGrounded; } }

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
    
    public void Move(bool acceptInput)
    {
        m_capsule.material = m_bootConfig.PhysicsMat;
        m_body.drag = m_bootConfig.LinearDrag;
        m_body.angularDrag = m_bootConfig.AngularDrag;

        bool leftButton = acceptInput ? m_input.Button1 : false;
        bool rightButton = acceptInput ? m_input.Button2 : false;
        
        m_body.AddForce(m_bootConfig.GravityFac * Physics.gravity);

        Vector3 force = m_bootConfig.ForwardAccel * transform.forward + m_bootConfig.VerticalAccel * m_bootConfig.GravityFac * Vector3.up;
        Vector3 forceOffset = m_bootConfig.TurnRatio * transform.right;

        if (leftButton)
        {
            AddForce(force, transform.position + forceOffset);
        }
        if (rightButton)
        {
            AddForce(force, transform.position - forceOffset);
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
}
