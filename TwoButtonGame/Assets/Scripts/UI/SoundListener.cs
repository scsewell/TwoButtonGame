﻿using UnityEngine;
using UnityEngine.EventSystems;

public class SoundListener : MonoBehaviour,
    IPointerEnterHandler, ISelectHandler, 
    IPointerExitHandler, IDeselectHandler, 
    IPointerClickHandler, ISubmitHandler, 
    IDragHandler
{
    [SerializeField] private AudioClip m_submitOverrideClip;

    private Menu m_menu;

	private void Awake()
    {
        m_menu = GetComponentInParent<Menu>();
    }

    public void OnPointerEnter(PointerEventData e)
    {
        m_menu.PlaySelectSound();
    }

    public void OnPointerExit(PointerEventData e)
    {
        m_menu.PlayDeselectSound();
    }

    public void OnSelect(BaseEventData e)
    {
        m_menu.PlaySelectSound();
    }

    public void OnDeselect(BaseEventData e)
    {
        m_menu.PlayDeselectSound();
    }

    public void OnPointerClick(PointerEventData e)
    {
        Submit();
    }

    public void OnSubmit(BaseEventData e)
    {
        Submit();
    }

    public void OnDrag(PointerEventData e)
    {
    }

    private void Submit()
    {
        if (m_submitOverrideClip != null)
        {
            m_menu.PlaySound(m_submitOverrideClip);
        }
        else
        {
            m_menu.PlaySubmitSound();
        }
    }
}
