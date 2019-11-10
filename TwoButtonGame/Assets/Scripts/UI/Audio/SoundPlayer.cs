using System.Collections.Generic;

using UnityEngine;

namespace BoostBlasters.UI
{
    /// <summary>
    /// Plays audio used by the menu system.
    /// </summary>
    public class SoundPlayer : MonoBehaviour
    {
        [SerializeField]
        private MenuSoundConfig m_config = null;

        private struct SoundClip
        {
            public readonly AudioClip clip;
            public readonly float volume;
            public readonly int priority;

            public SoundClip(AudioClip clip, float volume, int priority)
            {
                this.clip = clip;
                this.volume = volume;
                this.priority = priority;
            }
        }

        private List<SoundClip> m_audioBuffer = new List<SoundClip>();

        /// <summary>
        /// Plays the highest prioriity sounds submitted since the last flush.
        /// </summary>
        public void FlushSoundQueue()
        {
            if (m_audioBuffer.Count > 0)
            {
                // sort by decending proprity
                m_audioBuffer.Sort((x, y) => -x.priority.CompareTo(y.priority));

                int prority = m_audioBuffer[0].priority;
                foreach (SoundClip clip in m_audioBuffer)
                {
                    if (clip.priority < prority)
                    {
                        break;
                    }

                    AudioManager.Instance.PlaySound(clip.clip, clip.volume, true);
                }

                m_audioBuffer.Clear();
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

        public void PlaySubmitSound(float volume = 1f, AudioClip clip = null)
        {
            if (clip == null)
            {
                clip = m_config.SubmitSound;
            }
            PlaySound(clip, volume, 20);
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

        private void PlaySound(AudioClip clip, float volume, int priority)
        {
            if (clip != null)
            {
                m_audioBuffer.Add(new SoundClip(clip, volume, priority));
            }
        }
    }
}
