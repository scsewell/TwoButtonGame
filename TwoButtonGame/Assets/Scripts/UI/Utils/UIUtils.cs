using System.Reflection;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace BoostBlasters.UI
{
    /// <summary>
    /// Contains utility methods used by the UI.
    /// </summary>
    public static class UIUtils
    {
        private static readonly StringBuilder s_builder = new StringBuilder();

        /// <summary>
        /// Gets the time formatted as minutes:seconds:milliseconds.
        /// </summary>
        /// <param name="time">The time in seconds.</param>
        /// <returns>The formatted string.</returns>
        public static string FormatRaceTime(float time)
        {
            if (time < 0f)
            {
                return "-:--:--";
            }
            else
            {
                var minutes = Mathf.FloorToInt(time / 60f);
                var seconds = Mathf.FloorToInt(time - (minutes * 60));
                var milliseconds = Mathf.FloorToInt((time - seconds - (minutes * 60)) * 100);

                s_builder.Clear();

                s_builder.Append(minutes);
                s_builder.Append(':');
                s_builder.Append(seconds.ToString("D2"));
                s_builder.Append(':');
                s_builder.Append(milliseconds.ToString("D2"));

                return s_builder.ToString();
            }
        }

        private static FieldInfo s_navigationState = null;
        private static FieldInfo s_moveCount = null;

        /// <summary>
        /// Gets the number of times this input module has attempted to move in the current navigation direction.
        /// </summary>
        /// <param name="inputModule">The input modele.</param>
        public static int GetRepeatCount(BaseInputModule inputModule)
        {
            var moveCount = 0;

            if (inputModule is InputSystemUIInputModule inputSystem)
            {
                if (s_moveCount == null)
                {
                    s_navigationState = inputSystem.GetType().GetField("m_NavigationState", BindingFlags.Instance | BindingFlags.NonPublic);
                    var state = s_navigationState.GetValue(inputSystem);

                    s_moveCount = state.GetType().GetField("consecutiveMoveCount", BindingFlags.Instance | BindingFlags.Public);
                    moveCount = (int)s_moveCount.GetValue(state);
                }
                else
                {
                    moveCount = (int)s_moveCount.GetValue(s_navigationState.GetValue(inputSystem));
                }
            }

            return moveCount;
        }
    }
}
