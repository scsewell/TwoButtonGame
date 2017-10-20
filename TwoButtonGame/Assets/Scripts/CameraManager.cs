using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PostProcessing;
using Framework.Interpolation;

[RequireComponent(typeof(TransformInterpolator))]
public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private Camera m_mainCam;
    [SerializeField]
    private Camera m_uiCam;

    [SerializeField]
    [Range(0, 20)]
    private float m_spliscreenMargin = 10.0f;
    [SerializeField]
    [Range(0, 20)]
    private float m_smoothness = 10.0f;
    [SerializeField]
    private Vector2 m_positionOffset = new Vector2(12, 2);
    [SerializeField]
    private float m_lookOffset = 2.0f;

    private PlayerUI m_ui;
    private TransformInterpolator m_tInterpolator;
    private Player m_player;
    private int m_playerCount;
    private Vector3 m_velocity = Vector3.zero;

    private void Awake()
    {
        m_ui = GetComponentInChildren<PlayerUI>();
    }

    public void Init(Player player, int playerCount)
    {
        m_player = player;
        m_playerCount = playerCount;

        m_tInterpolator = GetComponent<TransformInterpolator>();

        SetSplitscreen(m_mainCam);
        SetSplitscreen(m_uiCam);

        PostProcessingProfile profile = m_mainCam.GetComponent<PostProcessingBehaviour>().profile;
        profile.screenSpaceReflection.enabled = SettingManager.Instance.UseSSR.Value;
        profile.motionBlur.enabled = SettingManager.Instance.UseMotionBlur.Value;

        m_ui.Init(m_uiCam, player);

        transform.position = GetPosTarget();
        transform.rotation = GetRotTarget();
        m_tInterpolator.ForgetPreviousValues();
        m_velocity = Vector3.zero;
    }

    public void MainUpdate()
    {
        if (m_player != null)
        {
            Vector3 playerVelocity = m_player.GetComponent<MemeBoots>().Velocity;

            transform.position = Vector3.SmoothDamp(transform.position, GetPosTarget(), ref playerVelocity, m_smoothness * Time.deltaTime);
            transform.rotation = GetRotTarget();
        }
    }

    private Vector3 GetPosTarget()
    {
        Vector3 forwardLook = m_player.transform.forward;
        return m_player.transform.position - m_positionOffset.x * forwardLook + m_positionOffset.y * Vector3.Cross(forwardLook, transform.right);
    }

    private Quaternion GetRotTarget()
    {
        Vector3 lookTarget = m_player.transform.position + m_lookOffset * Vector3.up;
        return Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);
    }
    
    private void Update()
    {
        m_ui.UpdateUI(m_uiCam, m_player);
    }

    private void SetSplitscreen(Camera cam)
    {
        Vector2 m = new Vector2(m_spliscreenMargin / Screen.width, m_spliscreenMargin / Screen.height) / 2;

        Rect FULL       = new Rect(0,           0,              1,              1);
        Rect TOP        = new Rect(0,           0.5f + m.y,     1,              0.5f - m.y);
        Rect BOTTOM     = new Rect(0,           0,              1,              0.5f - m.y);
        Rect TOP_L      = new Rect(0,           0.5f + m.y,     0.5f - m.x,     0.5f - m.y);
        Rect TOP_R      = new Rect(0.5f + m.x,  0.5f + m.y,     0.5f - m.x,     0.5f - m.y);
        Rect BOTTOM_L   = new Rect(0,           0,              0.5f - m.x,     0.5f - m.y);
        Rect BOTTOM_R   = new Rect(0.5f + m.x,  0,              0.5f - m.x,     0.5f - m.y);

        switch (m_playerCount)
        {
            case 1: cam.rect = FULL; break;
            case 2:
                switch (m_player.PlayerNum)
                {
                    case 0: cam.rect = TOP; break;
                    case 1: cam.rect = BOTTOM; break;
                }
                break;
            case 3:
                switch (m_player.PlayerNum)
                {
                    case 0: cam.rect = TOP; break;
                    case 1: cam.rect = BOTTOM_L; break;
                    case 2: cam.rect = BOTTOM_R; break;
                }
                break;
            case 4:
                switch (m_player.PlayerNum)
                {
                    case 0: cam.rect = TOP_L; break;
                    case 1: cam.rect = TOP_R; break;
                    case 2: cam.rect = BOTTOM_L; break;
                    case 3: cam.rect = BOTTOM_R; break;
                }
                break;
        }
    }
}
