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
    private Dictionary<Player, bool> m_playerUsed;
    private Dictionary<Player, float> m_playerToGlow;

    private void Awake()
    {
        m_playerUsed = new Dictionary<Player, bool>();
        m_playerToGlow = new Dictionary<Player, float>();

        m_mat = GetComponentInChildren<MeshRenderer>().material;
    }

    public void ResetGate()
    {
        foreach (Player player in Main.Instance.RaceManager.Players)
        {
            m_playerUsed[player] = false;
            m_playerToGlow[player] = m_glowReady;
        }
    }

    public void ResetGate(Player player)
    {
        m_playerUsed[player] = false;
    }

    public void UpdateGate()
    {
        foreach (Player player in Main.Instance.RaceManager.Players)
        {
            float glow = m_playerToGlow[player];
            
            float glowTarget = m_playerUsed[player] ? m_glowUsed : m_glowReady;
            glow = Mathf.MoveTowards(glow, glowTarget, Time.deltaTime / m_glowSmoothing);
            
            m_playerToGlow[player] = glow;
        }
    }

    public bool UseGate(Player player)
    {
        bool used = m_playerUsed[player];

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
            glow = m_playerToGlow[camManager.Owner];
        }

        m_mat.SetColor("_EmissionColor", Color.red * glow);
    }
}
