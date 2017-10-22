﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] Transform m_head;
    [SerializeField] Renderer[] m_leftBoosters;
    [SerializeField] Renderer[] m_rightBoosters;
    [SerializeField] Material m_boosterGlowLMat;
    [SerializeField] Material m_boosterGlowRMat;

    [SerializeField] [Range(0, 5)]
    private float m_boostGlow = 1.75f;
    [SerializeField] [Range(0, 5)]
    private float m_notBoostGlow = 1.0f;

    [SerializeField] [Range(0, 1)]
    private float m_velocityScale = 0.25f;
    [SerializeField] [Range(0, 1)]
    private float m_velocitySmoothing = 0.125f;
    [SerializeField] [Range(0, 1)]
    private float m_boostSmoothing = 0.125f;
    [SerializeField] [Range(0, 10)]
    private float m_idleVelThresh = 2f;

    [SerializeField] [Range(0, 90)]
    private float m_lookClamp = 42.5f;
    [SerializeField] [Range(0.01f, 1)]
    private float m_lookSmoothing = 0.25f;
    [SerializeField] [Range(0, 1)]
    private float m_lookUpScale = 0.16f;
    [SerializeField] [Range(0, 1)]
    private float m_lookDownScale = 0.33f;

    private Animator m_anim;
    private Material m_leftGlow;
    private Material m_rightGlow;
    private Color m_emissionColor;
    private Quaternion m_headRotation;

    private void Awake()
    {
        m_anim = GetComponent<Animator>();

        m_emissionColor = m_boosterGlowLMat.GetColor("_EmissionColor");
        m_leftGlow = InstanceSharedMat(m_boosterGlowLMat, m_leftBoosters);
        m_rightGlow = InstanceSharedMat(m_boosterGlowRMat, m_rightBoosters);
    }
    
    public void UpdateAnimation(MemeBoots movement)
    {
        Vector3 localVel = movement.transform.InverseTransformVector(movement.Velocity);
        localVel = localVel.normalized * SmoothMin(m_velocityScale * Mathf.Log(localVel.magnitude + 1), 0.95f, 0.1f);

        SetFloatSmooth("VelocityX", localVel.x, m_velocitySmoothing);
        SetFloatSmooth("VelocityY", localVel.y, m_velocitySmoothing);
        SetFloatSmooth("VelocityZ", localVel.z, m_velocitySmoothing);

        float leftBoost = movement.LeftBoost ? 1 : 0;
        float rightBoost = movement.RightBoost ? 1 : 0;

        SetFloatSmooth("LeftBoost", leftBoost, m_boostSmoothing);
        SetFloatSmooth("RightBoost", rightBoost, m_boostSmoothing);

        m_anim.SetBool("IsFlying", !(movement.IsGrounded && movement.Velocity.magnitude < m_idleVelThresh));

        m_leftGlow.SetColor("_EmissionColor", m_emissionColor * Mathf.Lerp(m_notBoostGlow, m_boostGlow, leftBoost));
        m_rightGlow.SetColor("_EmissionColor", m_emissionColor * Mathf.Lerp(m_notBoostGlow, m_boostGlow, rightBoost));
    }

    public void LateUpdateAnimation(Waypoint nextWaypoint)
    {
        Quaternion headRot;
        if (nextWaypoint != null)
        {
            Vector3 disp = nextWaypoint.Position - m_head.position;

            float horzontalDisp = Mathf.Sqrt((disp.x * disp.x) + (disp.z * disp.z));
            disp.y = Mathf.Clamp(disp.y, -horzontalDisp * m_lookDownScale, horzontalDisp * m_lookUpScale);

            if (disp.magnitude < 0.01f)
            {
                disp = m_headRotation * Vector3.forward;
            }

            Quaternion targetRot = Quaternion.LookRotation(disp, m_head.up);
            headRot = Quaternion.RotateTowards(m_head.rotation, targetRot, m_lookClamp);
        }
        else
        {
            headRot = m_head.rotation;
        }

        m_head.rotation = Quaternion.Slerp(m_headRotation, headRot, Time.deltaTime / m_lookSmoothing);
        m_headRotation = m_head.rotation;
    }

    private Material InstanceSharedMat(Material mat, Renderer[] renderers)
    {
        Material instance = new Material(mat);
        foreach (Renderer r in renderers)
        {
            for (int i = 0; i < r.sharedMaterials.Length; i++)
            {
                Material[] mats = r.sharedMaterials;
                if (mats[i] == mat)
                {
                    mats[i] = instance;
                    r.sharedMaterials = mats;
                    break;
                }
            }
        }
        return instance;
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
