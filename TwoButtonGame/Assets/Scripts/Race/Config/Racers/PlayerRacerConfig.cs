using System;

using BoostBlasters.Characters;
using BoostBlasters.Input;
using BoostBlasters.Profiles;

using UnityEngine;

namespace BoostBlasters.Races
{
    /// <summary>
    /// The <see cref="RacerConfig"/> for a racer controlled by a local player.
    /// </summary>
    public class PlayerRacerConfig : RacerConfig
    {
        /// <summary>
        /// The input for this player.
        /// </summary>
        public UserInput Input { get; }

        /// <summary>
        /// Creates a new <see cref="PlayerRacerConfig"/> instance.
        /// </summary>
        /// <param name="character">The character used by this racer. Cannot be null.</param>
        /// <param name="profile">The profile used by the player. Cannot be null.</param>
        /// <param name="color">The color associated with the racer.</param>
        /// <param name="input">The input for this player. Cannot be null.</param>
        public PlayerRacerConfig(Character character, Profile profile, Color color, UserInput input) : base(character, profile, color)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            Input = input;
        }
    }
}
