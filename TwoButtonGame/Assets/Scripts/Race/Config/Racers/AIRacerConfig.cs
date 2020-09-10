using BoostBlasters.Characters;
using BoostBlasters.Profiles;

using UnityEngine;

namespace BoostBlasters.Races
{
    /// <summary>
    /// The <see cref="RacerConfig"/> for a racer controlled by AI.
    /// </summary>
    public class AIRacerConfig : RacerConfig
    {
        /// <summary>
        /// Creates a new <see cref="AIRacerConfig"/> instance.
        /// </summary>
        /// <param name="character">The character used by this racer. Cannot be null.</param>
        /// <param name="profile">The profile used by the AI. Cannot be null.</param>
        /// <param name="color">The color associated with the racer.</param>
        public AIRacerConfig(Character character, Profile profile, Color color) : base(character, profile, color)
        {
        }
    }
}
