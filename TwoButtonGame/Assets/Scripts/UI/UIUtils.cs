using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public static class UIUtils
{
    public static string FormatRaceTime(float time)
    {
        if (time < 0)
        {
            return "-:--:--";
        }
        else
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time - (minutes * 60));
            int milliseconds = Mathf.FloorToInt((time - seconds - (minutes * 60)) * 100);
            return string.Format(minutes.ToString() + ":" + seconds.ToString("D2") + ":" + milliseconds.ToString("D2"));
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

    public static bool IsAlphaNumeric(char c)
    {
        if ('A' <= c && c <= 'Z') { return true; };
        if ('a' <= c && c <= 'z') { return true; };
        if ('0' <= c && c <= '9') { return true; }
        return false;
    }

    public static bool IsAlphaNumeric(KeyCode c)
    {
        if (KeyCode.A <= c && c <= KeyCode.Z) { return true; };
        if (KeyCode.Alpha0 <= c && c <= KeyCode.Alpha9) { return true; }
        if (KeyCode.Keypad0 <= c && c <= KeyCode.Keypad9) { return true; }
        return false;
    }
}
