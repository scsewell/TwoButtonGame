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

    private List<PlayerProfile> m_guestProfiles;
    private List<PlayerProfile> m_uniqueGuestProfiles;

    private List<PlayerProfile> m_profiles;
    public IReadOnlyList<PlayerProfile> Profiles { get { return m_profiles; } }

    public PlayerProfileManager()
    {
        m_guestProfiles = new List<PlayerProfile>();
        m_uniqueGuestProfiles = new List<PlayerProfile>();
        m_profiles = new List<PlayerProfile>();
        m_random = new Random();
    }

    public PlayerProfile GetGuestProfile(string name, bool enforceUnique)
    {
        return CreateProfile(enforceUnique ? m_uniqueGuestProfiles : m_guestProfiles, true, name, enforceUnique);
    }

    public void ReleaseGuestProfile(PlayerProfile profile)
    {
        m_guestProfiles.Remove(profile);
        m_uniqueGuestProfiles.Remove(profile);
    }

    public PlayerProfile AddNewProfile()
    {
        PlayerProfile profile = CreateProfile(m_profiles, false, "DefaultName", true);
        SaveProfiles();
        return profile;
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

    public string GetUniqueName(PlayerProfile profile, string baseName)
    {
        return GetUniqueName(profile, baseName, false, m_profiles);
    }

    private PlayerProfile CreateProfile(List<PlayerProfile> profiles, bool isGuest, string baseName, bool uniqueName)
    {
        byte[] buffer = new byte[sizeof(long)];
        BinaryReader reader = new BinaryReader(buffer);

        m_random.NextBytes(buffer);
        long id = reader.ReadLong();

        while (profiles.Any(p => p.UniqueId == id))
        {
            reader.SetReadPointer(0);
            m_random.NextBytes(buffer);
            id = reader.ReadLong();
        }

        string name = uniqueName ? GetUniqueName(null, baseName, true, profiles) : baseName;

        PlayerProfile profile = new PlayerProfile(id, isGuest, name);
        profiles.Add(profile);
        return profile;
    }

    private string GetUniqueName(PlayerProfile profile, string baseName, bool startWithCount, List<PlayerProfile> profiles)
    {
        List<PlayerProfile> others = profiles.Where(p => p != profile).ToList();

        int count = 0;
        string name = startWithCount ? baseName + (++count) : baseName;

        while (others.Any(p => p.Name == name))
        {
            count++;
            name = baseName + count;
        }
        return name;
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
            m_profiles.Add(new PlayerProfile(ids[i], false, names[i]));
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
