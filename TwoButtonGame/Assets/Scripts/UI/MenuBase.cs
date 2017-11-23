using UnityEngine;

public class MenuBase : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip m_selectSound;
    [SerializeField] private AudioClip m_deselectSound;
    [SerializeField] private AudioClip m_submitSound;
    [SerializeField] private AudioClip m_cancelSound;
    [SerializeField] private AudioClip m_nextMenu;
    [SerializeField] private AudioClip m_backMenu;

    private float m_lastSoundTime;
    private float m_highestPrority;

    protected virtual void Awake()
    {
        m_lastSoundTime = -1;
        m_highestPrority = -1;
    }

    public void PlaySound(AudioClip clip, int priority)
    {
        if (clip != null)
        {
            if (m_highestPrority < priority || m_lastSoundTime != Time.time)
            {
                m_lastSoundTime = Time.time;
                m_highestPrority = priority;
            }
            else if (priority < m_highestPrority && m_lastSoundTime == Time.time)
            {
                return;
            }
            AudioManager.Instance.PlaySound(clip, 1, true);
        }
    }

    public void PlaySelectSound()
    {
        PlaySound(m_selectSound, 10);
    }

    public void PlayDeselectSound()
    {
        PlaySound(m_deselectSound, 10);
    }

    public void PlaySubmitSound(AudioClip clip = null)
    {
        AudioClip clipToPlay = clip;
        if (clip == null)
        {
            clip = m_submitSound;
        }
        PlaySound(clip, 20);
    }

    public void PlayCancelSound()
    {
        PlaySound(m_cancelSound, 20);
    }

    public void PlayNextMenuSound()
    {
        PlaySound(m_nextMenu, 30);
    }

    public void PlayBackMenuSound()
    {
        PlaySound(m_backMenu, 30);
    }
}
