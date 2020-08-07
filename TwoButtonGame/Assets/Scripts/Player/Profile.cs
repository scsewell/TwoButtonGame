using System;
using System.Collections.Generic;

using BoostBlasters.Levels;
using BoostBlasters.Races;

using Framework.IO;

namespace BoostBlasters.Players
{
    /// <summary>
    /// Stores information about a player.
    /// </summary>
    public class Profile : SerializableData
    {
        protected override char[] SerializerType { get; } = new char[] { 'B', 'B', 'P', 'F' };
        protected override ushort SerializerVersion => 1;

        private readonly List<RaceResult> m_results = new List<RaceResult>();
        private readonly Dictionary<Guid, List<RaceResult>> m_levelToResults = new Dictionary<Guid, List<RaceResult>>();

        /// <summary>
        /// Is this a temporary profile.
        /// </summary>
        public bool IsTemporary { get; }

        /// <summary>
        /// The ID of this profile.
        /// </summary>
        public Guid Guid { get; private set; }

        /// <summary>
        /// The name of the profile.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The completed races associated with this profile.
        /// </summary>
        public IReadOnlyList<RaceResult> RaceResults => m_results;


        /// <summary>
        /// Creates a new profile.
        /// </summary>
        /// <param name="isTemporary">Is this a temporary profile.</param>
        /// <param name="name">The name of the profile.</param>
        public Profile(bool isTemporary, string name)
        {
            IsTemporary = isTemporary;

            Guid = Guid.NewGuid();
            Name = name;
        }

        /// <summary>
        /// Loads a serialized profile.
        /// </summary>
        /// <param name="reader">A data reader at a serialized profile.</param>
        public Profile(DataReader reader)
        {
            IsTemporary = false;

            Deserialize(reader);
        }

        protected override void OnSerialize(DataWriter writer)
        {
            writer.Write(Guid);
            writer.Write(Name);

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

            var levelCount = reader.Read<int>();
            for (var i = 0; i < levelCount; i++)
            {
                var levelGuid = reader.Read<Guid>();

                var resultCount = reader.Read<int>();
                var results = new List<RaceResult>(resultCount);
                for (var j = 0; j < resultCount; j++)
                {
                    var result = new RaceResult(reader);

                    m_results.Add(result);
                    results.Add(result);
                }

                m_levelToResults.Add(levelGuid, results);
            }
        }

        /// <summary>
        /// Adds a race result to the profile and saves it.
        /// </summary>
        /// <param name="level">The level the result was achieved on.</param>
        /// <param name="result">The result.</param>
        public void AddRaceResult(Level level, RaceResult result)
        {
            // profiles should only include completed races
            if (level == null || result == null || !result.Finished)
            {
                return;
            }

            m_results.Add(result);
            GetRaceResults(level).Add(result);

            // save the updated profile
            ProfileManager.SaveProfile(this);
        }

        /// <summary>
        /// Gets the results of races this player has completed on a level.
        /// </summary>
        /// <param name="level">The level to get the results for.</param>
        /// <returns>The list of results for this player.</returns>
        public List<RaceResult> GetRaceResults(Level level)
        {
            if (!m_levelToResults.TryGetValue(level.Guid, out var results))
            {
                results = new List<RaceResult>();
                m_levelToResults.Add(level.Guid, results);
            }
            return results;
        }
    }
}
