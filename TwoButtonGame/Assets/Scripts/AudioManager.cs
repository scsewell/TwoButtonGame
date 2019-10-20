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
        private AudioSource m_audio = null;
        private AudioSource m_noPauseAudio = null;
        private MusicPlayer m_music = null;
        private readonly Dictionary<AudioClip, float> m_lastPlayTimes = new Dictionary<AudioClip, float>();

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

        /// <summary>
        /// Determines if the music stops while the game is paused.
        /// </summary>
        public bool MusicPausable
        {
            get => m_music.Pausable;
            set => m_music.Pausable = value;
        }

        protected override void Awake()
        {
            base.Awake();

            gameObject.AddComponent<AudioListener>();

            AudioListener.volume = 0f;

            m_audio = gameObject.AddComponent<AudioSource>();
            m_audio.playOnAwake = false;

            m_noPauseAudio = gameObject.AddComponent<AudioSource>();
            m_noPauseAudio.ignoreListenerPause = true;
            m_noPauseAudio.playOnAwake = false;

            m_music = new MusicPlayer(gameObject, false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            m_music.Dispose();
        }

        private void Update()
        {
            m_music.Update();
            m_music.Volume = m_musicVolume * Mathf.Pow(SettingManager.Instance.MusicVolume.Value / 100.0f, 2.0f);
            AudioListener.volume = Volume * Mathf.Pow(SettingManager.Instance.MasterVolume.Value / 100.0f, 2.0f);
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
        /// Plays a music track.
        /// </summary>
        /// <param name="music">The music to play.</param>
        public void PlayMusic(MusicParams music)
        {
            m_music.Play(music);
        }

        /// <summary>
        /// Stops playing music.
        /// </summary>
        public void StopMusic()
        {
            m_music.Stop();
        }
    }
}
