using System.Collections;
using System.Linq;
using System.Threading.Tasks;

using UnityEngine;

namespace BoostBlasters.Levels
{
    public static class LevelManager
    {
        /// <summary>
        /// The available levels.
        /// </summary>
        public static Level[] Levels { get; private set; } = null;

        /// <summary>
        /// Loads the available levels asynchronously.
        /// </summary>
        public static IEnumerator LoadAsync()
        {
            Task<Level[]> task = AssetBundleUtils.LoadBundledAssetsAsync<Level>("level", "config");

            // wait until all the files have loaded
            while (!task.IsCompleted)
            {
                yield return null;
            }

            // sort and assign the results
            Levels = task.Result.OrderBy(c => c.SortOrder).ToArray();

            foreach (Level level in Levels)
            {
                Debug.Log($"Loaded level \"{level.Name}\"");
            }
        }
    }
}
