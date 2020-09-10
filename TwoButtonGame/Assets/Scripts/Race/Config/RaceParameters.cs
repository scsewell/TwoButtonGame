using BoostBlasters.Levels;

namespace BoostBlasters.Races
{
    /// <summary>
    /// A class that stores the configuration of a race.
    /// </summary>
    public class RaceParameters
    {
        /// <summary>
        /// The level on which the race takes place.
        /// </summary>
        public Level Level { get; }

        /// <summary>
        /// The number of laps to be completed.
        /// </summary>
        public int Laps { get; }

        /// <summary>
        /// The racers taking part in the race.
        /// </summary>
        public RacerConfig[] Racers { get; }

        /// <summary>
        /// Creates a new race configuration.
        /// </summary>
        /// <param name="level">The level on which the race takes place.</param>
        /// <param name="laps">The number of laps to be completed.</param>
        /// <param name="racers">The racers taking part in the race.</param>
        public RaceParameters(Level level, int laps, RacerConfig[] racers)
        {
            Level = level;
            Laps = laps;
            Racers = racers;
        }
    }
}
