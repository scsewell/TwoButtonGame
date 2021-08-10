using UnityEngine;
using UnityEngine.Playables;

namespace BoostBlasters.Races
{
    [ExecuteAlways]
    public class IntroCamera : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The animated bone whose y-axis scale is used to store the camera's vertical field of view.")]
        private Transform m_fovBone = null;

        private PlayableDirector m_director = null;
        private Camera m_cam = null;

        private void Awake()
        {
            m_director = GetComponentInChildren<PlayableDirector>();
            m_cam = GetComponentInChildren<Camera>();

            m_director.played += OnStart;
            m_director.stopped += OnStop;
        }

        private void LateUpdate()
        {
            m_cam.fieldOfView = m_fovBone.localScale.y;
        }

        /// <summary>
        /// Gets the duration of the intro in seconds.
        /// </summary>
        public float GetIntroSequenceLength()
        {
            return (float)m_director.duration;
        }

        /// <summary>
        /// Starts playing the intro camera sequence.
        /// </summary>
        public void PlayIntroSequence()
        {
            m_director.Play();
        }

        /// <summary>
        /// Cancels the into sequence.
        /// </summary>
        public void StopIntroSequence()
        {
            m_director.Stop();
        }

        private void OnStart(PlayableDirector director)
        {
            m_cam.enabled = true;
        }

        private void OnStop(PlayableDirector director)
        {
            m_cam.enabled = false;
        }
    }
}
