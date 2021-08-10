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
    public class CountdownState : RaceState
    {
        [SerializeField]
        [Tooltip("The duration in seconds of the fade in when starting the countdown.")]
        [Range(0.01f, 5f)]
        private float m_fadeInDuration = 1.0f;

        [SerializeField]
        [Tooltip("The number of seconds in race countdown.")]
        [Range(0, 10)]
        private int m_countdownDuration = 4;

        [SerializeField]
        [Tooltip("Adjusts how fast the race countdown is.")]
        [Range(0.5f, 5f)]
        private float m_countdownScale = 1.15f;

        [SerializeField]
        [Tooltip("The sound played when the countdown decrements.")]
        private AudioClip m_countdownSound = null;

        [SerializeField]
        [Tooltip("The sound played when the race begins.")]
        private AudioClip m_goSound = null;


        private float m_startTime;
        private float m_endTime;
        private int m_countdown;


        public override void OnEnter(RaceState previousState)
        {
            m_startTime = Time.time;
            m_endTime = m_startTime + m_fadeInDuration + (m_countdownScale * m_countdownDuration);
            m_countdown = 0;
        }

        public override RaceState GetNextState()
        {
            if (Time.time >= m_endTime)
            {
                return StateMachine.GetState<MainState>();
            }
            return this;
        }

        public override void OnUpdate()
        {
            var countdown = Mathf.CeilToInt((m_endTime - Time.time) / m_countdownScale);

            if (m_countdown != countdown)
            {
                m_countdown = countdown;

                if (countdown == 0)
                {
                    AudioManager.Instance.PlaySound(m_goSound);
                }
                else if (countdown <= 3)
                {
                    AudioManager.Instance.PlaySound(m_countdownSound);
                }
            }
        }

        public override float GetScreenFade()
        {
            return FadeUtils.FadeIn(m_startTime, m_fadeInDuration);
        }
    }
}
