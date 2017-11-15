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
    private CinemachineVirtualCamera[] m_virtualCams;
    private ICinemachineCamera m_lastActiveCam;
    private float m_viewSizeStart;
    private float m_viewSizeEnd;
    private float m_viewSizeDuration;
    private float m_viewSizeChangeTime;

    private void Awake()
    {
        m_camera = GetComponentInChildren<Camera>();
        m_brain = m_camera.GetComponent<CinemachineBrain>();
        
        SettingManager.Instance.ConfigureCamera(m_camera, true);
        m_camera.GetComponent<CinemachinePostFX>().m_Profile = m_camera.GetComponent<PostProcessingBehaviour>().profile;

        m_virtualCams = FindObjectsOfType<CinemachineVirtualCamera>();
        foreach (CinemachineVirtualCamera virtualCam in m_virtualCams)
        {
            virtualCam.m_Lens.FieldOfView = ToFOV(Random.Range(m_minViewSize, m_maxViewSize), 100);
            virtualCam.gameObject.transform.SetParent(transform);
        }

        GetComponent<CinemachineClearShot>().m_ChildCameras = m_virtualCams;

        m_brain.m_CameraCutEvent.AddListener(() => PickNewSettings());

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
            float distance = Mathf.Sqrt(Vector3.Distance(target.position, virtualCam.transform.position));
            virtualCam.m_Lens.FieldOfView = ToFOV(Mathf.Lerp(m_viewSizeStart, m_viewSizeEnd, 1 - (0.5f * Mathf.Cos(Mathf.Clamp01((Time.time - m_viewSizeChangeTime) / m_viewSizeDuration) * Mathf.PI) + 0.5f)), distance);
        }
    }

    private void PickNewSettings()
    {
        m_viewSizeChangeTime = Time.time;

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
            if (!CinemachineCore.Instance.IsLive(virtualCam))
            {
                m_lastActiveCam = m_brain.ActiveVirtualCamera;
                virtualCam.m_Lens.FieldOfView = ToFOV(m_viewSizeStart, 100);
            }
        }
    }

    private float ToFOV(float viewSize, float distance)
    {
        return 2 * Mathf.Atan(viewSize / (2 * distance)) * (180 / Mathf.PI);
    }
}
