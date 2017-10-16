using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Framework.Interpolation;

[RequireComponent(typeof(TransformInterpolator))]
public class MemeBoots : MonoBehaviour
{
    private PlayerInput m_input;
    private PlayerConfig m_bootConfig;
    private Rigidbody m_body;
    private CapsuleCollider m_capsule;

    public Vector3 Velocity
    {
        get { return m_body.velocity; }
    }

    private void Awake()
    {
        m_body = GetComponent<Rigidbody>();
        m_capsule = GetComponent<CapsuleCollider>();
    }

    public void Init(PlayerInput input, PlayerConfig bootConfig)
    {
        m_input = input;
        m_bootConfig = bootConfig;

        m_capsule.material = m_bootConfig.PhysicsMat;
        m_body.useGravity = false;
    }
    
    public void Move(bool acceptInput)
    {
        bool leftButton = acceptInput ? m_input.Button1 : false;
        bool rightButton = acceptInput ? m_input.Button2 : false;
        
        m_body.AddForce(m_bootConfig.GravityFac * Physics.gravity);

        Vector3 force = m_bootConfig.ForwardAccel * transform.forward + m_bootConfig.VerticalAccel * m_bootConfig.GravityFac * Vector3.up;
        Vector3 forceOffset = m_bootConfig.TurnRatio * transform.right;

        if (leftButton)
        {
            m_body.AddForceAtPosition(force, transform.position + forceOffset);
        }
        if (rightButton)
        {
            m_body.AddForceAtPosition(force, transform.position - forceOffset);
        }
    }
}
