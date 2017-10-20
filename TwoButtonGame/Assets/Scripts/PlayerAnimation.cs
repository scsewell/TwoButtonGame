using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] [Range(0, 1)]
    private float m_velocityScale = 0.25f;
    [SerializeField] [Range(0, 1)]
    private float m_velocitySmoothing = 0.125f;
    [SerializeField] [Range(0, 10)]
    private float m_idleVelThresh = 2f;

    private Animator m_anim;

    private void Awake()
    {
        m_anim = GetComponent<Animator>();
    }
    
    public void UpdateAnimation(MemeBoots movement)
    {
        Vector3 localVel = movement.transform.InverseTransformVector(movement.Velocity);

        localVel = localVel.normalized * SmoothMin(m_velocityScale * Mathf.Log(localVel.magnitude + 1), 0.95f, 0.1f);

        SetFloatSmooth("VelocityX", localVel.x, m_velocitySmoothing);
        SetFloatSmooth("VelocityY", localVel.y, m_velocitySmoothing);
        SetFloatSmooth("VelocityZ", localVel.z, m_velocitySmoothing);

        m_anim.SetBool("IsFlying", !(movement.IsGrounded && movement.Velocity.magnitude < m_idleVelThresh));
    }

    private float SmoothMin(float a, float b, float k)
    {
        float h = Mathf.Clamp01(0.5f + 0.5f * (a - b) / k);
        return Mathf.Lerp(a, b, h) - (k * h * (1.0f - h));
    }

    private void SetFloatSmooth(string key, float value, float smoothing)
    {
        if (smoothing > 0)
        {
            m_anim.SetFloat(key, Mathf.MoveTowards(m_anim.GetFloat(key), value, Time.deltaTime / smoothing));
        }
        else
        {
            m_anim.SetFloat(key, value);
        }
    }
}
