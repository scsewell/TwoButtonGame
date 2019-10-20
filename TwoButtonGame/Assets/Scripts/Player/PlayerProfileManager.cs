using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using UnityEngine;

using Framework;
using Framework.IO;

namespace BoostBlasters.Players
{
    /// <summary>
    /// Manages access to player profiles.
    /// </summary>
    public class PlayerProfileManager : Singleton<PlayerProfileManager>
    {
        /// <summary>
        /// The name of the folder to store profiles under.
        /// </summary>
        private static readonly string FOLDER_NAME = "Profiles";

        /// <summary>
        /// The file extention used for profiles.
        /// </summary>
        private static readonly string FILE_EXTENTION = ".prf";

        private readonly System.Random m_random = new System.Random();

        private readonly List<PlayerProfile> m_guestProfiles = new List<PlayerProfile>();
        private readonly List<PlayerProfile> m_uniqueGuestProfiles = new List<PlayerProfile>();
        private readonly List<PlayerProfile> m_profiles = new List<PlayerProfile>();

        /// <summary>
        /// Gets all loaded player profiles.
        /// </summary>
        public IReadOnlyList<PlayerProfile> Profiles => m_profiles;

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
            SaveProfile(profile);
            return profile;
        }

        public bool DeleteProfile(PlayerProfile profile)
        {
            bool removedProfile = m_profiles.Remove(profile);
            if (removedProfile)
            {
                File.Delete(GetProfilePath(profile));
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
            Framework.IO.BinaryReader reader = new Framework.IO.BinaryReader(buffer);

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
            m_profiles.Clear();

            if (Directory.Exists(GetProfileDir()))
            {
                List<FileInfo> files = FileIO.GetFiles(GetProfileDir(), FILE_EXTENTION).ToList();
                files.Sort((x, y) => x.CreationTime.CompareTo(y.CreationTime));

                foreach (FileInfo file in files)
                {
                    if (FileIO.ReadFileBytes(file.FullName, out byte[] bytes))
                    {
                        PlayerProfile profile = new PlayerProfile(bytes);
                        m_profiles.Add(profile);

                        Debug.Log($"Loaded profile \"{profile.Name}\"");
                    }
                    else
                    {
                        Debug.LogError($"Failed to load profile from \"{file.Name}\"!");
                    }
                }
            }
        }

        public void RenameProfile(PlayerProfile profile, string newName)
        {
            string name = GetUniqueName(profile, newName);
            if (profile.Name != name)
            {
                File.Delete(GetProfilePath(profile));
                profile.Name = name;
                SaveProfile(profile);
            }
        }

        public void SaveProfile(PlayerProfile profile)
        {
            if (!profile.IsGuest)
            {
                if (FileIO.WriteFile(GetProfilePath(profile), profile.GetBytes()))
                {
                    Debug.Log($"Saved profile \"{profile.Name}\"");
                }
                else
                {
                    Debug.LogError($"Failed to save profile \"{profile.Name}\"!");
                }
            }
        }

        private string GetProfilePath(PlayerProfile profile)
        {
            return Path.Combine(GetProfileDir(), profile.Name + FILE_EXTENTION);
        }

        private string GetProfileDir()
        {
            return Path.Combine(FileIO.GetConfigDirectory(), FOLDER_NAME);
        }
    }
}
