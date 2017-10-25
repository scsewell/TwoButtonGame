using System.Collections.Generic;
using System.Linq;

public struct BufferedInput
{
    public enum EventType { Up, Down, }

    private EventType m_type;
    public EventType Type { get { return m_type; } }

    private float m_time;
    public float Time { get { return m_time; } }

    public BufferedInput(EventType type)
    {
        m_type = type;
        m_time = UnityEngine.Time.unscaledTime;
    }
}
