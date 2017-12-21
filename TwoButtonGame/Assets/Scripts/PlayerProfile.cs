using System.Collections.Generic;
using System.Linq;
using Framework.IO;

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
        set { m_name = value; }
    }

    private List<RaceResult> m_raceResults;
    public List<RaceResult> RaceResults { get { return m_raceResults; } }

    public PlayerProfile(long uniqueId, bool isGuest, string name)
    {
        m_uniqueId = uniqueId;
        m_isGuest = isGuest;
        m_name = name;

        m_raceResults = new List<RaceResult>();
    }

    public PlayerProfile(byte[] bytes)
    {
        BinaryReader reader = new BinaryReader(bytes);
        
        m_uniqueId = reader.ReadLong();
        m_isGuest = false;
        m_name = reader.ReadString();

        m_raceResults = new List<RaceResult>();

        int resultCount = reader.ReadInt();
        for (int i = 0; i < resultCount; i++)
        {
            m_raceResults.Add(new RaceResult(reader.ReadArray<byte>(), this));
        }
    }

    public byte[] GetBytes()
    {
        BinaryWriter writer = new BinaryWriter();
        writer.WriteValue(m_uniqueId);
        writer.WriteValue(m_name);

        writer.WriteValue(m_raceResults.Count);
        foreach (RaceResult result in m_raceResults)
        {
            writer.WriteArray(result.GetBytes());
        }

        return writer.GetBytes();
    }

    public void AddRaceResult(RaceResult result)
    {
        if (result != null && result.Finished)
        {
            m_raceResults.Add(result);
            PlayerProfileManager.Instance.SaveProfile(this);
        }
    }
}
