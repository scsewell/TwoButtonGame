using BoostBlasters.Players;
using BoostBlasters.Characters;
using BoostBlasters.Levels;

namespace BoostBlasters.Races
{
    public enum RacerType
    {
        /// <summary>
        /// A racer controlled by a local human.
        /// </summary>
        Player,
        /// <summary>
        /// An racer controlled by atrificial intelligence.
        /// </summary>
        AI,
        /// <summary>
        /// A racer controlled by replay data.
        /// </summary>
        Replay,
    }

    /// <summary>
    /// The configuration for a racer in a race.
    /// </summary>
    public class RacerConfig
    {
        /// <summary>
        /// The type of the racer.
        /// </summary>
        public RacerType Type { get; }

        /// <summary>
        /// The character used by this racer.
        /// </summary>
        public Character Character { get; }

        /// <summary>
        /// The profile of the racer.
        /// </summary>
        public Profile Profile { get; }

        /// <summary>
        /// The input for this racer.
        /// </summary>
        public PlayerBaseInput Input { get; }


        private RacerConfig(RacerType type, Character character, Profile profile, PlayerBaseInput input)
        {
            Type = type;
            Character = character;
            Profile = profile;
            Input = input;
        }

        /// <summary>
        /// Creates a human player racer config.
        /// </summary>
        /// <param name="character">The character used by this racer.</param>
        /// <param name="profile">The profile of the racer.</param>
        /// <param name="input">The input for this racer.</param>
        /// <returns>The generated racer config.</returns>
        public static RacerConfig CreatePlayer(Character character, Profile profile, PlayerBaseInput input)
        {
            return new RacerConfig(RacerType.Player, character, profile, input);
        }

        /// <summary>
        /// Creates an AI racer config.
        /// </summary>
        /// <param name="character">The character used by this racer.</param>
        /// <param name="profile">The profile of the racer.</param>
        /// <returns>The generated racer config.</returns>
        public static RacerConfig CreateAI(Character character, Profile profile)
        {
            return new RacerConfig(RacerType.AI, character, profile, null);
        }

        /// <summary>
        /// Creates a replay racer config.
        /// </summary>
        /// <param name="character">The character used by this racer.</param>
        /// <param name="profile">The profile of the racer.</param>
        /// <returns>The generated racer config.</returns>
        public static RacerConfig CreateReplay(Character character, Profile profile)
        {
            return new RacerConfig(RacerType.Replay, character, profile, null);
        }
    }

    /// <summary>
    /// The configuration of a race.
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
        /// The number of local players in the race.
        /// </summary>
        public int PlayerCount { get; }

        /// <summary>
        /// The number of AI players in the race.
        /// </summary>
        public int AICount { get; }

        /// <summary>
        /// The number of racers in the race.
        /// </summary>
        public int RacerCount => Racers.Length;

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

            for (int i = 0; i < racers.Length; i++)
            {
                switch (racers[i].Type)
                {
                    case RacerType.Player: 
                        PlayerCount++; 
                        break;
                    case RacerType.AI: 
                        AICount++; 
                        break;
                }
            }
        }
    }
}
