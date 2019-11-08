using System;
using System.Linq;
using System.Threading.Tasks;

using Framework.AssetBundles;

namespace BoostBlasters.Levels
{
    /// <summary>
    /// Manages the levels.
    /// </summary>
    public static class LevelManager
    {
        /// <summary>
        /// The available levels.
        /// </summary>
        public static Level[] Levels { get; private set; } = null;

        /// <summary>
        /// Gets a level by GUID.
        /// </summary>
        /// <param name="guid">The guid of the level.</param>
        /// <returns>The level, or null if not found.</returns>
        public static Level GetByGUID(Guid guid)
        {
            foreach (Level level in Levels)
            {
                if (level.Guid == guid)
                {
                    return level;
                }
            }
            return null;
        }

        /// <summary>
        /// Loads the available levels asynchronously.
        /// </summary>
        public static async Task LoadLevelsAsync()
        {
            Level[] levels = await AssetBundleManager.LoadAssetsAsync<Level>("level", "level");

            // sort and assign the results
            Levels = levels.OrderBy(c => c.SortOrder).ToArray();
        }
    }
}
