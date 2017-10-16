using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInput
{
    private KeyCode m_button1;
    private KeyCode m_button2;

    private string m_button1Name;
    private string m_button2Name;

    public string Button1Name { get { return m_button1Name; } }
    public string Button2Name { get { return m_button2Name; } }
    public List<string> ButtonNames { get { return new List<string>() { m_button1Name, m_button2Name }; } }

    public bool Button1 { get { return Input.GetKey(m_button1); } }
    public bool Button2 { get { return Input.GetKey(m_button2); } }
    public bool Button1Up { get { return !m_ignore && Input.GetKeyUp(m_button1); } }
    public bool Button2Up { get { return !m_ignore && Input.GetKeyUp(m_button2); } }

    private bool m_bothDown = false;
    public bool BothDown { get { return m_bothDown; } }
    
    private bool m_ignore = false;
    private float m_oneDownTime = 0;

    public PlayerInput(KeyCode button1, KeyCode button2)
    {
        m_button1 = button1;
        m_button2 = button2;

        m_button1Name = GetName(m_button1);
        m_button2Name = GetName(m_button2);
    }

    public void Update()
    {
        bool button1Down = Input.GetKeyDown(m_button1);
        bool button2Down = Input.GetKeyDown(m_button2);
        
        m_bothDown = false;
        if (!m_ignore)
        {
            if (button1Down && button2Down)
            {
                m_bothDown = true;
                m_ignore = true;
            }
            else if (button1Down || button2Down)
            {
                if (!(Button1 && Button2))
                {
                    m_oneDownTime = Time.time;
                }
                else if (Time.time - m_oneDownTime < 0.075f)
                {
                    m_bothDown = true;
                    m_ignore = true;
                }
            }
        }
    }

    public void LateUpdate()
    {
        if (!Button1 && !Button2)
        {
            m_ignore = false;
        }
    }

    private string GetName(KeyCode button)
    {
        string str = button.ToString();
        switch (button)
        {
            case KeyCode.Keypad0:
            case KeyCode.Keypad1:
            case KeyCode.Keypad2:
            case KeyCode.Keypad3:
            case KeyCode.Keypad4:
            case KeyCode.Keypad5:
            case KeyCode.Keypad6:
            case KeyCode.Keypad7:
            case KeyCode.Keypad8:
            case KeyCode.Keypad9:       return "Num" + str.Substring(str.Length - 1);

            case KeyCode.LeftArrow:     return "Left";
            case KeyCode.RightArrow:    return "Right";
            case KeyCode.UpArrow:       return "Up";
            case KeyCode.DownArrow:     return "Down";
        }
        return str;
    }
}
