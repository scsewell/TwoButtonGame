﻿using System;
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
                        using (DataReader reader = new DataReader(bytes))
                        {
                            Profile profile = new Profile(reader);
                            m_profiles.Add(profile);

                            Debug.Log($"Loaded profile \"{profile.Name}\"");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to parse player profile from \"{file.Name}\"! {e.ToString()}");
                    }
                }
            }
        }

        /// <summary>
        /// Saves a player profile.
        /// </summary>
        /// <param name="profile">The profile to save.</param>
        /// <returns>True if the profile was successfully saved.</returns>
        public static bool SaveProfile(Profile profile)
        {
            // don't write temporary profiles
            if (profile.IsGuest)
            {
                return true;
            }

            try
            {
                using (DataWriter writer = new DataWriter())
                {
                    profile.Serialize(writer);

                    if (FileIO.WriteFile(GetProfileFilePath(profile), writer.GetBytes()))
                    {
                        Debug.Log($"Saved profile \"{profile.Name}\"");
                        return true;
                    }
                    else
                    {
                        Debug.LogError($"Failed to save profile \"{profile.Name}\"!");
                        return false;
                    }
                }
            }
            catch
            {
                Debug.LogError($"Failed to save profile \"{profile.Name}\"!");
                return false;
            }
        }

        /// <summary>
        /// Creates a temporary profile.
        /// </summary>
        /// <param name="name">The desired name for the profile.</param>
        /// <param name="enforceUnique">Make sure the name does not match any other profile names.</param>
        /// <returns>The new profile.</returns>
        public static Profile GetGuestProfile(string name, bool enforceUnique)
        {
            return CreateProfile(enforceUnique ? m_uniqueGuestProfiles : m_guestProfiles, true, name, enforceUnique);
        }

        /// <summary>
        /// Rmoves a temporary profile.
        /// </summary>
        /// <param name="profile">The profile to remove.</param>
        public static void ReleaseGuestProfile(Profile profile)
        {
            if (profile == null || !profile.IsGuest)
            {
                return;
            }

            m_guestProfiles.Remove(profile);
            m_uniqueGuestProfiles.Remove(profile);
        }

        /// <summary>
        /// Creates a new player profile.
        /// </summary>
        /// <returns>The new profile.</returns>
        public static Profile AddNewProfile()
        {
            Profile profile = CreateProfile(m_profiles, false, "DefaultName", true);
            SaveProfile(profile);
            return profile;
        }

        /// <summary>
        /// Renames a player profile.
        /// </summary>
        /// <param name="profile">The profile to rename.</param>
        /// <param name="newName">The desired name.</param>
        public static void RenameProfile(Profile profile, string newName)
        {
            string name = GetUniqueName(profile, newName, false, m_profiles);

            if (profile.Name != name)
            {
                File.Delete(GetProfileFilePath(profile));
                profile.Name = name;
                SaveProfile(profile);
            }
        }

        /// <summary>
        /// Deletes a player profile
        /// </summary>
        /// <param name="profile">The profile to delete.</param>
        /// <returns>True if the profile was deleted.</returns>
        public static bool DeleteProfile(Profile profile)
        {
            if (profile == null || profile.IsGuest)
            {
                return false;
            }

            bool removedProfile = m_profiles.Remove(profile);
            if (removedProfile)
            {
                File.Delete(GetProfileFilePath(profile));
            }
            return removedProfile;
        }

        private static Profile CreateProfile(List<Profile> profiles, bool isGuest, string baseName, bool uniqueName)
        {
            string name = uniqueName ? GetUniqueName(null, baseName, true, profiles) : baseName;

            Profile profile = new Profile(name, isGuest);
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
