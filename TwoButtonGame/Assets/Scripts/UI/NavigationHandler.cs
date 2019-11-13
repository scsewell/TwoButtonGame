using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Framework.UI;

namespace BoostBlasters.UI
{
    /// <summary>
    /// Handles moving selection between selectables.
    /// </summary>
    public class NavigationHandler : MonoBehaviour, IMoveHandler
    {
        private Selectable m_selectable = null;

        private void Awake()
        {
            m_selectable = GetComponent<Selectable>();

            // we override the navigation, so make sure to disable it
            var navigation = m_selectable.navigation;
            navigation.mode = Navigation.Mode.None;
            m_selectable.navigation = navigation;
        }

        public void OnMove(AxisEventData eventData)
        {
            // figure out what selectable we should navigate to
            Selectable toSelect = null;

            switch (eventData.moveDir)
            {
                case MoveDirection.Up:      toSelect = m_selectable.navigation.selectOnUp;      break;
                case MoveDirection.Down:    toSelect = m_selectable.navigation.selectOnDown;    break;
                case MoveDirection.Left:    toSelect = m_selectable.navigation.selectOnLeft;    break;
                case MoveDirection.Right:   toSelect = m_selectable.navigation.selectOnRight;   break;
            }

            // ensure the target exists
            if (toSelect == null || !toSelect.isActiveAndEnabled)
            {
                return;
            }

            // only wrap navigation if the move input is not repeating
            if (UIHelper.Wraps(this, toSelect, eventData.moveDir) && UIUtils.GetRepeatCount(eventData.currentInputModule) > 0)
            {
                return;
            }

            // set the navigation
            eventData.selectedObject = toSelect.gameObject;
        }
    }
}
