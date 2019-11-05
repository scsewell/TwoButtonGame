using System.Collections.Generic;

using BoostBlasters.Players;
using BoostBlasters.Characters;
using BoostBlasters.Levels;

namespace BoostBlasters.Races
{
    /// <summary>
    /// The configuration of a race.
    /// </summary>
    public class RaceParameters
    {
        public readonly Level level;
        public readonly int laps;

        public readonly int humanCount;
        public readonly int aiCount;
        public readonly int racerCount;
        public readonly List<Character> characters;
        public readonly List<Profile> profiles;
        public readonly List<PlayerBaseInput> inputs;
        public readonly List<int> playerIndicies;

        public RaceParameters(
            Level level,
            int laps,
            int humanCount,
            int aiCount,
            List<Character> characters,
            List<Profile> profiles,
            List<PlayerBaseInput> inputs,
            List<int> playerIndicies
        )
        {
            this.level = level;
            this.laps = laps;

            this.humanCount = humanCount;
            this.aiCount = aiCount;
            racerCount = humanCount + aiCount;

            this.characters = characters;
            this.profiles = profiles;
            this.inputs = inputs;
            this.playerIndicies = playerIndicies;
        }
    }
}
