using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Menu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip m_selectSound;
    [SerializeField] private AudioClip m_deselectSound;
    [SerializeField] private AudioClip m_submitSound;
    [SerializeField] private AudioClip m_cancelSound;
    [SerializeField] private AudioClip m_nextMenu;
    [SerializeField] private AudioClip m_backMenu;
    
    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioManager.Instance.PlaySound(clip);
        }
    }

    public void PlaySelectSound()
    {
        PlaySound(m_selectSound);
    }

    public void PlayDeselectSound()
    {
        PlaySound(m_deselectSound);
    }

    public void PlaySubmitSound()
    {
        PlaySound(m_submitSound);
    }

    public void PlayCancelSound()
    {
        PlaySound(m_cancelSound);
    }

    public void PlayNextMenuSound()
    {
        PlaySound(m_nextMenu);
    }

    public void PlayBackMenuSound()
    {
        PlaySound(m_backMenu);
    }
}
