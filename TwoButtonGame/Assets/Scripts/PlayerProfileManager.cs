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

    private List<PlayerProfile> m_profiles;
    public List<PlayerProfile> Profile { get { return m_profiles; } }

    public void LoadProfiles()
    {
        m_profiles = new List<PlayerProfile>();

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
        int[] ids       = reader.ReadArray<int>();
        string[] names  = reader.ReadArray<string>();

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
        writer.WriteArray(m_profiles.Select(p => p.String).ToArray());
        return writer.GetBytes();
    }
}
