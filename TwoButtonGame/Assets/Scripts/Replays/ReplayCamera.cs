using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using Cinemachine;
using Cinemachine.PostFX;

public class ReplayCamera : MonoBehaviour
{
    [SerializeField] [Range(0, 20)]
    private float m_minViewSize = 1.5f;
    [SerializeField] [Range(0, 20)]
    private float m_maxViewSize = 10.0f;
    [SerializeField] [Range(0, 1)]
    private float m_viewChangeChance = 0.5f;
    [SerializeField] [Range(1, 60)]
    private float m_minViewChangeDuration = 3.0f;
    [SerializeField] [Range(1, 60)]
    private float m_maxViewChangeDuration = 30.0f;

    private Camera m_camera;
    private CinemachineBrain m_brain;
    private List<CinemachineVirtualCamera> m_virtualCams;
    private float m_viewSizeStart;
    private float m_viewSizeEnd;
    private float m_viewSizeDuration;
    private float m_viewSizeChangeTime;

    private void Awake()
    {
        m_camera = GetComponent<Camera>();
        m_brain = GetComponent<CinemachineBrain>();
        m_virtualCams = new List<CinemachineVirtualCamera>();
        
        SettingManager.Instance.ConfigureCamera(m_camera, true);
        GetComponent<CinemachinePostFX>().m_Profile = GetComponent<PostProcessingBehaviour>().profile;

        m_virtualCams.AddRange(FindObjectsOfType<CinemachineVirtualCamera>());

        m_brain.m_CameraActivatedEvent.AddListener(() => PickNewSettings());

        m_viewSizeStart = 2;
        m_viewSizeEnd = 2;
        m_viewSizeDuration = 1;
        m_viewSizeChangeTime = 0;

        Deactivate();
    }

    public void Activate()
    {
        m_camera.enabled = true;
    }

    public void Deactivate()
    {
        m_camera.enabled = false;
    }

    public void SetTarget(Transform target)
    {
        foreach (CinemachineVirtualCamera virtualCam in m_virtualCams)
        {
            virtualCam.LookAt = target;   
            virtualCam.m_Lens.FieldOfView = ToFOV(Mathf.Lerp(m_viewSizeStart, m_viewSizeEnd, 1 - (0.5f * Mathf.Cos(Mathf.Clamp01((Time.time - m_viewSizeChangeTime) / m_viewSizeDuration) * Mathf.PI) + 0.5f)), target, virtualCam.transform);
        }
    }

    private void PickNewSettings()
    {
        m_viewSizeChangeTime = Time.time;
        m_viewSizeStart = Random.Range(m_minViewSize, m_maxViewSize);

        if (Random.value < m_viewChangeChance)
        {
            m_viewSizeEnd = Random.Range(m_minViewSize, m_maxViewSize);
            m_viewSizeDuration = Random.Range(m_minViewChangeDuration, m_maxViewChangeDuration);
        }
        else
        {
            m_viewSizeEnd = m_viewSizeStart;
        }

        foreach (CinemachineVirtualCamera virtualCam in m_virtualCams)
        {
            virtualCam.m_Lens.FieldOfView = ToFOV(m_viewSizeStart, virtualCam.LookAt, virtualCam.transform);
        }
    }

    private float ToFOV(float viewSize, Transform target, Transform cam)
    {
        float distance = Mathf.Sqrt(Vector3.Distance(target.position, cam.position));
        return 2 * Mathf.Atan(viewSize / (2 * distance)) * (180 / Mathf.PI);
    }
}
