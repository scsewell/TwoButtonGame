using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoostGate : MonoBehaviour
{
    [SerializeField]
    private float m_energy = 50.0f;
    public float Energy { get { return m_energy; } }

    [Header("Graphics")]
    [SerializeField] [Range(0, 3)]
    private float m_glowReady = 1.2f;
    [SerializeField] [Range(0, 3)]
    private float m_glowUsed = 0.4f;
    [SerializeField] [Range(0, 1)]
    private float m_glowSmoothing = 0.35f;

    private Material m_mat;
    private Dictionary<Player, bool> m_playerUsed = new Dictionary<Player, bool>();
    private Dictionary<Player, float> m_playerToGlow = new Dictionary<Player, float>();

    private void Awake()
    {
        m_mat = GetComponentInChildren<MeshRenderer>().material;
    }

    public void UpdateGate()
    {
        foreach (Player player in Main.Instance.RaceManager.Players)
        {
            float glow;
            if (!m_playerToGlow.TryGetValue(player, out glow))
            {
                glow = 0;
            }

            bool used;
            if (!m_playerUsed.TryGetValue(player, out used))
            {
                used = false;
            }
            
            float glowTarget = used ? m_glowUsed : m_glowReady;
            glow = Mathf.MoveTowards(glow, glowTarget, Time.deltaTime / m_glowSmoothing);
            
            m_playerToGlow[player] = glow;
        }
    }

    public void ResetGate(Player player)
    {
        m_playerUsed[player] = false;
    }

    public bool UseGate(Player player)
    {
        bool used;
        if (!m_playerUsed.TryGetValue(player, out used))
        {
            used = false;
        }

        if (!used)
        {
            m_playerUsed[player] = true;
        }
        return !used;
    }

    private void OnWillRenderObject()
    {
        // make the gate glow only for a player's camera if it has not been used already
        CameraManager camManager = Camera.current.GetComponentInParent<CameraManager>();
        
        float glow = m_glowReady;

        if (camManager != null)
        {
            if (!m_playerToGlow.TryGetValue(camManager.Owner, out glow))
            {
                glow = m_glowReady;
            }
        }

        m_mat.SetColor("_EmissionColor", Color.red * glow);
    }
}
