using UnityEngine;
using UnityEngine.EventSystems;

public class SoundListener : MonoBehaviour,
    IPointerEnterHandler, ISelectHandler, 
    IPointerExitHandler, IDeselectHandler, 
    IPointerClickHandler, ISubmitHandler, 
    IDragHandler
{
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
        m_menu.PlaySubmitSound();
    }

    public void OnSubmit(BaseEventData e)
    {
        m_menu.PlaySubmitSound();
    }

    public void OnDrag(PointerEventData e)
    {
    }
}
