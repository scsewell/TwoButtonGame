using System;
using System.Linq;
using System.Text;

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
        /// <param name="level">The level on which the race takes place. Cannot be null.</param>
        /// <param name="laps">The number of laps to be completed. Must be greater than 0 and no
        /// greater than <see cref="Consts.MAX_LAP_COUNT"/>.</param>
        /// <param name="racers">The racers taking part in the race. Cannot be null or contain
        /// any null or duplicate entries.</param>
        public RaceParameters(Level level, int laps, RacerConfig[] racers)
        {
            if (level == null)
            {
                throw new ArgumentNullException(nameof(level));
            }
            if (laps <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(laps), laps, "Must be greater than 0!");
            }
            if (laps > Consts.MAX_LAP_COUNT)
            {
                throw new ArgumentOutOfRangeException(nameof(laps), laps, "Max lap count exceeded!");
            }
            if (racers == null)
            {
                throw new ArgumentNullException(nameof(racers));
            }
            if (racers.Any(racer => racer == null))
            {
                throw new ArgumentException("Cannot contain any null configs!", nameof(racers));
            }
            if (racers.Distinct().Count() != racers.Length)
            {
                throw new ArgumentException("Cannot contain any duplicate configs!", nameof(racers));
            }

            Level = level;
            Laps = laps;
            Racers = racers;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("{");
            sb.AppendLine($"Level: {Level},");
            sb.AppendLine($"Laps: {Laps},");

            for (var i = 0; i < Racers.Length; i++)
            {
                sb.AppendLine($"Racer {i}: {Racers[i]},");
            }

            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
