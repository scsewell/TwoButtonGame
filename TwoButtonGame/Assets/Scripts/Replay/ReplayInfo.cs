using System.IO;

using BoostBlasters.Races;

namespace BoostBlasters.Replays
{
    /// <summary>
    /// Contains information about a replay file.
    /// </summary>
    public class ReplayInfo
    {
        private FileInfo m_file = null;
        public FileInfo File => m_file;

        private RaceParameters m_raceParams = null;
        public RaceParameters RaceParams => m_raceParams;

        private RaceResult[] m_results = null;
        public RaceResult[] RaceResults => m_results;

        public string Name => m_file.Name.Substring(0, m_file.Name.IndexOf('.'));

        public ReplayInfo(FileInfo file, RaceParameters raceParams, RaceResult[] results)
        {
            m_file = file;
            m_raceParams = raceParams;
            m_results = results;
        }
    }
}
