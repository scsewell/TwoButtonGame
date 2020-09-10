using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Framework.IO;

using UnityEngine;

namespace BoostBlasters.Profiles
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


        private static readonly List<Profile> m_tempProfiles = new List<Profile>();
        private static readonly List<Profile> m_uniqueTempProfiles = new List<Profile>();
        private static readonly List<Profile> m_profiles = new List<Profile>();

        /// <summary>
        /// Gets all loaded persistent player profiles.
        /// </summary>
        public static IReadOnlyList<Profile> Profiles => m_profiles;

        /// <summary>
        /// An event invoked when a profile has been added.
        /// </summary>
        public static event Action<Profile> Added;

        /// <summary>
        /// An event invoked when a profile has been renamed.
        /// </summary>
        public static event Action<Profile> Renamed;

        /// <summary>
        /// An event invoked when a profile has been deleted.
        /// </summary>
        public static event Action<Profile> Deleted;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            m_tempProfiles.Clear();
            m_uniqueTempProfiles.Clear();
            m_profiles.Clear();

            Added = delegate {};
            Renamed = delegate {};
            Deleted = delegate {};
        }

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
            var files = FileIO.GetFiles(GetProfileDirectory(), FILE_EXTENTION).ToList();

            // sort the files by the date of creation so the oldest appear first
            files.Sort((x, y) => x.CreationTimeUtc.CompareTo(y.CreationTimeUtc));

            // parse the profiles from the files
            m_profiles.Clear();

            foreach (var file in files)
            {
                if (FileIO.ReadFileBytes(file.FullName, out var bytes))
                {
                    using (var reader = new DataReader(bytes))
                    {
                        var profile = new Profile(reader);

                        if (profile.IsValid)
                        {
                            m_profiles.Add(profile);

                            Debug.Log($"Loaded profile \"{profile.Name}\"");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a non-persistent profile.
        /// </summary>
        /// <param name="name">The desired name for the profile.</param>
        /// <param name="enforceUnique">Ensures sure the name does not match any other profile names.</param>
        /// <returns>The new profile.</returns>
        public static Profile CreateTemporaryProfile(string name, bool enforceUnique)
        {
            if (enforceUnique)
            {
                name = GetUniqueName(m_uniqueTempProfiles, null, name, true);
            }

            var profile = new Profile(true, name);

            var profiles = enforceUnique ? m_uniqueTempProfiles : m_tempProfiles;
            profiles.Add(profile);

            return profile;
        }

        /// <summary>
        /// Removes a non-persisitent profile.
        /// </summary>
        /// <param name="profile">The profile to remove.</param>
        public static void ReleaseTemporaryProfile(Profile profile)
        {
            if (profile == null || !profile.IsTemporary)
            {
                return;
            }

            m_tempProfiles.Remove(profile);
            m_uniqueTempProfiles.Remove(profile);
        }

        /// <summary>
        /// Creates a new persisitent player profile.
        /// </summary>
        /// <remarks>
        /// The name will be made unique by appending a number if another
        /// persistent profile with the same name exists.
        /// </remarks>
        /// <param name="name">The desired name.</param>
        /// <returns>The new profile.</returns>
        public static Profile CreateProfile(string name = "DefaultName")
        {
            name = GetUniqueName(m_profiles, null, name, false);
            
            var profile = new Profile(false, name);
            m_profiles.Add(profile);

            SaveProfile(profile);

            Added?.Invoke(profile);
            return profile;
        }

        /// <summary>
        /// Renames a persistent player profile.
        /// </summary>
        /// <remarks>
        /// The name will be made unique by appending a number if another
        /// persistent profile with the same name exists.
        /// </remarks>
        /// <param name="profile">The profile to rename.</param>
        /// <param name="name">The desired name.</param>
        /// <returns>True if the name was changed, false otherwise.</returns>
        public static bool RenameProfile(Profile profile, string name)
        {
            if (profile == null || profile.IsTemporary)
            {
                return false;
            }

            name = GetUniqueName(m_profiles, profile, name, false);

            if (profile.Name == name)
            {
                return false;
            }

            // we need to remove the profile file with the old name
            FileIO.DeleteFile(GetProfileFilePath(profile));
            profile.Name = name;
            SaveProfile(profile);

            Renamed?.Invoke(profile);
            return true;
        }

        /// <summary>
        /// Deletes a persistent player profile.
        /// </summary>
        /// <param name="profile">The profile to delete.</param>
        /// <returns>True if the profile was deleted, false otherwise.</returns>
        public static bool DeleteProfile(Profile profile)
        {
            if (profile == null || profile.IsTemporary)
            {
                return false;
            }

            var removedProfile = m_profiles.Remove(profile);

            if (removedProfile)
            {
                FileIO.DeleteFile(GetProfileFilePath(profile));

                Deleted?.Invoke(profile);

                Debug.Log($"Deleted profile \"{profile.Name}\"");
            }

            return removedProfile;
        }

        /// <summary>
        /// Saves a persistent player profile.
        /// </summary>
        /// <param name="profile">The profile to save.</param>
        /// <returns>True if the profile was successfully saved.</returns>
        internal static bool SaveProfile(Profile profile)
        {
            if (profile == null || profile.IsTemporary)
            {
                return false;
            }

            using (var writer = new DataWriter())
            {
                if (profile.Serialize(writer) && FileIO.WriteFile(GetProfileFilePath(profile), writer.GetBytes()))
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

        private static string GetUniqueName(List<Profile> profiles, Profile profile, string name, bool alwaysAddNumber)
        {
            var i = 0;
            var n = alwaysAddNumber ? name + (++i) : name;

            while (profiles.Any(p => p.Name == n && p != profile))
            {
                i++;
                n = name + i;
            }

            return n;
        }

        private static string GetProfileFilePath(Profile profile)
        {
            return Path.Combine(GetProfileDirectory(), profile.Name + FILE_EXTENTION);
        }

        private static string GetProfileDirectory()
        {
            return Path.Combine(FileIO.ConfigDirectory, FOLDER_NAME);
        }
    }
}
