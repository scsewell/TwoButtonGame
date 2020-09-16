using System.Collections.Generic;

using Framework;
using Framework.Audio;
using Framework.Settings;

using UnityEngine;

namespace BoostBlasters
{
    /// <summary>
    /// Controls the game audio.
    /// </summary>
    public class AudioManager : ComponentSingleton<AudioManager>
    {
        [SerializeField]
        private IntSetting m_musicVolumeSetting = null;
        [SerializeField]
        private IntSetting m_sfxVolumeSetting = null;

        private AudioSource m_audio = null;
        private AudioSource m_noPauseAudio = null;
        private MusicPlayer m_music = null;
        private readonly Dictionary<AudioClip, float> m_lastPlayTimes = new Dictionary<AudioClip, float>();

        private float m_volume = 1.0f;

        /// <summary>
        /// The master volume.
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

            if (m_music != null)
            {
                m_music.Dispose();
                m_music = null;
            }
        }

        private void Update()
        {
            m_music.Update();
            m_music.Volume = ProcessVolume(m_musicVolume, m_musicVolumeSetting);

            AudioListener.volume = Volume;
        }

        /// <summary>
        /// Plays the provided sound clip.
        /// </summary>
        /// <param name="clip">The sound clip to play.</param>
        /// <param name="volume">The volume of the sound.</param>
        /// <param name="ignorePause">Will the sound play while the game is paused.</param>
        public void PlaySound(AudioClip clip, float volume = 1f, bool ignorePause = false)
        {
            volume = ProcessVolume(volume, m_sfxVolumeSetting);

            if (clip != null && volume > 0f)
            {
                if (!m_lastPlayTimes.TryGetValue(clip, out var lastPlayTime))
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
        public void PlayMusic(Music music)
        {
            m_music.Play(music);
        }

        /// <summary>
        /// Loads then plays a music track.
        /// </summary>
        /// <param name="music">The music to play.</param>
        public async void PlayMusic(AssetBundleMusicReference music)
        {
            PlayMusic(await music.GetAsync());
        }

        /// <summary>
        /// Stops playing music.
        /// </summary>
        public void StopMusic()
        {
            m_music.Stop();
        }

        private float ProcessVolume(float volume, IntSetting setting)
        {
            return volume * Mathf.Pow(setting.Value / 100.0f, 2.0f);
        }
    }
}
