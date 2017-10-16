using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class AudioManager : ComponentSingleton<AudioManager>
{
    private AudioSource m_audio;

    protected override void Awake()
    {
        base.Awake();
        
        gameObject.AddComponent<AudioListener>();

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
}
