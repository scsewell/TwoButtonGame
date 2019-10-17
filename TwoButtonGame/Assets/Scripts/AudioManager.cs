using System.Collections.Generic;
using UnityEngine;
using Framework;
using Framework.Audio;

namespace BoostBlasters
{
    /// <summary>
    /// Controls the game audio.
    /// </summary>
    public class AudioManager : ComponentSingleton<AudioManager>
    {
        private float m_volume = 1.0f;

        /// <summary>
        /// The global volume.
        /// </summary>
        public float Volume
        {
            get => m_volume;
            set => m_volume = Mathf.Clamp01(value);
        }

        private float m_musicVolume = 1.0f;

        /// <summary>
        /// The volume of the music tracks.
        /// </summary>
        public float MusicVolume
        {
            get => m_musicVolume;
            set => m_musicVolume = Mathf.Clamp01(value);
        }

        private bool m_musicPausable = false;

        /// <summary>
        /// Determines if the music stops while the game is paused.
        /// </summary>
        public bool MusicPausable
        {
            get => m_musicPausable;
            set => m_musicPausable = value;
        }

        private AudioSource m_audio = null;
        private AudioSource m_noPauseAudio = null;
        private readonly AudioSource[] m_musicSources = new AudioSource[2];
        private int m_lastMusicSource = 0;
        private MusicParams m_musicParams = null;
        private double m_lastLoopTime = 0.0;

        private readonly Dictionary<AudioClip, float> m_lastPlayTimes = new Dictionary<AudioClip, float>();

        protected override void Awake()
        {
            base.Awake();

            AudioListener.volume = 0f;

            m_audio = gameObject.AddComponent<AudioSource>();
            m_audio.playOnAwake = false;

            m_noPauseAudio = gameObject.AddComponent<AudioSource>();
            m_noPauseAudio.ignoreListenerPause = true;
            m_noPauseAudio.playOnAwake = false;

            for (int i = 0; i < m_musicSources.Length; i++)
            {
                m_musicSources[i] = gameObject.AddComponent<AudioSource>();
                m_musicSources[i].playOnAwake = false;
            }
        }

        private void Update()
        {
            if (m_musicParams != null)
            {
                double nextLoopTime = m_lastLoopTime + m_musicParams.LoopDuration;
                if (nextLoopTime - AudioSettings.dspTime < 1.0)
                {
                    PlayMusicScheduled(nextLoopTime);
                }
            }
        
            for (int i = 0; i < m_musicSources.Length; i++)
            {
                m_musicSources[i].ignoreListenerPause = !m_musicPausable;
            }

            for (int i = 0; i < m_musicSources.Length; i++)
            {
                m_musicSources[i].volume = m_musicVolume * Mathf.Pow((SettingManager.Instance.MusicVolume.Value / 100.0f), 2.0f);
            }

            AudioListener.volume = Volume * Mathf.Pow((SettingManager.Instance.MasterVolume.Value / 100.0f), 2.0f);
        }

        /// <summary>
        /// Plays the provided sound clip.
        /// </summary>
        /// <param name="clip">The sound clip to play.</param>
        /// <param name="volume">The volume of the sound.</param>
        /// <param name="ignorePause">Will the sound play while the game is paused.</param>
        public void PlaySound(AudioClip clip, float volume = 1f, bool ignorePause = false)
        {
            if (clip != null && volume > 0f)
            {
                if (!m_lastPlayTimes.TryGetValue(clip, out float lastPlayTime))
                {
                    lastPlayTime = -1f;
                }

                if (lastPlayTime != Time.unscaledTime)
                {
                    m_lastPlayTimes[clip] = Time.unscaledTime;

                    if (ignorePause)
                    {
                        m_noPauseAudio.PlayOneShot(clip, volume);
                    }
                    else
                    {
                        m_audio.PlayOneShot(clip, volume);
                    }
                }
            }
        }

        /// <summary>
        /// Stops all playing sounds.
        /// </summary>
        public void StopSounds()
        {
            m_audio.Stop();
            m_noPauseAudio.Stop();
        }

        /// <summary>
        /// Plays the provided music track.
        /// </summary>
        /// <param name="music"></param>
        public void PlayMusic(MusicParams music)
        {
            m_musicParams = music;
            PlayMusicScheduled(AudioSettings.dspTime + 0.01);
        }


        private void PlayMusicScheduled(double time)
        {
            int source = (m_lastMusicSource + 1) % m_musicSources.Length;
            m_musicSources[source].clip = m_musicParams.Track;
            m_musicSources[source].PlayScheduled(time);
            m_lastLoopTime = time;
            m_lastMusicSource = source;
        }

        /// <summary>
        /// Stops playing the current music track.
        /// </summary>
        public void StopMusic()
        {
            m_musicSources[0].Stop();
            m_musicSources[1].Stop();
        }
    }
}
