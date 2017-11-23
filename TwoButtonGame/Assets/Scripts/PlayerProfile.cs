using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class PlayerProfile
{
    private long m_uniqueId;
    public long UniqueId { get { return m_uniqueId; } }

    private string m_name;
    public string Name
    {
        get { return m_name; }
        set
        {
            string name = PlayerProfileManager.Instance.GetUniqueName(this, value);
            if (m_name != name)
            {
                m_name = name;
                PlayerProfileManager.Instance.SaveProfiles();
            }
        }
    }

    public PlayerProfile(long uniqueId, string name)
    {
        m_uniqueId = uniqueId;
        m_name = name;
    }
}
