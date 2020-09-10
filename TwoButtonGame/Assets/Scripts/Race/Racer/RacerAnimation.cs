using UnityEngine;

using BoostBlasters.Levels.Elements;

namespace BoostBlasters.Races.Racers
{
    /// <summary>
    /// Animates a racer.
    /// </summary>
    public class RacerAnimation : MonoBehaviour
    {
        [SerializeField] Transform m_head = null;
        [SerializeField] Renderer[] m_leftBoosters = null;
        [SerializeField] Renderer[] m_rightBoosters = null;
        [SerializeField] Material m_boosterGlowLMat = null;
        [SerializeField] Material m_boosterGlowRMat = null;

        [Header("Glow")]

        [SerializeField]
        private Color m_glowCol = new Color(1, 0.334f, 0, 1);
        [SerializeField]
        private Color m_boostGlowCol = new Color(1, 0.334f, 0, 1);
        [SerializeField]
        [Range(0f, 5f)]
        private float m_boostGlow = 3.0f;
        [SerializeField]
        [Range(0f, 5f)]
        private float m_notBoostGlow = 1.0f;
        [SerializeField]
        [Range(0f, 2f)]
        private float m_boostGlowNoiseStrength = 0.1f;
        [SerializeField]
        [Range(1f, 30f)]
        private float m_boostGlowNoiseFrequency = 10.0f;

        [Header("Looking")]

        [SerializeField]
        [Range(0f, 90f)]
        private float m_lookClamp = 42.5f;
        [SerializeField]
        [Range(0.01f, 1f)]
        private float m_lookSmoothing = 0.25f;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_lookUpScale = 0.16f;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_lookDownScale = 0.33f;

        [Header("Other")]

        [SerializeField]
        [Range(0f, 1f)]
        private float m_velocityScale = 0.25f;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_velocitySmoothing = 0.125f;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_boostSmoothing = 0.125f;
        [SerializeField]
        [Range(0f, 10f)]
        private float m_idleVelThresh = 2f;

        [Header("Audio")]

        [SerializeField]
        [Range(0f, 1f)]
        private float m_engineVolume = 0.15f;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_enginePitch = 0.35f;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_engineNormVol = 0.35f;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_volumePower = 0.4f;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_boostPitch = 0.45f;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_audioSmoothing = 0.065f;

        private Racer m_racer;
        private Animator m_anim;
        private AudioSource m_audio;
        private Material m_leftGlow;
        private Material m_rightGlow;
        private Quaternion m_headRotation;
        private Vector3 m_velocity;
        private float m_leftVolume;
        private float m_rightVolume;
        private float m_pitch;

        private void Awake()
        {
            m_racer = GetComponentInParent<Racer>();
            m_anim = GetComponent<Animator>();
            m_audio = GetComponent<AudioSource>();

            m_leftGlow = InstanceSharedMat(m_boosterGlowLMat, m_leftBoosters);
            m_rightGlow = InstanceSharedMat(m_boosterGlowRMat, m_rightBoosters);
        }

        public void ResetAnimation()
        {
            m_headRotation = m_head.rotation;
            m_velocity = Vector3.zero;

            m_leftVolume = 0f;
            m_rightVolume = 0f;
            m_pitch = m_enginePitch;

            m_anim.SetFloat("VelocityX", 0f);
            m_anim.SetFloat("VelocityY", 0f);
            m_anim.SetFloat("VelocityZ", 0f);
            m_anim.SetFloat("LeftBoost", 0f);
            m_anim.SetFloat("RightBoost", 0f);
            m_anim.SetBool("IsFlying", false);

            m_leftGlow.SetColor("_EmissionColor", m_glowCol * m_notBoostGlow);
            m_rightGlow.SetColor("_EmissionColor", m_glowCol * m_notBoostGlow);
        }

        public void UpdateAnimation(RacerMovement movement)
        {
            Vector3 localVel = movement.transform.InverseTransformVector(movement.Velocity);
            localVel = localVel.normalized * SmoothMin(m_velocityScale * Mathf.Log(localVel.magnitude + 1f), 0.95f, 0.1f);

            m_velocity = Vector3.Lerp(m_velocity, localVel, 6.0f * Time.deltaTime);

            SetFloatLerp("VelocityX", m_velocity.x, m_velocitySmoothing);
            SetFloatLerp("VelocityY", m_velocity.y, m_velocitySmoothing);
            SetFloatLerp("VelocityZ", m_velocity.z, m_velocitySmoothing);

            float leftEngine = Mathf.Lerp(movement.LeftEngine, 2f, movement.BoostFactor);
            float rightEngine = Mathf.Lerp(movement.RightEngine, 2f, movement.BoostFactor);

            SetFloatMoveTowards("LeftBoost", leftEngine, m_boostSmoothing);
            SetFloatMoveTowards("RightBoost", rightEngine, m_boostSmoothing);

            m_anim.SetBool("IsFlying", !(movement.IsGrounded && movement.Velocity.magnitude < m_idleVelThresh));

            float noiseTime = m_boostGlowNoiseFrequency * Time.time;

            float leftNoiseFac = Mathf.LerpUnclamped(leftEngine, 0f, m_boostGlowNoiseStrength * Mathf.PerlinNoise(noiseTime, 0.25f));
            float rightNoiseFac = Mathf.LerpUnclamped(rightEngine, 0f, m_boostGlowNoiseStrength * Mathf.PerlinNoise(noiseTime, 0.75f));

            Color glow = Color.Lerp(m_glowCol, m_boostGlowCol, movement.BoostFactor);
            m_leftGlow.SetColor("_EmissionColor", glow * Mathf.LerpUnclamped(m_notBoostGlow, m_boostGlow, leftNoiseFac));
            m_rightGlow.SetColor("_EmissionColor", glow * Mathf.LerpUnclamped(m_notBoostGlow, m_boostGlow, rightNoiseFac));

            //m_audio.enabled = m_racer.Config.Type == RacerType.Player;
            float audioSmoothing = Time.deltaTime / m_audioSmoothing;

            float leftVolTarget = leftEngine * m_engineNormVol;
            float rightVolTarget = rightEngine * m_engineNormVol;
            m_leftVolume = Mathf.Lerp(m_leftVolume, movement.IsBoosting ? 1f : leftVolTarget, audioSmoothing) / 2f;
            m_rightVolume = Mathf.Lerp(m_rightVolume, movement.IsBoosting ? 1f : rightVolTarget, audioSmoothing) / 2f;

            float targetPitch = Mathf.Lerp(m_enginePitch, m_boostPitch, movement.BoostFactor);
            m_pitch = Mathf.Lerp(m_pitch, targetPitch, audioSmoothing);

            m_audio.volume = Mathf.Pow(m_leftVolume + m_rightVolume, m_volumePower) * m_engineVolume;
            m_audio.pitch = m_pitch;
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

        private void SetFloatLerp(string key, float value, float smoothing)
        {
            if (smoothing > 0f)
            {
                m_anim.SetFloat(key, Mathf.Lerp(m_anim.GetFloat(key), value, 3f * Time.deltaTime / smoothing));
            }
            else
            {
                m_anim.SetFloat(key, value);
            }
        }

        private void SetFloatMoveTowards(string key, float value, float smoothing)
        {
            if (smoothing > 0f)
            {
                m_anim.SetFloat(key, Mathf.MoveTowards(m_anim.GetFloat(key), value, Time.deltaTime / smoothing));
            }
            else
            {
                m_anim.SetFloat(key, value);
            }
        }
    }
}
