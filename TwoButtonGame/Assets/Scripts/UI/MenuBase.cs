using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuBase : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip m_selectSound;
    [SerializeField] private AudioClip m_deselectSound;
    [SerializeField] private AudioClip m_submitSound;
    [SerializeField] private AudioClip m_cancelSound;
    [SerializeField] private AudioClip m_openMenu;
    [SerializeField] private AudioClip m_nextMenu;
    [SerializeField] private AudioClip m_backMenu;
    
    private List<MenuSoundClip> m_clips = new List<MenuSoundClip>();
    
    public void PlaySound(AudioClip clip, float volume, int priority)
    {
        if (clip != null)
        {
            m_clips.Add(new MenuSoundClip(clip, 1, priority));
        }
    }
    
    protected void FlushSoundQueue()
    {
        if (m_clips.Count > 0)
        {
            m_clips = m_clips.OrderByDescending(c => c.priority).ToList();

            int prority = m_clips.First().priority;
            foreach (MenuSoundClip clip in m_clips)
            {
                if (clip.priority >= prority)
                {
                    AudioManager.Instance.PlaySound(clip.clip, clip.volume, true);
                }
                else
                {
                    break;
                }
            }

            m_clips.Clear();
        }
    }

    public void PlaySelectSound(float volume = 1f)
    {
        PlaySound(m_selectSound, volume, 10);
    }

    public void PlayDeselectSound(float volume = 1f)
    {
        PlaySound(m_deselectSound, volume, 10);
    }

    public void PlaySubmitSound(float volume = 1f, AudioClip clip = null)
    {
        if (clip == null)
        {
            clip = m_submitSound;
        }
        PlaySound(clip, volume, 20);
    }

    public void PlayCancelSound(float volume = 1f)
    {
        PlaySound(m_cancelSound, volume, 20);
    }

    public void PlayOpenMenuSound(float volume = 1f)
    {
        PlaySound(m_openMenu, volume, 30);
    }

    public void PlayNextMenuSound(float volume = 1f)
    {
        PlaySound(m_nextMenu, volume, 30);
    }

    public void PlayBackMenuSound(float volume = 1f)
    {
        PlaySound(m_backMenu, volume, 30);
    }

    private struct MenuSoundClip
    {
        public AudioClip clip;
        public float volume;
        public int priority;

        public MenuSoundClip(AudioClip clip, float volume, int priority)
        {
            this.clip = clip;
            this.volume = volume;
            this.priority = priority;
        }
    }
}
