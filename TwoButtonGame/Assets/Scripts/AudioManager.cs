using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class AudioManager : ComponentSingleton<AudioManager>
{
    private AudioSource m_audio;

    private float m_volume = 1;
    public float Volume
    {
        get { return m_volume; }
        set
        {
            m_volume = Mathf.Clamp01(value);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        
        m_audio = gameObject.AddComponent<AudioSource>();
        m_audio.playOnAwake = false;
    }

    public void PlaySound(AudioClip clip, float volume = 1)
    {
        if (clip != null && volume > 0)
        {
            m_audio.PlayOneShot(clip, volume);
        }
    }

    public void Update()
    {
        AudioListener.volume = Volume * (SettingManager.Instance.Volume.Value / 100.0f);
    }
}
