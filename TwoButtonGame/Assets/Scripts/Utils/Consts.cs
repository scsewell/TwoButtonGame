using Framework.Audio;

namespace BoostBlasters
{
    /// <summary>
    /// A class that contains core game constants.
    /// </summary>
    public static class Consts
    {
        /// <summary>
        /// The maximum number of local (splitscreen) players.
        /// </summary>
        public const int MAX_LOCAL_PLAYERS = 4;

        /// <summary>
        /// The maximum number of races supported in a race.
        /// </summary>
        public const int MAX_RACERS = 12;

        /// <summary>
        /// The maximum number of laps that can be completed in a race.
        /// </summary>
        public const int MAX_LAP_COUNT = 20;
    }

    /// <summary>
    /// The paths where bundles of a specific type are stored relative to the 
    /// bundles directory.
    /// </summary>
    public static class Bundles
    {
        /// <summary>
        /// The path for bundles containing character.
        /// </summary>
        public const string CHARACTERS = "character";

        /// <summary>
        /// The path for bundles containing level descriptions.
        /// </summary>
        public const string LEVELS = "level";

        /// <summary>
        /// The path for bundles containing scenes referenced by level descriptions.
        /// </summary>
        public const string LEVEL_SCENES = "levelscene";

        /// <summary>
        /// The path for bundles containing <see cref="Music"/> instances.
        /// </summary>
        public const string MUSIC = "music";

        /// <summary>
        /// The path for bundles containing audio tracks referenced by <see cref="Music"/> instances.
        /// </summary>
        public const string MUSIC_DATA = "musicData";
    }
}
