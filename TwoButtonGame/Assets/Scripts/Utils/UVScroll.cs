using UnityEngine;

namespace BoostBlasters
{
    public class UVScroll : MonoBehaviour
    {
        [SerializeField]
        [Range(0f, 2f)]
        private float m_speed = 1.0f;

        [SerializeField]
        [Range(0f, 360f)]
        private float m_angle = 45f;

        private Renderer m_renderer = null;
        private Vector2 m_offset = Vector2.zero;

        private void Awake()
        {
            m_renderer = GetComponent<Renderer>();
        }

        private void Update()
        {
            m_offset += (m_speed * Time.deltaTime) * new Vector2(
                Mathf.Cos(Mathf.Deg2Rad * m_angle),
                Mathf.Sin(Mathf.Deg2Rad * m_angle)
            );
            m_renderer.material.SetTextureOffset("_MainTex", m_offset);
        }
    }
}
