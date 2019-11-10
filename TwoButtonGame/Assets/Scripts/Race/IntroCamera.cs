using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

using BoostBlasters.Levels;

namespace BoostBlasters.Races
{
    public class IntroCamera : MonoBehaviour
    {
        [SerializeField]
        private Transform m_fovBone = null;

        private Animator m_anim = null;
        private Camera m_cam = null;
        private Level m_level = null;

        private PlayableGraph m_graph;
        private PlayableOutput m_animOutput;
        private AnimationClipPlayable m_currentClip;
        private Queue<Level.CameraShot> m_shotsToPlay = null;

        /// <summary>
        /// Is the intro camera sequence currently playing.
        /// </summary>
        public bool IsPlaying => m_shotsToPlay.Count > 0 || (m_graph.IsValid() && !m_graph.IsDone());

        private void Awake()
        {
            m_anim = GetComponentInChildren<Animator>();
            m_cam = m_anim.GetComponentInChildren<Camera>();

            m_shotsToPlay = new Queue<Level.CameraShot>();
        }

        public IntroCamera Init(Level level)
        {
            m_level = level;
            return this;
        }

        private void OnDestroy()
        {
            if (m_graph.IsValid())
            {
                m_graph.Destroy();
            }
        }

        /// <summary>
        /// Gets the duration of the intro in seconds.
        /// </summary>
        /// <returns></returns>
        public float GetIntroSequenceLength()
        {
            return m_level.IntroSequence.Sum(s => s.Clip.length / s.Speed);
        }

        /// <summary>
        /// Starts playing the intro camera sequence.
        /// </summary>
        public void PlayIntroSequence()
        {
            if (m_graph.IsValid())
            {
                m_graph.Destroy();
            }

            m_graph = PlayableGraph.Create();
            m_graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            m_animOutput = AnimationPlayableOutput.Create(m_graph, "IntroAnimation", m_anim);

            foreach (Level.CameraShot shot in m_level.IntroSequence)
            {
                m_shotsToPlay.Enqueue(shot);
            }
        }

        /// <summary>
        /// Cancels the into sequence.
        /// </summary>
        public void StopIntroSequence()
        {
            if (m_graph.IsValid())
            {
                m_graph.Destroy();
            }

            m_shotsToPlay.Clear();
        }

        /// <summary>
        /// Updates the intro animation sequence.
        /// </summary>
        /// <param name="enableCamera">Should the camera render the scene.</param>
        public void UpdateCamera(bool enableCamera)
        {
            if (m_shotsToPlay.Count > 0)
            {
                if (m_shotsToPlay.Count == 0 || m_graph.IsDone())
                {
                    m_graph.Stop();
                    Level.CameraShot shot = m_shotsToPlay.Dequeue();

                    m_currentClip = AnimationClipPlayable.Create(m_graph, shot.Clip);
                    m_currentClip.SetDuration(shot.Clip.length);
                    m_currentClip.SetTime(0);
                    m_currentClip.SetSpeed(shot.Speed);

                    m_animOutput.SetSourcePlayable(m_currentClip);
                    m_graph.Play();
                }
            }

            m_cam.enabled = IsPlaying && enableCamera;
        }

        private void LateUpdate()
        {
            m_cam.fieldOfView = m_fovBone.localScale.y;
        }
    }
}
