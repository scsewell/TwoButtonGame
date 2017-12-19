using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class MenuBase : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip m_selectSound;
    [SerializeField] private AudioClip m_deselectSound;
    [SerializeField] private AudioClip m_submitSound;
    [SerializeField] private AudioClip m_cancelSound;
    [SerializeField] private AudioClip m_openMenu;
    [SerializeField] private AudioClip m_nextMenu;
    [SerializeField] private AudioClip m_backMenu;

    private List<MenuSoundClip> m_audioBuffer;
    private List<MenuScreen> m_menuScreens;
    private MenuScreen m_targetMenu;

    private MenuScreen m_activeMenu;
    protected MenuScreen ActiveMenu { get { return m_activeMenu; } }

    private List<PlayerBaseInput> m_inputs;
    public List<PlayerBaseInput> Inputs { get { return m_inputs; } }

    public enum TransitionSound
    {
        None,
        Open,
        Next,
        Back,
    }

    protected void InitBase(List<PlayerBaseInput> inputs)
    {
        m_inputs = inputs;

        m_audioBuffer = new List<MenuSoundClip>();
        
        CustomInputModule inputModule = gameObject.AddComponent<CustomInputModule>();
        CustomInput customInput = gameObject.AddComponent<CustomInput>();
        customInput.SetInputs(inputs);
        inputModule.SetInputOverride(customInput);
        
        m_menuScreens = new List<MenuScreen>();
        GetComponentsInChildren(m_menuScreens);

        foreach (MenuScreen menu in m_menuScreens)
        {
            menu.InitMenu();
            menu.enabled = false;
        }
    }

    public void SetMenu(MenuScreen menu, TransitionSound sound = TransitionSound.Next)
    {
        MenuScreen previous = m_targetMenu;

        if (previous != menu)
        {
            m_targetMenu = menu;
            
            switch (sound)
            {
                case TransitionSound.Open: PlayOpenMenuSound(); break;
                case TransitionSound.Next: PlayNextMenuSound(); break;
                case TransitionSound.Back: PlayBackMenuSound(); break;
            }
        }
    }

    protected void UpdateBase()
    {
        m_menuScreens.ForEach(m => m.UpdateMenu());
    }

    protected void LateUpdateBase(Func<MenuScreen, bool> fullReset)
    {
        if (m_activeMenu != m_targetMenu)
        {
            MenuScreen previous = m_activeMenu;
            m_activeMenu = m_targetMenu;

            foreach (MenuScreen menu in m_menuScreens)
            {
                if (menu != m_activeMenu)
                {
                    menu.enabled = false;
                    menu.ResetMenu(fullReset(previous));
                }
            }

            EventSystem.current.SetSelectedGameObject(null);

            if (m_activeMenu != null)
            {
                m_activeMenu.enabled = true;
                m_activeMenu.ResetMenu(fullReset(previous));
            }
        }
        m_menuScreens.ForEach(m => m.UpdateGraphics());

        FlushSoundQueue();
    }
    
    protected void FlushSoundQueue()
    {
        if (m_audioBuffer.Count > 0)
        {
            m_audioBuffer = m_audioBuffer.OrderByDescending(c => c.priority).ToList();

            int prority = m_audioBuffer.First().priority;
            foreach (MenuSoundClip clip in m_audioBuffer)
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

            m_audioBuffer.Clear();
        }
    }

    private void PlaySound(AudioClip clip, float volume, int priority)
    {
        if (clip != null)
        {
            m_audioBuffer.Add(new MenuSoundClip(clip, 1, priority));
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
