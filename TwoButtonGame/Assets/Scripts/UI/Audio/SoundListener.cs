using UnityEngine;
using UnityEngine.EventSystems;

using Framework;

namespace BoostBlasters.UI
{
    /// <summary>
    /// Generates interaction sounds based on UI element interactions.
    /// </summary>
    public class SoundListener : MonoBehaviour,
        IPointerEnterHandler, ISelectHandler,
        IPointerExitHandler, IDeselectHandler,
        IPointerClickHandler, ISubmitHandler,
        ICancelHandler,
        IDragHandler
    {
        private SoundPlayer m_player = null;
        private CancelHandler m_cancelHandler = null;

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
            m_cancelHandler = GetComponent<CancelHandler>();
        }

        public void OnPointerEnter(PointerEventData e)
        {
            Player.PlaySelectSound();
        }

        public void OnPointerExit(PointerEventData e)
        {
            Player.PlayDeselectSound();
        }

        public void OnSelect(BaseEventData e)
        {
            Player.PlaySelectSound();
        }

        public void OnDeselect(BaseEventData e)
        {
            Player.PlayDeselectSound();
        }

        public void OnPointerClick(PointerEventData e)
        {
            Player.PlaySubmitSound();
        }

        public void OnSubmit(BaseEventData e)
        {
            Player.PlaySubmitSound();
        }

        public void OnCancel(BaseEventData eventData)
        {
            // if there is custom cancel behaviour, we should not play a sound
            if (m_cancelHandler == null)
            {
                Player.PlayCancelSound();
            }
        }

        public void OnDrag(PointerEventData e)
        {
        }
    }
}
