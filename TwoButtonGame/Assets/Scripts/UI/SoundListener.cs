using UnityEngine;
using UnityEngine.EventSystems;

public class SoundListener : MonoBehaviour,
    IPointerEnterHandler, ISelectHandler, 
    IPointerExitHandler, IDeselectHandler, 
    IPointerClickHandler, ISubmitHandler, 
    IDragHandler
{
    [SerializeField] private AudioClip m_submitOverrideClip;

    private MenuBase m_menu;

	private void Awake()
    {
        m_menu = GetComponentInParent<MenuBase>();
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
        m_menu.PlaySubmitSound(1f, m_submitOverrideClip);
    }
}
