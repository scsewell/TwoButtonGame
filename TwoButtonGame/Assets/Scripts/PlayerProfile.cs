public class PlayerProfile
{
    private long m_uniqueId;
    public long UniqueId { get { return m_uniqueId; } }
    
    private bool m_isGuest;
    public bool IsGuest { get { return m_isGuest; } }

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

    public PlayerProfile(long uniqueId, bool isGuest, string name)
    {
        m_uniqueId = uniqueId;
        m_isGuest = isGuest;
        m_name = name;
    }
}
