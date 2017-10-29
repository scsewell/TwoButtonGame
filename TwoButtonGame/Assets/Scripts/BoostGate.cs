using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostGate : MonoBehaviour
{
    [SerializeField]
    private float m_duration = 1.0f;
    public float Duration { get { return m_duration; } }

    [SerializeField]
    private Transform m_target;
    public Transform Target { get { return m_target; } }

    private void Awake()
    {

    }

    private void Update()
    {

    }
}
