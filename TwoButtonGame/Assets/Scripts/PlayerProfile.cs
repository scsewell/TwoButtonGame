using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class PlayerProfile
{
    private int m_uniqueId;
    public int UniqueId { get { return m_uniqueId; } }

    private string m_name;
    public string String { get { return m_name; } }

    public PlayerProfile(int uniqueId, string name)
    {
        m_uniqueId = uniqueId;
        m_name = name;
    }
}
