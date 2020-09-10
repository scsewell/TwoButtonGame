using BoostBlasters.Characters;
using BoostBlasters.Profiles;

using UnityEngine;

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
        /// <param name="color">The color associated with the racer.</param>
        public ReplayRacerConfig(Character character, Profile profile, Color color) : base(character, profile, color)
        {
        }
    }
}
