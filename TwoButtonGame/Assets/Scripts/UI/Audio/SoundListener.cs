using UnityEngine;
using UnityEngine.EventSystems;

namespace BoostBlasters.UI
{
    /// <summary>
    /// Generates interaction sounds based on UI element interactions.
    /// </summary>
    public class SoundListener : MonoBehaviour,
        IPointerEnterHandler, ISelectHandler,
        IPointerExitHandler, IDeselectHandler,
        IPointerClickHandler, ISubmitHandler,
        IDragHandler
    {
        private SoundPlayer m_player;

        private void Awake()
        {
            m_player = GetComponentInParent<SoundPlayer>();
        }

        public void OnPointerEnter(PointerEventData e)
        {
            m_player.PlaySelectSound();
        }

        public void OnPointerExit(PointerEventData e)
        {
            m_player.PlayDeselectSound();
        }

        public void OnSelect(BaseEventData e)
        {
            m_player.PlaySelectSound();
        }

        public void OnDeselect(BaseEventData e)
        {
            m_player.PlayDeselectSound();
        }

        public void OnPointerClick(PointerEventData e)
        {
            m_player.PlaySubmitSound();
        }

        public void OnSubmit(BaseEventData e)
        {
            m_player.PlaySubmitSound();
        }

        public void OnDrag(PointerEventData e)
        {
        }
    }
}
