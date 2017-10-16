using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVScroll : MonoBehaviour
{
    [SerializeField] [Range(0, 2)]
    private float m_speed = 1.0f;
    [SerializeField] [Range(0, 360)]
    private float m_angle = 45;

    private Renderer m_renderer;
    private Vector2 m_offset = Vector2.zero;
    
    private void Awake()
    {
        m_renderer = GetComponent<Renderer>();
    }
    
    private void Update()
    {
        m_offset += m_speed * Time.deltaTime * new Vector2(Mathf.Cos(Mathf.Deg2Rad * m_angle), Mathf.Sin(Mathf.Deg2Rad * m_angle));
        m_renderer.material.SetTextureOffset("_MainTex", m_offset);
    }
}
