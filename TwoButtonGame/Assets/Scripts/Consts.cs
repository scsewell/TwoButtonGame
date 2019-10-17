using UnityEngine;

namespace BoostBlasters
{
    /// <summary>
    /// Application constants.
    /// </summary>
    public static class Consts
    {
        /// <summary>
        /// The maximum number of races supported.
        /// </summary>
        public const int MAX_RACERS = 12;

        /// <summary>
        /// The colors used for racers.
        /// </summary>
        private static readonly Color[] RACER_COLORS =
        {
            new Color(0.3f,  1f,     0.35f, 1f),
            new Color(1.0f,  0.25f,  0.25f, 1f),
            new Color(1.0f,  1.0f,   0.2f,  1f),
            new Color(0.75f, 0.5f,   1.0f,  1f),
            new Color(0.05f, 0.1f,   1.0f,  1f),
            new Color(1.0f,  0.5f,   0.0f,  1f),
            new Color(1.0f,  0.0f,   0.65f, 1f),
            new Color(0.1f,  1.0f,   1.0f,  1f),
            new Color(0.85f, 0.85f,  0.85f, 1f),
            new Color(0.3f,  0.3f,   0.3f,  1f),
            new Color(0.4f,  0.075f, 0.0f,  1f),
            new Color(0.6f,  1.0f,   0.1f,  1f),
        };

        /// <summary>
        /// Gets the color for a racer.
        /// </summary>
        /// <param name="racer">The index of this racer.</param>
        public static Color GetRacerColor(int racer)
        {
            return racer < MAX_RACERS ? RACER_COLORS[racer] : Color.black;
        }
    }
}
