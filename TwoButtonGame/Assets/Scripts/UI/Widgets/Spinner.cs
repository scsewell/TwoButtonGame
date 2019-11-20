using UnityEngine;
using UnityEngine.EventSystems;

namespace BoostBlasters.UI
{
    /// <summary>
    /// A UI widget which allows cycling between values.
    /// </summary>
    public class Spinner : ValueSelector, IMoveHandler
    {
        public void OnMove(AxisEventData eventData)
        {
            switch (eventData.moveDir)
            {
                case MoveDirection.Left: SelectIndex(GetRelativeIndex(eventData, -1)); break;
                case MoveDirection.Right: SelectIndex(GetRelativeIndex(eventData, 1)); break;
                default:
                    return;
            }
        }

        private int GetRelativeIndex(AxisEventData eventData, int offset)
        {
            // allow wrapping if this is not a repeat navigation input
            if (UIUtils.GetRepeatCount(eventData.currentInputModule) == 0)
            {
                return (CurrentIndex + Options.Length + offset) % Options.Length;
            }
            else
            {
                return Mathf.Clamp(CurrentIndex + offset, 0, Options.Length - 1);
            }
        }
    }
}
