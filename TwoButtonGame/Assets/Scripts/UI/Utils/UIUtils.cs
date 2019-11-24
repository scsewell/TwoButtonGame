using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace BoostBlasters.UI
{
    public static class UIUtils
    {
        private static StringBuilder s_builder = new StringBuilder();

        public static string FormatRaceTime(float time)
        {
            if (time < 0f)
            {
                return "-:--:--";
            }
            else
            {
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time - (minutes * 60));
                int milliseconds = Mathf.FloorToInt((time - seconds - (minutes * 60)) * 100);

                s_builder.Clear();

                s_builder.Append(minutes);
                s_builder.Append(':');
                s_builder.Append(seconds.ToString("D2"));
                s_builder.Append(':');
                s_builder.Append(milliseconds.ToString("D2"));

                return s_builder.ToString();
            }
        }

        private static FieldInfo s_joystickState = null;
        private static PropertyInfo s_internalData = null;
        private static PropertyInfo s_moveCount = null;

        /// <summary>
        /// Gets the number of times this input module has attempted to move in the current navigation direction.
        /// </summary>
        /// <param name="inputModule">The input modele.</param>
        public static int GetRepeatCount(BaseInputModule inputModule)
        {
            int moveCount = 0;

            if (inputModule is InputSystemUIInputModule inputSystem)
            {
                if (s_moveCount == null)
                {
                    s_joystickState = inputSystem.GetType().GetField("joystickState", BindingFlags.Instance | BindingFlags.NonPublic);
                    object state = s_joystickState.GetValue(inputSystem);

                    s_internalData = state.GetType().GetProperty("internalData", BindingFlags.Instance | BindingFlags.Public);
                    object internalData = s_internalData.GetValue(state);

                    s_moveCount = internalData.GetType().GetProperty("consecutiveMoveCount", BindingFlags.Instance | BindingFlags.Public);
                    moveCount = (int)s_moveCount.GetValue(internalData);
                }
                else
                {
                    moveCount = (int)s_moveCount.GetValue(s_internalData.GetValue(s_joystickState.GetValue(inputSystem)));
                }
            }

            return moveCount;
        }

        /// <summary>
        /// Cuts of the text using epllises if needed.
        /// </summary>
        /// <param name="text">The text component.</param>
        /// <param name="value"></param>
        public static void FitText(Text text, string value)
        {
            RectTransform rt = text.GetComponent<RectTransform>();

            float maxWidth = Mathf.Abs(rt.lossyScale.x * rt.rect.width);

            TextGenerationSettings settings = text.GetGenerationSettings(Vector2.zero);
            string name = value;
            int length = name.Length;

            while (length > 0 && text.cachedTextGenerator.GetPreferredWidth(name, settings) > maxWidth)
            {
                length--;
                name = name.Substring(0, length) + "...";
            }

            text.text = name;
        }
    }
}
