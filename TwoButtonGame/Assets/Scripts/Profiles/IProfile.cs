using System;

using UnityEngine;

namespace BoostBlasters.Profiles
{
    /// <summary>
    /// The interface implemented to provide information about a player.
    /// </summary>
    public interface IProfile
    {
        /// <summary>
        /// The unique identifier of this profile.
        /// </summary>
        Guid Guid { get; }

        /// <summary>
        /// The player name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The player color.
        /// </summary>
        Color Color { get; }
    }
}
