using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInput
{
    private const float DOUBLE_PRESS_TIME = 0.075f;
    private const float DOUBLE_TAP_TIME = 0.2f;
    private const float DOUBLE_TAP_TOLERANCE = 0.0825f;

    private InputButton m_button1;
    public InputButton Button1 { get { return m_button1; } }

    private InputButton m_button2;
    public InputButton Button2 { get { return m_button2; } }

    public List<string> ButtonNames { get { return new List<string>() { m_button1.Name, m_button2.Name }; } }

    private bool m_button1Press = false;
    public bool Button1Pressed { get { return m_button1Press; } }

    private bool m_button2Press = false;
    public bool Button2Pressed { get { return m_button2Press; } }

    private bool m_bothDown = false;
    public bool BothDown { get { return m_bothDown; } }

    private bool m_bothDoubleTap = false;
    public bool BothDoubleTap { get { return m_bothDoubleTap; } }

    private bool m_bothDoubleTapHeld = false;
    public bool BothDoubleTapHeld { get { return m_bothDoubleTapHeld; } }

    public PlayerInput(KeyCode button1, KeyCode button2)
    {
        m_button1 = new InputButton(button1);
        m_button2 = new InputButton(button2);
    }

    public void Update(bool inMenu)
    {
        m_button1.Update();
        m_button2.Update();

        List<BufferedInput> button1Buf = m_button1.Buffer;
        List<BufferedInput> button2Buf = m_button2.Buffer;

        m_bothDoubleTap = false;
        if (!m_button1.IsDown || !m_button2.IsDown)
        {
            m_bothDoubleTapHeld = false;
        }

        float doubleTapTime1 = GetDoubleTapTime(button1Buf);
        float doubleTapTime2 = GetDoubleTapTime(button2Buf);

        if (doubleTapTime1 > 0 && doubleTapTime2 > 0 && Mathf.Abs(doubleTapTime1 - doubleTapTime2) < DOUBLE_TAP_TOLERANCE)
        {
            m_bothDoubleTap = true;
            m_bothDoubleTapHeld = true;
            button1Buf.Clear();
            button2Buf.Clear();
        }

        m_bothDown = false;
        if (button1Buf.Count > 0 && button2Buf.Count > 0)
        {
            BufferedInput button1Last = button1Buf.Last();
            BufferedInput button2Last = button2Buf.Last();

            if (button1Last.Type == BufferedInput.EventType.Down &&
                button2Last.Type == BufferedInput.EventType.Down &&
                Mathf.Abs(button1Last.Time - button2Last.Time) <= DOUBLE_PRESS_TIME)
            {
                m_bothDown = true;
                if (inMenu)
                {
                    button1Buf.Clear();
                    button2Buf.Clear();
                }
            }
        }

        m_button1Press = false;
        if (WasPressed(button1Buf, inMenu))
        {
            m_button1Press = true;
        }

        m_button2Press = false;
        if (WasPressed(button2Buf, inMenu))
        {
            m_button2Press = true;
        }
    }

    private bool WasPressed(List<BufferedInput> buf, bool inMenu)
    {
        if (buf.Count > 0)
        {
            BufferedInput last = buf.Last();

            if ((last.Type == BufferedInput.EventType.Down && Time.unscaledTime - last.Time > DOUBLE_PRESS_TIME) ||
                last.Type == BufferedInput.EventType.Up)
            {
                if (inMenu)
                {
                    buf.Clear();
                }
                return true;
            }
        }
        return false;
    }

    private float GetDoubleTapTime(List<BufferedInput> buf)
    {
        for (int i = buf.Count; i >= 3; i--)
        {
            BufferedInput input1 = buf[i - 3];
            BufferedInput input2 = buf[i - 2];
            BufferedInput input3 = buf[i - 1];

            if (input1.Type == BufferedInput.EventType.Down &&
                input2.Type == BufferedInput.EventType.Up &&
                input3.Type == BufferedInput.EventType.Down &&
                input3.Time - input1.Time < DOUBLE_TAP_TIME)
            {
                return input1.Time;
            }
        }
        return float.MinValue;
    }
}
