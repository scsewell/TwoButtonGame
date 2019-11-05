using System.Collections.Generic;
using System.Linq;

using Framework.IO;

using BoostBlasters.Races;
using BoostBlasters.Levels;

namespace BoostBlasters.Players
{
    /// <summary>
    /// Stores information about a player.
    /// </summary>
    public class PlayerProfile
    {
        private long m_uniqueId;
        public long UniqueId => m_uniqueId;

        private bool m_isGuest;
        public bool IsGuest => m_isGuest;

        private string m_name;
        public string Name
        {
            get => m_name;
            set => m_name = value;
        }

        private List<RaceResult> m_allResults;
        public IReadOnlyList<RaceResult> AllResults => m_allResults;

        private Dictionary<int, List<RaceResult>> m_levelToResults;

        public PlayerProfile(long uniqueId, bool isGuest, string name)
        {
            m_uniqueId = uniqueId;
            m_isGuest = isGuest;
            m_name = name;

            m_allResults = new List<RaceResult>();
            m_levelToResults = new Dictionary<int, List<RaceResult>>();
        }

        public PlayerProfile(byte[] bytes)
        {
            BinaryReader reader = new BinaryReader(bytes);

            m_uniqueId = reader.ReadLong();
            m_isGuest = false;
            m_name = reader.ReadString();

            m_allResults = new List<RaceResult>();
            m_levelToResults = new Dictionary<int, List<RaceResult>>();

            int levelCount = reader.ReadInt();
            for (int i = 0; i < levelCount; i++)
            {
                int levelId = reader.ReadInt();
                List<RaceResult> results = new List<RaceResult>();
                int resultCount = reader.ReadInt();

                for (int j = 0; j < resultCount; j++)
                {
                    results.Add(new RaceResult(reader.ReadArray<byte>(), this));
                }

                m_allResults.AddRange(results);
                m_levelToResults.Add(levelId, results);
            }
        }

        public byte[] GetBytes()
        {
            BinaryWriter writer = new BinaryWriter();
            writer.WriteValue(m_uniqueId);
            writer.WriteValue(m_name);

            writer.WriteValue(m_levelToResults.Keys.Count);
            foreach (KeyValuePair<int, List<RaceResult>> results in m_levelToResults)
            {
                writer.WriteValue(results.Key);
                writer.WriteValue(results.Value.Count);
                foreach (RaceResult result in results.Value)
                {
                    writer.WriteArray(result.GetBytes());
                }
            }

            return writer.GetBytes();
        }

        public void AddRaceResult(Level level, RaceResult result)
        {
            if (result != null && result.Finished)
            {
                m_allResults.Add(result);

                List<RaceResult> levelResults;
                if (!m_levelToResults.TryGetValue(level.Id, out levelResults))
                {
                    levelResults = new List<RaceResult>();
                    m_levelToResults.Add(level.Id, levelResults);
                }
                levelResults.Add(result);

                PlayerProfileManager.SaveProfile(this);
            }
        }

        public List<RaceResult> GetRaceResults(Level level)
        {
            List<RaceResult> results;
            if (!m_levelToResults.TryGetValue(level.Id, out results))
            {
                results = new List<RaceResult>();
            }
            return results;
        }
    }
}
