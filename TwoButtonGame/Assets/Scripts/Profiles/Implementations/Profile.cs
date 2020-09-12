using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using BoostBlasters.Levels;

using Framework;
using Framework.IO;

using UnityEngine;

namespace BoostBlasters.Profiles
{
    /// <summary>
    /// A persistent <see cref="IProfile"/> used by local players.
    /// </summary>
    public class Profile : SerializableData, IProfile
    {
        /// <summary>
        /// The name of the folder to store profiles under.
        /// </summary>
        private static readonly string FOLDER_NAME = "Profiles";

        /// <summary>
        /// The file extention used for local profiles.
        /// </summary>
        private static readonly string FILE_EXTENTION = ".prf";


        private static readonly List<Profile> s_profiles = new List<Profile>();
        private static readonly NameRegistry s_nameRegistery = new NameRegistry();

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
            s_profiles.Clear();
            s_nameRegistery.Reset();

            Added = delegate { };
            Renamed = delegate { };
            Deleted = delegate { };
        }


        /// <summary>
        /// Gets all currently loaded player profiles.
        /// </summary>
        /// <seealso cref="LoadProfilesAsync"/>
        public static IReadOnlyList<Profile> AllProfiles => s_profiles;

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
            s_profiles.Clear();

            foreach (var file in files)
            {
                if (FileIO.ReadFileBytes(file.FullName, out var bytes))
                {
                    using (var reader = new DataReader(bytes))
                    {
                        new Profile(reader);
                    }
                }
            }
        }

        private static string GetProfileFilePath(string name)
        {
            return Path.Combine(GetProfileDirectory(), name + FILE_EXTENTION);
        }

        private static string GetProfileDirectory()
        {
            return Path.Combine(FileIO.ConfigDirectory, FOLDER_NAME);
        }


        protected override FourCC SerializerType { get; } = new FourCC("PROF");
        protected override ushort SerializerVersion => 1;

        private bool m_deleted;
        private readonly List<RaceResult> m_results = new List<RaceResult>();
        private readonly Dictionary<Guid, List<RaceResult>> m_levelToResults = new Dictionary<Guid, List<RaceResult>>();

        /// <inheritdoc/>
        public Guid Guid { get; private set; }

        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public Color Color { get; private set; }

        /// <summary>
        /// The completed races associated with this profile.
        /// </summary>
        public IReadOnlyList<RaceResult> RaceResults => m_results;


        /// <summary>
        /// Creates a new <see cref="Profile"/> instance.
        /// </summary>
        /// <remarks>
        /// The name will be made unique by appending a number if another
        /// local profile with the same name exists.
        /// </remarks>
        /// <param name="name">The desired player name.</param>
        public Profile(string name)
        {
            Guid = Guid.NewGuid();
            Name = s_nameRegistery.ReserveUniqueName(name, false);
            Color = ProfileUtils.GetRandomColor();

            Save();
            s_profiles.Add(this);

            Added?.Invoke(this);

            Debug.Log($"Created profile \"{Name}\"");
        }

        private Profile(DataReader reader)
        {
            Deserialize(reader);

            if (!IsValid)
            {
                return;
            }

            s_profiles.Add(this);
            s_nameRegistery.ReserveUniqueName(Name, false);

            Added?.Invoke(this);

            Debug.Log($"Loaded profile \"{Name}\"");
        }

        protected override void OnSerialize(DataWriter writer)
        {
            writer.Write(Guid);
            writer.Write(Name);
            writer.Write(Color);

            writer.Write(m_levelToResults.Keys.Count);
            foreach (var results in m_levelToResults)
            {
                writer.Write(results.Key);
                writer.Write(results.Value.Count);
                foreach (var result in results.Value)
                {
                    result.Serialize(writer);
                }
            }
        }

        protected override void OnDeserialize(DataReader reader, ushort version)
        {
            Guid = reader.Read<Guid>();
            Name = reader.ReadString();
            Color = reader.Read<Color>();

            var levelCount = reader.Read<int>();
            for (var i = 0; i < levelCount; i++)
            {
                var levelGuid = reader.Read<Guid>();

                var levelResultCount = reader.Read<int>();
                var levelResults = new List<RaceResult>(levelResultCount);
                for (var j = 0; j < levelResultCount; j++)
                {
                    var result = new RaceResult(reader);
                    levelResults.Add(result);
                    m_results.Add(result);
                }

                m_levelToResults.Add(levelGuid, levelResults);
            }
        }

        /// <summary>
        /// Changes the player name.
        /// </summary>
        /// <remarks>
        /// The name will be made unique by appending a number if another
        /// local profile with the same name exists.
        /// </remarks>
        /// <param name="name">The desired name.</param>
        public void Rename(string name)
        {
            EnsureNotDeleted();

            var oldName = Name;

            s_nameRegistery.ReleaseName(oldName);
            Name = s_nameRegistery.ReserveUniqueName(name, false);

            if (Name != oldName)
            {
                FileIO.DeleteFile(GetProfileFilePath(oldName));
                Save();

                Renamed?.Invoke(this);
            }
        }

        /// <summary>
        /// Deletes this profile.
        /// </summary>
        public void Delete()
        {
            EnsureNotDeleted();

            s_profiles.Remove(this);
            s_nameRegistery.ReleaseName(Name);

            FileIO.DeleteFile(GetProfileFilePath(Name));

            Deleted?.Invoke(this);

            m_deleted = true;

            Debug.Log($"Deleted profile \"{Name}\"");
        }

        /// <summary>
        /// Adds a race result to the profile.
        /// </summary>
        /// <param name="level">The level the result was achieved on.</param>
        /// <param name="result">The result.</param>
        public void AddRaceResult(Level level, RaceResult result)
        {
            EnsureNotDeleted();

            // profiles should only include completed races
            if (level == null || result == null || !result.Finished)
            {
                return;
            }

            m_results.Add(result);
            GetRaceResults(level).Add(result);

            // save the updated profile
            Save();
        }

        /// <summary>
        /// Gets the results of races this player has completed on a level.
        /// </summary>
        /// <param name="level">The level to get the results for.</param>
        /// <returns>The list of results for this player.</returns>
        public List<RaceResult> GetRaceResults(Level level)
        {
            EnsureNotDeleted();

            if (!m_levelToResults.TryGetValue(level.Guid, out var results))
            {
                results = new List<RaceResult>();
                m_levelToResults.Add(level.Guid, results);
            }
            return results;
        }

        private void EnsureNotDeleted()
        {
            if (m_deleted)
            {
                throw new ObjectDisposedException($"{GetType().Name}: {Name}");
            }
        }

        private bool Save()
        {
            using (var writer = new DataWriter())
            {
                if (Serialize(writer) && FileIO.WriteFile(GetProfileFilePath(Name), writer.GetBytes()))
                {
                    Debug.Log($"Saved profile \"{Name}\"");
                    return true;
                }
                else
                {
                    Debug.LogError($"Failed to save profile \"{Name}\"!");
                    return false;
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
