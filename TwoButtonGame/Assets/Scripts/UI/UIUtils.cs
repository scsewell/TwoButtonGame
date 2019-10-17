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

                return $"{minutes.ToString()}:{seconds.ToString("D2")}:{milliseconds.ToString("D2")}";
            }
        }

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
