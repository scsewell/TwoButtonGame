using System.Collections.Generic;

using UnityEngine;

namespace BoostBlasters.UI
{
    /// <summary>
    /// A comppnent that plays audio used by the menu system.
    /// </summary>
    /// <remarks>
    /// This limits the number of sounds playing in the menu during navigation, only playing
    /// the highest priority sounds if many sounds are played within a few frames.
    /// </remarks>
    public class SoundPlayer : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The asset that configures which sounds are used for UI events in the menu.")]
        private MenuSoundConfig m_config = null;

        [SerializeField]
        [Tooltip("How long in seconds sounds are delayed to ensure that low priority sounds are not played near a higher priority sound.")]
        [Range(0f, 0.1f)]
        private float m_bufferTime = 0.035f;


        private struct SoundClip
        {
            public AudioClip clip;
            public float volume;
            public int priority;
            public float time;
        }

        private readonly List<SoundClip> m_buffer = new List<SoundClip>();


        /// <summary>
        /// Plays the highest priority sounds submitted since the last call to this method.
        /// </summary>
        public void FlushSoundQueue()
        {
            for (var i = 0; i < m_buffer.Count;)
            {
                var sound = m_buffer[i];
                var delta = Time.unscaledTime - sound.time;

                if (delta < m_bufferTime)
                {
                    i++;
                }
                else
                {
                    AudioManager.Instance.PlaySound(sound.clip, sound.volume, true);
                    m_buffer.RemoveAt(i);
                }
            }
        }

        public void PlaySelectSound(float volume = 1f)
        {
            PlaySound(m_config.SelectSound, volume, 10);
        }

        public void PlayDeselectSound(float volume = 1f)
        {
            PlaySound(m_config.DeselectSound, volume, 10);
        }

        public void PlaySubmitSound(float volume = 1f)
        {
            PlaySound(m_config.SubmitSound, volume, 20);
        }

        public void PlayCancelSound(float volume = 1f)
        {
            PlaySound(m_config.CancelSound, volume, 20);
        }

        public void PlayOpenMenuSound(float volume = 1f)
        {
            PlaySound(m_config.OpenMenu, volume, 30);
        }

        public void PlayNextMenuSound(float volume = 1f)
        {
            PlaySound(m_config.NextMenu, volume, 30);
        }

        public void PlayBackMenuSound(float volume = 1f)
        {
            PlaySound(m_config.BackMenu, volume, 30);
        }

        private void PlaySound(MenuSoundConfig.Config config, float volume, int priority)
        {
            if (config == null || config.Clip == null)
            {
                return;
            }

            // low priority sounds are not played when there are higher priority sounds
            if (m_buffer.Count > 0)
            {
                var sound = m_buffer[0];

                if (sound.priority > priority)
                {
                    return;
                }
                else if (sound.priority < priority)
                {
                    m_buffer.Clear();
                }
            }

            // Don't play duplicates of the same clip, but ensure the loudest requested
            // volume for the clip is used.
            var vol = config.Volume * volume;

            for (var i = 0; i < m_buffer.Count; i++)
            {
                var sound = m_buffer[i];

                if (sound.clip == config.Clip)
                {
                    sound.volume = Mathf.Max(sound.volume, vol);
                    m_buffer[i] = sound;
                    return;
                }
            }

            // if the clip is unique, queue it to be played
            m_buffer.Add(new SoundClip
            {
                clip = config.Clip,
                volume = vol,
                priority = priority,
                time = Time.unscaledTime,
            });
        }
    }
}
