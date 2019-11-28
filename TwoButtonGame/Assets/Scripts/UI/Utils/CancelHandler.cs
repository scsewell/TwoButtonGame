using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BoostBlasters.UI
{
    /// <summary>
    /// Used to pick up cancel events for the menu. Should be placed on all
    /// selectables in a menu.
    /// </summary>
    public class CancelHandler : MonoBehaviour, ICancelHandler
    {
        private Selectable m_selectable = null;
        private MenuScreen m_screen = null;

        private void Awake()
        {
            m_selectable = GetComponent<Selectable>();
            m_screen = GetComponentInParent<MenuScreen>();
        }

        public void OnCancel(BaseEventData eventData)
        {
            if (m_selectable.IsInteractable())
            {
                m_screen.Back();
            }
        }
    }
}
