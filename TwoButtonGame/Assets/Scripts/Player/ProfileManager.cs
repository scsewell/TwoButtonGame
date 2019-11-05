using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

using UnityEngine;

using Framework;
using Framework.IO;

namespace BoostBlasters.Players
{
    /// <summary>
    /// Manages access to player profiles.
    /// </summary>
    public static class ProfileManager
    {
        /// <summary>
        /// The name of the folder to store profiles under.
        /// </summary>
        private static readonly string FOLDER_NAME = "Profiles";

        /// <summary>
        /// The file extention used for profiles.
        /// </summary>
        private static readonly string FILE_EXTENTION = ".prf";

        private static readonly System.Random m_random = new System.Random();

        private static readonly List<Profile> m_guestProfiles = new List<Profile>();
        private static readonly List<Profile> m_uniqueGuestProfiles = new List<Profile>();
        private static readonly List<Profile> m_profiles = new List<Profile>();

        /// <summary>
        /// Gets all loaded player profiles.
        /// </summary>
        public static IReadOnlyList<Profile> Profiles => m_profiles;

        /// <summary>
        /// Loads all the available player profiles.
        /// </summary>
        public static async Task LoadProfilesAsync()
        {
            await Task.Run(LoadProfiles);
        }

        private static void LoadProfiles()
        {
            // get all profile files
            List<FileInfo> files = FileIO.GetFiles(GetProfileDirectory(), FILE_EXTENTION).ToList();

            // sort the files by the date of creation so the oldest appear first
            files.Sort((x, y) => x.CreationTimeUtc.CompareTo(y.CreationTimeUtc));

            // parse the profiles from the files
            m_profiles.Clear();

            foreach (FileInfo file in files)
            {
                if (FileIO.ReadFileBytes(file.FullName, out byte[] bytes))
                {
                    try
                    {
                        Profile profile = new Profile(bytes);
                        m_profiles.Add(profile);

                        Debug.Log($"Loaded profile \"{profile.Name}\"");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to parse player profile from \"{file.Name}\"! {e.ToString()}");
                    }
                }
                else
                {
                    Debug.LogError($"Failed to load player profile from \"{file.Name}\"!");
                }
            }
        }

        public static Profile GetGuestProfile(string name, bool enforceUnique)
        {
            return CreateProfile(enforceUnique ? m_uniqueGuestProfiles : m_guestProfiles, true, name, enforceUnique);
        }

        public static void ReleaseGuestProfile(Profile profile)
        {
            m_guestProfiles.Remove(profile);
            m_uniqueGuestProfiles.Remove(profile);
        }

        public static Profile AddNewProfile()
        {
            Profile profile = CreateProfile(m_profiles, false, "DefaultName", true);
            SaveProfile(profile);
            return profile;
        }

        public static bool DeleteProfile(Profile profile)
        {
            bool removedProfile = m_profiles.Remove(profile);
            if (removedProfile)
            {
                File.Delete(GetProfileFilePath(profile));
            }
            return removedProfile;
        }

        public static string GetUniqueName(Profile profile, string baseName)
        {
            return GetUniqueName(profile, baseName, false, m_profiles);
        }

        private static Profile CreateProfile(List<Profile> profiles, bool isGuest, string baseName, bool uniqueName)
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

            Profile profile = new Profile(id, isGuest, name);
            profiles.Add(profile);
            return profile;
        }

        private static string GetUniqueName(Profile profile, string baseName, bool startWithCount, List<Profile> profiles)
        {
            List<Profile> others = profiles.Where(p => p != profile).ToList();

            int count = 0;
            string name = startWithCount ? baseName + (++count) : baseName;

            while (others.Any(p => p.Name == name))
            {
                count++;
                name = baseName + count;
            }
            return name;
        }

        public static void RenameProfile(Profile profile, string newName)
        {
            string name = GetUniqueName(profile, newName);
            if (profile.Name != name)
            {
                File.Delete(GetProfileFilePath(profile));
                profile.Name = name;
                SaveProfile(profile);
            }
        }

        public static void SaveProfile(Profile profile)
        {
            if (!profile.IsGuest)
            {
                if (FileIO.WriteFile(GetProfileFilePath(profile), profile.GetBytes()))
                {
                    Debug.Log($"Saved profile \"{profile.Name}\"");
                }
                else
                {
                    Debug.LogError($"Failed to save profile \"{profile.Name}\"!");
                }
            }
        }

        private static string GetProfileFilePath(Profile profile)
        {
            return Path.Combine(GetProfileDirectory(), profile.Name + FILE_EXTENTION);
        }

        private static string GetProfileDirectory()
        {
            return Path.Combine(FileIO.GetConfigDirectory(), FOLDER_NAME);
        }
    }
}
