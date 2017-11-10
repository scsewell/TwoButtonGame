using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Framework.Interpolation;

[RequireComponent(typeof(TransformInterpolator))]
public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private Camera m_mainCam;
    [SerializeField]
    private Camera m_uiCam;

    [SerializeField]
    private LayerMask m_blockingLayers = Physics.DefaultRaycastLayers;
    [SerializeField]
    private float m_lookOffset = 2.0f;
    [SerializeField]
    private float m_softLookOffset = 0.45f;
    [SerializeField] [Range(0.01f, 1)]
    private float m_softLookRadius = 0.35f;
    [SerializeField] [Range(0, 20)]
    private float m_softLookDistance = 5.0f;
    [SerializeField] [Range(0.01f, 1)]
    private float m_softLookSmoothing = 0.2f;
    [SerializeField]
    private Vector2 m_positionOffset = new Vector2(12, 2);
    [SerializeField] [Range(0, 20)]
    private float m_softBlockDistance = 12.0f;
    [SerializeField] [Range(0, 20)]
    private float m_softBlockMinDistance = 1.0f;
    [SerializeField] [Range(0.01f, 1)]
    private float m_softBlockSmoothing = 0.25f;
    [SerializeField] [Range(0, 10)]
    private float m_hSmoothness = 2.0f;
    [SerializeField] [Range(0, 10)]
    private float m_vSmoothness = 4.0f;

    [Header("Dust Particles")]
    [SerializeField] [Range(0, 1)]
    private float m_dustAlpha = 0.1f;
    [SerializeField] [Range(1, 200)]
    private float m_dustMinVelocity = 5.0f;
    [SerializeField] [Range(1, 200)]
    private float m_dustMaxVelocity = 80.0f;
    [SerializeField] [Range(1, 50)]
    private float m_dustVelocityPower = 3.0f;
    [SerializeField] [Range(0.01f, 1)]
    private float m_dustFadeSmoothing = 0.25f;
    
    public Camera MainCam { get { return m_mainCam; } }
    public Camera UICam { get { return m_uiCam; } }

    private Player m_player;
    public Player Owner { get { return m_player; } }

    public int PlayerMainLayer { get { return m_player.PlayerNum + 8; } }
    public int PlayerUILayer { get { return m_player.PlayerNum + 16; } }

    private TransformInterpolator m_tInterpolator;
    private ParticleSystem m_dustParticles;
    private RaycastHit[] m_hits = new RaycastHit[20];
    private List<Vector3> m_blockHits = new List<Vector3>();
    private float m_blockSmooth = 1;
    private float m_lookSmooth = 1;
    private float m_dustFade = 0;

    private void Awake()
    {
        m_tInterpolator = GetComponent<TransformInterpolator>();
        m_dustParticles = GetComponentInChildren<ParticleSystem>();
    }

    public CameraManager Init(Player player, int playerCount)
    {
        m_player = player;

        Rect slipscreen = GetSplitscreen(player.PlayerNum, playerCount);
        m_mainCam.rect = slipscreen;
        m_uiCam.rect = slipscreen;

        SettingManager.Instance.ConfigureCamera(m_mainCam, true);

        m_mainCam.cullingMask |= (1 << PlayerMainLayer);
        m_uiCam.cullingMask |= (1 << PlayerUILayer);

        m_dustParticles.gameObject.layer = PlayerMainLayer;

        transform.position = GetPosTarget();
        transform.rotation = GetRotTarget(GetLookTarget());
        m_tInterpolator.ForgetPreviousValues();

        return this;
    }

    public void SetCameraEnabled(bool enabled)
    {
        m_mainCam.enabled = enabled;
        m_uiCam.enabled = enabled;
    }

    public void UpdateCamera()
    {
        if (m_player != null)
        {
            float targetDustFade = Mathf.Pow(Mathf.Clamp01((m_player.Movement.Velocity.magnitude - m_dustMinVelocity) / m_dustMaxVelocity), m_dustVelocityPower);
            Debug.Log(targetDustFade);
            m_dustFade = Mathf.Lerp(m_dustFade, targetDustFade, Time.deltaTime / m_dustFadeSmoothing);

            Color dustColor = Color.white;
            dustColor.a = Mathf.Lerp(0, m_dustAlpha, targetDustFade);
            m_dustParticles.GetComponent<Renderer>().material.SetColor("_TintColor", dustColor);

            Vector3 targetPos = GetPosTarget();

            Vector3 vel = m_player.Movement.Velocity;
            Vector3 hPos = Vector3.SmoothDamp(transform.position, targetPos, ref vel, m_hSmoothness * Time.deltaTime);

            vel = m_player.Movement.Velocity;
            Vector3 vPos = Vector3.SmoothDamp(transform.position, targetPos, ref vel, m_vSmoothness * Time.deltaTime);

            Vector3 goalPos = new Vector3(hPos.x, vPos.y, hPos.z);
            Vector3 lookTarget = GetLookTarget();
            Vector3 disp = goalPos - lookTarget;
            float radius = m_mainCam.nearClipPlane + 0.05f;
            float softDistance = disp.magnitude + m_softBlockDistance;
            float minSmooth = m_softBlockMinDistance / disp.magnitude;

            Vector3 hitPos;
            Vector3 finalPos;

            if (IsBlocked(lookTarget, radius, disp, out hitPos, softDistance))
            {
                Vector3 hitDisp = hitPos - lookTarget;

                float fac = Mathf.Clamp01(hitDisp.magnitude / softDistance);
                m_blockSmooth = Mathf.Max(Mathf.Lerp(m_blockSmooth, fac, Time.deltaTime / m_softBlockSmoothing), minSmooth);
                Vector3 smoothPos = Vector3.Lerp(lookTarget, goalPos, m_blockSmooth);

                if (hitDisp.magnitude > (smoothPos - lookTarget).magnitude)
                {
                    finalPos = smoothPos;
                }
                else
                {
                    m_blockSmooth = Mathf.Max(hitDisp.magnitude / disp.magnitude, minSmooth);
                    finalPos = hitPos;
                }
            }
            else
            {
                m_blockSmooth = Mathf.Max(Mathf.Lerp(m_blockSmooth, 1, Time.deltaTime / m_softBlockSmoothing), minSmooth);
                finalPos = Vector3.Lerp(lookTarget, goalPos, m_blockSmooth);
            }

            transform.position = finalPos;
            transform.rotation = GetRotTarget(lookTarget);
        }
    }
    
    private Vector3 GetPosTarget()
    {
        Vector3 forwardLook = m_player.transform.forward;
        return m_player.transform.position - m_positionOffset.x * forwardLook + m_positionOffset.y * Vector3.Cross(forwardLook, transform.right);
    }

    private Quaternion GetRotTarget(Vector3 lookTarget)
    {
        return Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);
    }

    private Vector3 GetLookTarget()
    {
        Vector3 origin = m_player.transform.position + (m_softLookOffset * Vector3.up);
        Vector3 goal = m_player.transform.position + (m_lookOffset * Vector3.up);
        Vector3 disp = goal - origin;
        float softDistance = disp.magnitude + m_softLookDistance;
        float radius = m_softLookRadius;

        RaycastHit hit;
        Vector3 finalPos;

        if (Physics.SphereCast(origin, radius, disp, out hit, softDistance, m_blockingLayers))
        {
            Vector3 hitPos = hit.point + (radius * hit.normal);
            Vector3 hitDisp = hitPos - origin;

            float fac = Mathf.Clamp01(hitDisp.magnitude / softDistance);
            m_lookSmooth = Mathf.Lerp(m_lookSmooth, fac, Time.deltaTime / m_softLookSmoothing);
            Vector3 smoothPos = Vector3.Lerp(origin, goal, m_lookSmooth);

            if (hitDisp.magnitude > (smoothPos - origin).magnitude)
            {
                finalPos = smoothPos;
            }
            else
            {
                m_lookSmooth = hitDisp.magnitude / disp.magnitude;
                finalPos = hitPos;
            }
        }
        else
        {
            m_lookSmooth = Mathf.Lerp(m_lookSmooth, 1, Time.deltaTime / m_softLookSmoothing);
            finalPos = Vector3.Lerp(origin, goal, m_lookSmooth);
        }
        
        return finalPos;
    }

    private bool IsBlocked(Vector3 origin, float radius, Vector3 dir, out Vector3 hitPos, float distance)
    {
        hitPos = origin;
        
        m_blockHits.Clear();
        GetBlockingHits(origin, radius, dir, distance, false);
        GetBlockingHits(origin, radius, dir, distance, true);
        
        if (m_blockHits.Count > 0)
        {
            float min = float.MaxValue;
            foreach (Vector3 point in m_blockHits)
            {
                float dist = (point - origin).magnitude;
                if (dist < min)
                {
                    min = dist;
                    hitPos = point;
                }
            }
            return true;
        }
        return false;
    }

    private void GetBlockingHits(Vector3 origin, float radius, Vector3 dir, float distance, bool reverseDir)
    {
        Vector3 start = origin;
        if (reverseDir)
        {
            start = (distance * dir.normalized) + origin;
            dir *= -1;
        }

        int hitCount = Physics.SphereCastNonAlloc(start, radius, dir.normalized, m_hits, distance, m_blockingLayers);
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit h = m_hits[i];
            m_blockHits.Add(h.point + ((reverseDir ? -1 : 1) * radius * h.normal));
        }
    }

    public static Rect GetSplitscreen(int playerNum, int playerCount)
    {
        const float pixelMargin = 4f;
        Vector2 m = new Vector2(pixelMargin / Screen.width, pixelMargin / Screen.height) / 2;
        
        Rect FULL       = new Rect(0,           0,              1,              1);
        Rect TOP        = new Rect(0,           0.5f + m.y,     1,              0.5f - m.y);
        Rect BOTTOM     = new Rect(0,           0,              1,              0.5f - m.y);
        Rect TOP_L      = new Rect(0,           0.5f + m.y,     0.5f - m.x,     0.5f - m.y);
        Rect TOP_R      = new Rect(0.5f + m.x,  0.5f + m.y,     0.5f - m.x,     0.5f - m.y);
        Rect BOTTOM_L   = new Rect(0,           0,              0.5f - m.x,     0.5f - m.y);
        Rect BOTTOM_R   = new Rect(0.5f + m.x,  0,              0.5f - m.x,     0.5f - m.y);

        switch (playerCount)
        {
            case 1: return FULL;
            case 2:
                switch (playerNum)
                {
                    case 0: return TOP;
                    case 1: return BOTTOM;
                }
                break;
            case 3:
                switch (playerNum)
                {
                    case 0: return TOP;
                    case 1: return BOTTOM_L;
                    case 2: return BOTTOM_R;
                }
                break;
            case 4:
                switch (playerNum)
                {
                    case 0: return TOP_L;
                    case 1: return TOP_R;
                    case 2: return BOTTOM_L;
                    case 3: return BOTTOM_R;
                }
                break;
        }
        return FULL;
    }
}
