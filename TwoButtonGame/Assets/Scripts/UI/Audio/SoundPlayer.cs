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
        /// <summary>
        /// How many frames the sounds are delayed to ensure that low priority
        /// sounds are not played near a higher priority sound.
        /// </summary>
        private const int FRAME_BUFFER_COUNT = 1;


        [SerializeField]
        [Tooltip("The asset that configures which sounds are used for UI events in the menu.")]
        private MenuSoundConfig m_config = null;

        private struct SoundClip
        {
            public AudioClip clip;
            public float volume;
            public int priority;
            public int frameCount;
        }

        private readonly List<SoundClip> m_audioBuffer = new List<SoundClip>();


        /// <summary>
        /// Plays the highest priority sounds submitted since the last call to this method.
        /// </summary>
        public void FlushSoundQueue()
        {
            if (m_audioBuffer.Count > 0)
            {
                // sort by decending proprity
                m_audioBuffer.Sort((x, y) => -x.priority.CompareTo(y.priority));

                // Remove all clips with lower priority than the highest proprity sound
                // and play sounds if they are the highest priority sounds in the last 
                // few frames.
                var highestPrority = m_audioBuffer[0].priority;

                for (var i = 0; i < m_audioBuffer.Count;)
                {
                    var clip = m_audioBuffer[i];

                    if (clip.priority < highestPrority)
                    {
                        m_audioBuffer.RemoveRange(i, m_audioBuffer.Count - i);
                        break;
                    }
                    else if (clip.frameCount == FRAME_BUFFER_COUNT)
                    {
                        AudioManager.Instance.PlaySound(clip.clip, clip.volume, true);
                        m_audioBuffer.RemoveAt(i);
                    }
                    else
                    {
                        clip.frameCount++;
                        m_audioBuffer[i] = clip;
                        i++;
                    }
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
            if (config != null && config.Clip != null)
            {
                m_audioBuffer.Add(new SoundClip
                {
                    clip = config.Clip,
                    volume = config.Volume * volume,
                    priority = priority,
                    frameCount = 0,
                });
            }
        }
    }
}
