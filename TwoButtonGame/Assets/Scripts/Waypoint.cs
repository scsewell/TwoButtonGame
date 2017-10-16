using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    private Collider m_collider;

    public Vector3 Position
    {
        get { return transform.position; }
    }

    private void Awake()
    {
        m_collider = GetComponent<Collider>();
        m_collider.isTrigger = true;
    }
    
    private void Update()
    {

    }
}
