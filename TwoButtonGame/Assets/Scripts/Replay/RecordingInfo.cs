using System.IO;

using BoostBlasters.Races;
using BoostBlasters.Profiles;

namespace BoostBlasters.Replays
{
    /// <summary>
    /// Contains information about a race recording file.
    /// </summary>
    public class RecordingInfo
    {
        /// <summary>
        /// The display name of the recording.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The file the recording is contained in.
        /// </summary>
        public FileInfo File { get; }

        /// <summary>
        /// The configuration of the recorded race.
        /// </summary>
        public RaceParameters RaceParams { get; }

        /// <summary>
        /// The outcomes of the recorded race.
        /// </summary>
        public RaceResult[] RaceResults { get; }


        /// <summary>
        /// Creates a new recording info.
        /// </summary>
        /// <param name="file">The file the recording is contained in.</param>
        /// <param name="raceParams">The configuration of the race in the recording.</param>
        /// <param name="results">The outcomes of the race in the recording.</param>
        public RecordingInfo(FileInfo file, RaceParameters raceParams, RaceResult[] results)
        {
            File = file;
            RaceParams = raceParams;
            RaceResults = results;

            Name = Path.GetFileNameWithoutExtension(File.Name);
        }
    }
}
