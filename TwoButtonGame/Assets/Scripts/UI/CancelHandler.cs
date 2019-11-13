using UnityEngine;
using UnityEngine.EventSystems;

namespace BoostBlasters.UI
{
    /// <summary>
    /// Used to pick up cancel events for the menu. Should be placed on all
    /// selectables in a menu.
    /// </summary>
    public class CancelHandler : MonoBehaviour, ICancelHandler
    {
        private MenuScreen m_screen = null;

        private void Awake()
        {
            m_screen = GetComponentInParent<MenuScreen>();
        }

        public void OnCancel(BaseEventData eventData)
        {
            m_screen.Back();
        }
    }
}
