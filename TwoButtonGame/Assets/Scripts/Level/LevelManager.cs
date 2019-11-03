using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UnityEngine;

using Framework.AssetBundles;

namespace BoostBlasters.Levels
{
    /// <summary>
    /// Manages the levels.
    /// </summary>
    public static class LevelManager
    {
        private static Dictionary<Level, AssetBundle> m_loadedBundles = new Dictionary<Level, AssetBundle>();

        /// <summary>
        /// The available levels.
        /// </summary>
        public static Level[] Levels { get; private set; } = null;

        /// <summary>
        /// Gets a level by GUID.
        /// </summary>
        /// <param name="guid">The id of the level.</param>
        /// <returns>The level, or null if not found.</returns>
        public static Level GetByGUID(int guid)
        {
            foreach (Level level in Levels)
            {
                if (level.Id == guid)
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
            Level[] levels = await AssetBundleManager.LoadAssetsAsync<Level>("level", "config");

            // sort and assign the results
            Levels = levels.OrderBy(c => c.SortOrder).ToArray();
        }

        /// <summary>
        /// Loads a level asset bundle so that its scene can be loaded.
        /// </summary>
        /// <param name="level">The level to load.</param>
        /// <returns>The scene path that can be loaded upon completion.</returns>
        public static async Task<string> LoadLevelAsync(Level level)
        {
            AssetBundle bundle;

            // check if the bundle is already loaded
            if (m_loadedBundles.TryGetValue(level, out bundle))
            {
                Debug.Log($"Skipping load of level scene \"{level.SceneName}\" since it is already loaded.");
            }
            else
            {
                bundle = null;
                //bundle = await AssetBundleManager.LoadBundleAsync("levelscene", level.SceneName);
                //m_loadedBundles.Add(level, bundle);
            }

            // return the first scene path
            return bundle.GetAllScenePaths().FirstOrDefault();
        }

        /// <summary>
        /// Unloads all levels that are currently loaded.
        /// </summary>
        public static void UnloadAllLevels()
        {
            foreach (var levelBundlePair in m_loadedBundles)
            {
                levelBundlePair.Value.Unload(true);
            }

            m_loadedBundles.Clear();
        }

        /// <summary>
        /// Unloads a level and all of its assets.
        /// </summary>
        /// <param name="level">The level to load.</param>
        public static void UnloadLevel(Level level)
        {
            if (m_loadedBundles.TryGetValue(level, out AssetBundle bundle))
            {
                bundle.Unload(true);

                m_loadedBundles.Remove(level);
            }
            else
            {
                Debug.LogWarning($"Cannot unload scene \"{level.SceneName}\" since it is not loaded.");
            }
        }
    }
}
