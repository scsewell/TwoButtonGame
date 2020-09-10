using System;

using BoostBlasters.Characters;
using BoostBlasters.Profiles;

using UnityEngine;

namespace BoostBlasters.Races
{
    /// <summary>
    /// A class that stores the configuration of a racer in a race.
    /// </summary>
    public abstract class RacerConfig
    {
        /// <summary>
        /// The character used by the racer.
        /// </summary>
        public Character Character { get; }

        /// <summary>
        /// The profile of the racer.
        /// </summary>
        public Profile Profile { get; }

        /// <summary>
        /// The color associated with the racer.
        /// </summary>
        public Color Color { get; }

        /// <summary>
        /// Creates a new <see cref="RacerConfig"/> instance.
        /// </summary>
        /// <param name="character">The character used by the racer. Cannot be null.</param>
        /// <param name="profile">The profile used by the racer. Cannot be null.</param>
        /// <param name="color">The color associated with the racer.</param>
        protected RacerConfig(Character character, Profile profile, Color color)
        {
            if (character == null)
            {
                throw new ArgumentNullException(nameof(character));
            }
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            Character = character;
            Profile = profile;
        }
    }
}
