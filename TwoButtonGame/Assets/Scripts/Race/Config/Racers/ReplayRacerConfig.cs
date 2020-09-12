using BoostBlasters.Characters;
using BoostBlasters.Profiles;

namespace BoostBlasters.Races
{
    /// <summary>
    /// The <see cref="RacerConfig"/> for a racer controlled by AI.
    /// </summary>
    public class ReplayRacerConfig : RacerConfig
    {
        /// <summary>
        /// Creates a new <see cref="ReplayRacerConfig"/> instance.
        /// </summary>
        /// <param name="character">The character used by this racer. Cannot be null.</param>
        /// <param name="profile">The profile used by the replay. Cannot be null.</param>
        public ReplayRacerConfig(Character character, IProfile profile) : base(character, profile)
        {
        }

        public override string ToString()
        {
            return $"Character: {Character}, Profile: {Profile}";
        }
    }
}
