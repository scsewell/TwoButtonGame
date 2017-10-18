using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class AudioManager : ComponentSingleton<AudioManager>
{
    private AudioSource m_audio;

    private float m_volume = 1.0f;
    public float Volume
    {
        get { return m_volume; }
        set
        {
            m_volume = Mathf.Clamp01(value);
        }
    }

    private AudioSource[] m_musicSources;
    private int m_lastMusicSource = 0;

    private MusicParams m_musicParams;
    private double m_lastLoopTime;

    private float m_musicVolume = 1.0f;
    public float MusicVolume
    {
        get { return m_musicVolume; }
        set
        {
            m_musicVolume = Mathf.Clamp01(value);
        }
    }

    protected override void Awake()
    {
        base.Awake();

        AudioListener.volume = 0;

        m_audio = gameObject.AddComponent<AudioSource>();
        m_audio.playOnAwake = false;

        m_musicSources = new AudioSource[2];
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
            if (nextLoopTime - AudioSettings.dspTime < 1)
            {
                PlayMusicScheduled(nextLoopTime);
            }
        }

        for (int i = 0; i < m_musicSources.Length; i++)
        {
            m_musicSources[i].volume = m_musicVolume * Mathf.Pow((SettingManager.Instance.MusicVolume.Value / 100.0f), 2.0f);
        }

        AudioListener.volume = Volume * Mathf.Pow((SettingManager.Instance.MasterVolume.Value / 100.0f), 2.0f);
    }

    public void PlaySound(AudioClip clip, float volume = 1)
    {
        if (clip != null && volume > 0)
        {
            m_audio.PlayOneShot(clip, volume);
        }
    }

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
}
