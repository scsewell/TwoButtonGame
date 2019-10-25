using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;
using BoostBlasters.Character;

namespace BoostBlasters.Replays
{
    /// <summary>
    /// Manages the camera used during replays.
    /// </summary>
    public class ReplayCamera : MonoBehaviour
    {
        [SerializeField]
        [Range(0, 20)]
        private float m_minViewSize = 1.5f;
        [SerializeField]
        [Range(0, 20)]
        private float m_maxViewSize = 10.0f;
        [SerializeField]
        [Range(0, 1)]
        private float m_viewChangeChance = 0.5f;
        [SerializeField]
        [Range(1, 60)]
        private float m_minViewChangeDuration = 3.0f;
        [SerializeField]
        [Range(1, 60)]
        private float m_maxViewChangeDuration = 30.0f;

        private Camera m_camera;
        private CinemachineBrain m_brain;
        private CinemachineVirtualCamera[] m_virtualCams;
        private float m_viewSizeStart;
        private float m_viewSizeEnd;
        private float m_viewSizeDuration;
        private float m_viewSizeChangeTime;

        private void Awake()
        {
            m_camera = GetComponentInChildren<Camera>();
            m_brain = m_camera.GetComponent<CinemachineBrain>();

            m_virtualCams = FindObjectsOfType<CinemachineVirtualCamera>();
            foreach (CinemachineVirtualCamera virtualCam in m_virtualCams)
            {
                virtualCam.m_Lens.FieldOfView = ToFOV(Random.Range(m_minViewSize, m_maxViewSize), 100f);
                virtualCam.gameObject.transform.SetParent(transform);
            }

            m_brain.m_CameraCutEvent.AddListener((brain) => PickNewSettings());

            m_viewSizeStart = 2f;
            m_viewSizeEnd = 2f;
            m_viewSizeDuration = 1f;
            m_viewSizeChangeTime = 0f;

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

        public void SetTarget(List<Player> players)
        {
            if (players.Any(p => !p.RaceResult.Finished))
            {
                Transform replayTarget = players.Where(p => !p.RaceResult.Finished).OrderBy(p => p.RaceResult.Rank).First().transform;

                foreach (CinemachineVirtualCamera virtualCam in m_virtualCams)
                {
                    virtualCam.LookAt = replayTarget;
                }
            }

            foreach (CinemachineVirtualCamera virtualCam in m_virtualCams)
            {
                float distance = Mathf.Sqrt(Vector3.Distance(virtualCam.LookAt.position, virtualCam.transform.position));

                if (!CinemachineCore.Instance.IsLive(virtualCam))
                {
                    float fac = 1f - (0.5f * Mathf.Cos(Mathf.Clamp01((Time.time - m_viewSizeChangeTime) / m_viewSizeDuration) * Mathf.PI) + 0.5f);
                    virtualCam.m_Lens.FieldOfView = ToFOV(Mathf.Lerp(m_viewSizeStart, m_viewSizeEnd, fac), distance);
                }
            }
        }

        private void PickNewSettings()
        {
            m_viewSizeStart = Random.Range(m_minViewSize, m_maxViewSize);
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
                    virtualCam.m_Lens.FieldOfView = ToFOV(m_viewSizeStart, 100f);
                }
            }
        }

        private float ToFOV(float viewSize, float distance)
        {
            return 2f * Mathf.Atan(viewSize / (2f * distance)) * (180f / Mathf.PI);
        }
    }
}
