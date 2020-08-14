using UnityEngine;

public class UIAxisInput
{
    private static readonly float THRESH = 0.75f;

    private bool m_isNegative;
    private bool m_repeat;
    private float m_waitTime;

    private bool m_pressed;
    public bool Pressed { get { return m_pressed; } }
    
    public UIAxisInput(bool isNegative)
    {
        m_isNegative = isNegative;

        m_repeat = false;
        m_waitTime = 0;
        m_pressed = false;
    }
    
    public void Update(float value)
    {
        m_pressed = false;

        if (m_isNegative ? value < -THRESH : value > THRESH)
        {
            if (m_waitTime <= 0)
            {
                m_pressed = true;
                m_waitTime = m_repeat ? CustomInputModule.REPEAT_DELAY / 2 : CustomInputModule.REPEAT_WAIT;
                m_repeat = true;
            }
            else
            {
                m_waitTime -= Time.unscaledDeltaTime;
            }
        }
        else
        {
            m_repeat = false;
            m_waitTime = 0;
        }
    }
}
