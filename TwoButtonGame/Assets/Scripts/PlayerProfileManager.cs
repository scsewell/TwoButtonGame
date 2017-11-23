using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Framework;
using Framework.IO;

public class PlayerProfileManager : Singleton<PlayerProfileManager>
{
    private static readonly string FILE_NAME = "profiles";
    private static readonly string FILE_EXTENTION = ".dat";

    private Random m_random;

    private List<PlayerProfile> m_profiles;
    public IReadOnlyList<PlayerProfile> Profiles { get { return m_profiles; } }
    
    public PlayerProfileManager()
    {
        m_profiles = new List<PlayerProfile>();
        m_random = new Random();
    }

    public PlayerProfile AddNewProfile()
    {
        byte[] buffer = new byte[sizeof(long)];
        BinaryReader reader = new BinaryReader(buffer);

        m_random.NextBytes(buffer);
        long id = reader.ReadLong();
        
        while (m_profiles.Any(p => p.UniqueId == id))
        {
            reader.SetReadPointer(0);
            m_random.NextBytes(buffer);
            id = reader.ReadLong();
        }

        PlayerProfile profile = new PlayerProfile(id, GetUniqueName(null, "DefaultName"));

        m_profiles.Add(profile);
        SaveProfiles();

        return profile;
    }

    public string GetUniqueName(PlayerProfile profile, string baseName)
    {
        List<PlayerProfile> others = m_profiles.Where(p => p != profile).ToList();

        int count = 0;
        string name = baseName;

        while (others.Any(p => p.Name == name))
        {
            count++;
            name = baseName + count;
        }
        return name;
    }

    public bool DeleteProfile(PlayerProfile profile)
    {
        bool removedProfile = m_profiles.Remove(profile);
        if (removedProfile)
        {
            SaveProfiles();
        }
        return removedProfile;
    }

    public void LoadProfiles()
    {
        byte[] bytes = FileIO.ReadFileBytes(GetSavePath());
        if (bytes != null)
        {
            FromBytes(bytes);
        }
    }

    public void SaveProfiles()
    {
        FileIO.WriteFile(GetBytes(), GetSavePath());
    }

    private string GetSavePath()
    {
        return FileIO.GetInstallDirectory() + FILE_NAME + FILE_EXTENTION;
    }
    
    private void FromBytes(byte[] bytes)
    {
        BinaryReader reader = new BinaryReader(bytes);

        int count       = reader.ReadInt();
        long[] ids      = reader.ReadArray<long>();
        string[] names  = reader.ReadStringArray();

        m_profiles.Clear();

        for (int i = 0; i < count; i++)
        {
            m_profiles.Add(new PlayerProfile(ids[i], names[i]));
        }
    }
    
    private byte[] GetBytes()
    {
        BinaryWriter writer = new BinaryWriter();
        writer.WriteValue(m_profiles.Count);
        writer.WriteArray(m_profiles.Select(p => p.UniqueId).ToArray());
        writer.WriteArray(m_profiles.Select(p => p.Name).ToArray());
        return writer.GetBytes();
    }
}
