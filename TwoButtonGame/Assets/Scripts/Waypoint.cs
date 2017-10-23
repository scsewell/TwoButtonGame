using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Interpolation;

[RequireComponent(typeof(TransformInterpolator))]
public class Waypoint : MonoBehaviour
{
    [Header("Glow")]
    [SerializeField]
    private int m_glowMaterialIndex = -1;
    [SerializeField] [Range(0, 5)]
    private float m_glowIntensity = 1.75f;
    [SerializeField] [Range(0, 2)]
    private float m_glowChangeTime = 0.5f;

    [Header("Bobbing")]
    [SerializeField] [Range(0, 1)]
    private float m_bobIntensity = 0.01f;
    [SerializeField] [Range(0, 10)]
    private float m_bobFrequency = 0.15f;
    [SerializeField] [Range(0, 1)]
    private float m_bobFrequencyVariance = 0.1f;
    
    private Material m_glowMat;
    private Dictionary<Player, float> m_playerToGlow = new Dictionary<Player, float>();
    private float m_bobCycleOffset;
    private float m_bobFreq;

    public Vector3 Position
    {
        get { return transform.position; }
    }

    private void Awake()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && m_glowMaterialIndex >= 0)
        {
            m_glowMat = renderer.materials[m_glowMaterialIndex];
        }

        m_bobCycleOffset = Random.value;
        m_bobFreq = m_bobFrequency * Random.Range(1 - m_bobFrequencyVariance, 1 + m_bobFrequencyVariance);
    }
    
    private void FixedUpdate()
    {
        Vector3 pos = transform.position;
        pos.y += ((100 * m_bobIntensity * m_bobFreq) * Mathf.Sin((Time.time * m_bobFreq + m_bobCycleOffset) * (2 * Mathf.PI))) * Time.deltaTime;
        transform.position = pos;
    }

    private void OnWillRenderObject()
    {
        // make the waypoint glow only for a player's camera if this is the next waypoint
        CameraManager camManager = Camera.current.GetComponent<CameraManager>();

        Color diffuse = new Color(0.2f, 0.2f, 0.2f);
        Color glow = Color.black;

        if (camManager != null && m_glowMat != null)
        {
            Player player = camManager.Owner;

            float currentGlowStrength;
            if (!m_playerToGlow.TryGetValue(player, out currentGlowStrength))
            {
                currentGlowStrength = 0;
            }

            bool isNextWaypoint = (player.CurrentWaypoint == this);

            float glowTarget = isNextWaypoint ? 1 : 0;
            if (m_glowChangeTime > 0)
            {
                currentGlowStrength = Mathf.MoveTowards(currentGlowStrength, glowTarget, Time.deltaTime / m_glowChangeTime);
            }
            else
            {
                currentGlowStrength = glowTarget;
            }

            diffuse = Color.Lerp(diffuse, player.GetColor(), currentGlowStrength);
            glow = Color.Lerp(glow, m_glowIntensity * player.GetColor(), currentGlowStrength);

            m_playerToGlow[player] = currentGlowStrength;
        }

        m_glowMat.SetColor("_Color", diffuse);
        m_glowMat.SetColor("_EmissionColor", glow);
    }
}
