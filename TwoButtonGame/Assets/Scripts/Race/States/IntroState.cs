using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Framework;
using Framework.Audio;
using Framework.StateMachines;

using BoostBlasters.Profiles;
using BoostBlasters.Characters;
using BoostBlasters.Races.Racers;
using BoostBlasters.Replays;
using BoostBlasters.UI.RaceMenus;

namespace BoostBlasters.Races
{
    /// <summary>
    /// The state used to manage showing the level introduction animation.
    /// </summary>
    public class IntroState : RaceState
    {
        [SerializeField]
        [Tooltip("The duration in seconds of the fade in when starting the intro.")]
        [Range(0.01f, 5f)]
        private float m_fadeInDuration = 1.65f;

        [SerializeField]
        [Tooltip("The duration in seconds of the fade out when completing the intro.")]
        [Range(0.01f, 5f)]
        private float m_fadeOutDuration = 1.0f;


        private IntroCamera m_introCamera;
        private float m_startTime;
        private float m_endTime;


        protected override void Awake()
        {
            base.Awake();

            m_introCamera = FindObjectOfType<IntroCamera>();
        }

        public override void OnEnter(RaceState previousState)
        {
            m_startTime = Time.time;
            m_endTime = m_startTime + m_introCamera.GetIntroSequenceLength();

            m_introCamera.PlayIntroSequence();
        }

        public override void OnExit(RaceState nextState)
        {
            m_introCamera.StopIntroSequence();
        }

        public override RaceState GetNextState()
        {
            // TODO skipping
            if (Time.time >= m_endTime)
            {
                return StateMachine.GetState<CountdownState>();
            }
            return null;
        }

        public override void OnLateUpdate()
        {

        }

        public override float GetScreenFade()
        {
            return FadeUtils.CombineFadeFactors(
                FadeUtils.FadeIn(m_startTime, m_fadeInDuration),
                FadeUtils.FadeOut(m_endTime, m_fadeOutDuration)
            );
        }

        public override float GetAudioFade()
        {
            return FadeUtils.FadeIn(m_startTime, m_fadeInDuration);
        }
    }
}
