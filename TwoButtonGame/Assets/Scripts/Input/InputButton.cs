using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InputButton
{
    private const int HISTORY_LENGTH = 10;

    private KeyCode m_key;
    public KeyCode Key
    {
        get { return m_key; }
        private set
        {
            m_key = value;
            m_name = InputManager.GetName(value);
        }
    }

    private string m_name;
    public string Name { get { return m_name; } }

    private List<BufferedInput> m_buffer = new List<BufferedInput>();
    public List<BufferedInput> Buffer { get { return m_buffer; } }

    public bool IsDown
    {
        get { return Input.GetKey(m_key); }
    }
    
    public InputButton(KeyCode key)
    {
        Key = key;
    }

    public void Update()
    {
        if (Input.GetKeyDown(m_key))
        {
            m_buffer.Add(new BufferedInput(BufferedInput.EventType.Down));
        }
        if (Input.GetKeyUp(m_key) && m_buffer.Count > 0 && m_buffer.Last().Type == BufferedInput.EventType.Down)
        {
            m_buffer.Add(new BufferedInput(BufferedInput.EventType.Up));
        }

        while (m_buffer.Count > HISTORY_LENGTH)
        {
            m_buffer.RemoveAt(0);
        }
    }
}