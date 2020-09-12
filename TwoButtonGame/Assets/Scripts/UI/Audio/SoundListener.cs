using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Framework;

namespace BoostBlasters.UI
{
    /// <summary>
    /// A component that handles playing UI interaction sounds using UI events.
    /// </summary>
    public class SoundListener : MonoBehaviour,
        IPointerEnterHandler, ISelectHandler,
        IPointerExitHandler, IDeselectHandler,
        IPointerClickHandler, ISubmitHandler,
        ICancelHandler,
        IDragHandler
    {
        private Selectable m_selectable = null;
        private SoundPlayer m_player = null;

        private SoundPlayer Player
        {
            get
            {
                if (m_player == null)
                {
                    m_player = this.GetComponentInParentAny<SoundPlayer>();
                }
                return m_player;
            }
        }

        private void Awake()
        {
            m_selectable = GetComponent<Selectable>();
        }

        public void OnPointerEnter(PointerEventData e)
        {
            if (m_selectable.IsInteractable())
            {
                Player.PlaySelectSound();
            }
        }

        public void OnSelect(BaseEventData e)
        {
            Player.PlaySelectSound();
        }

        public void OnPointerExit(PointerEventData e)
        {
            if (m_selectable.IsInteractable())
            {
                Player.PlayDeselectSound();
            }
        }

        public void OnDeselect(BaseEventData e)
        {
            Player.PlayDeselectSound();
        }

        public void OnPointerClick(PointerEventData e)
        {
            if (m_selectable.IsInteractable())
            {
                Player.PlaySubmitSound();
            }
        }

        public void OnSubmit(BaseEventData e)
        {
            if (m_selectable.IsInteractable())
            {
                Player.PlaySubmitSound();
            }
        }

        public void OnCancel(BaseEventData eventData)
        {
            Player.PlayCancelSound();
        }

        public void OnDrag(PointerEventData e)
        {
        }
    }
}
