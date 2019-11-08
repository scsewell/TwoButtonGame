using System;
using System.Collections.Generic;

using Framework.IO;

using BoostBlasters.Races;
using BoostBlasters.Levels;

namespace BoostBlasters.Players
{
    /// <summary>
    /// Stores information about a player.
    /// </summary>
    public class Profile : SerializableData
    {
        private readonly char[] SERIALIZER_TYPE = new char[] { 'B', 'B', 'P', 'F' };

        protected override char[] SerializerType => SERIALIZER_TYPE;
        protected override ushort SerializerVersion => 1;


        /// <summary>
        /// The ID of this profile.
        /// </summary>
        public Guid Guid { get; private set; }

        /// <summary>
        /// The name of the player.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Is this a temporary profile.
        /// </summary>
        public bool IsGuest { get; }

        private readonly List<RaceResult> m_results = new List<RaceResult>();
        private readonly Dictionary<Guid, List<RaceResult>> m_levelToResults = new Dictionary<Guid, List<RaceResult>>();

        /// <summary>
        /// The races that this player has completed.
        /// </summary>
        public IReadOnlyList<RaceResult> RaceResults => m_results;


        /// <summary>
        /// Creates a new profile.
        /// </summary>
        /// <param name="name">The name of the player.</param>
        /// <param name="isGuest">Is this a temporary profile.</param>
        public Profile(string name, bool isGuest)
        {
            Guid = Guid.NewGuid();
            Name = name;
            IsGuest = isGuest;
        }

        /// <summary>
        /// Loads a serialized profile.
        /// </summary>
        /// <param name="reader">A data reader at a serialized profile.</param>
        public Profile(DataReader reader)
        {
            Deserialize(reader);

            // serialized profiles are not temporary
            IsGuest = false;
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
            if (!m_levelToResults.TryGetValue(level.Guid, out List<RaceResult> results))
            {
                results = new List<RaceResult>();
                m_levelToResults.Add(level.Guid, results);
            }
            return results;
        }

        protected override void OnSerialize(DataWriter writer)
        {
            writer.Write(Guid);
            writer.Write(Name);

            writer.Write(m_levelToResults.Keys.Count);
            foreach (KeyValuePair<Guid, List<RaceResult>> results in m_levelToResults)
            {
                writer.Write(results.Key);
                writer.Write(results.Value.Count);
                foreach (RaceResult result in results.Value)
                {
                    result.Serialize(writer);
                }
            }
        }

        protected override void OnDeserialize(DataReader reader, ushort version)
        {
            Guid = reader.Read<Guid>();
            Name = reader.ReadString();

            int levelCount = reader.Read<int>();
            for (int i = 0; i < levelCount; i++)
            {
                Guid levelGuid = reader.Read<Guid>();

                int resultCount = reader.Read<int>();
                List<RaceResult> results = new List<RaceResult>(resultCount);
                for (int j = 0; j < resultCount; j++)
                {
                    RaceResult result = new RaceResult(reader);

                    m_results.Add(result);
                    results.Add(result);
                }

                m_levelToResults.Add(levelGuid, results);
            }
        }
    }
}
