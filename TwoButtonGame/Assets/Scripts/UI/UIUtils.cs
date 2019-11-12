using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

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
