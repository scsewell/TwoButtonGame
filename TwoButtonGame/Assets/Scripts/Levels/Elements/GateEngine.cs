using UnityEngine;

namespace BoostBlasters.Levels
{
    public class GateEngine : MonoBehaviour
    {
        [SerializeField]
        private Transform m_leftEngineFacing = null;
        [SerializeField]
        private Transform m_rightEngineFacing = null;
        [SerializeField]
        private int m_leftGlowMatIndex = -1;
        [SerializeField]
        private int m_rightGlowMatIndex = -1;
        [SerializeField]
        private ParticleSystem m_leftParticles = null;
        [SerializeField]
        private ParticleSystem m_rightParticles = null;

        [Header("Effects")]

        [SerializeField]
        private Color m_glowColor = new Color(1f, 0.8010381f, 0.4117647f);
        [SerializeField]
        [Range(0f, 5f)]
        private float m_minGlowIntensity = 0.25f;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_flickerIntensity = 0.2f;
        [SerializeField]
        [Range(0f, 60f)]
        private float m_flickerFrequency = 25.0f;
        [SerializeField]
        [Range(0f, 5f)]
        private float m_flameScale = 1.0f;
        [SerializeField]
        [Range(0f, 5f)]
        private float m_accelerationScale = 1.0f;
        [SerializeField]
        [Range(0f, 5f)]
        private float m_maxAcceleration = 3.0f;

        private Material m_leftGlowMat;
        private Material m_rightGlowMat;

        private void Awake()
        {
            Material[] mats = GetComponentInChildren<MeshRenderer>().materials;
            m_leftGlowMat = mats[m_leftGlowMatIndex];
            m_rightGlowMat = mats[m_rightGlowMatIndex];
        }

        public void UpdateEngine(Vector3 acceleration)
        {
            Vector3 a = 5f * m_accelerationScale * acceleration;
            SetEngine(m_leftEngineFacing, m_leftGlowMat, m_leftParticles, a);
            SetEngine(m_rightEngineFacing, m_rightGlowMat, m_rightParticles, a);
        }

        private void SetEngine(Transform engineFacing, Material engineMat, ParticleSystem particles, Vector3 acceleration)
        {
            float fac = Mathf.Pow(Mathf.Max(Vector3.Dot(-engineFacing.forward, acceleration), 0f), 0.33f);
            float maxStrength = m_flickerIntensity * (0.5f * Mathf.Sin(2 * Mathf.PI * m_flickerFrequency * Time.time) + 0.5f) + (1f - m_flickerIntensity);
            float strength = Mathf.LerpUnclamped(0f, maxStrength, Mathf.Min(fac, m_maxAcceleration));

            engineMat.SetColor("_EmissionColor", Mathf.Max(strength, m_minGlowIntensity) * m_glowColor);

            if (particles != null)
            {
                ParticleSystem.SizeOverLifetimeModule s = particles.sizeOverLifetime;
                s.yMultiplier = m_flameScale * Mathf.Max(strength - 0.25f, 0f);

                if (s.yMultiplier == 0f)
                {
                    particles.Stop();
                }
                else if (!particles.isPlaying)
                {
                    particles.Play();
                }
            }
        }
    }
}
