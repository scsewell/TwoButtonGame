using System.Collections;
using System.Linq;
using System.Threading.Tasks;

using UnityEngine;

namespace BoostBlasters.Characters
{
    public static class CharacterManager
    {
        /// <summary>
        /// The available characters.
        /// </summary>
        public static Character[] Characters { get; private set; } = null;

        /// <summary>
        /// Loads the available characters asynchronously.
        /// </summary>
        public static IEnumerator LoadAsync()
        {
            Task<Character[]> task = AssetBundleUtils.LoadBundledAssetsAsync<Character>("character", "config");

            // wait until all the files have loaded
            while (!task.IsCompleted)
            {
                yield return null;
            }

            // sort and assign the results
            Characters = task.Result.OrderBy(c => c.SortOrder).ToArray();

            foreach (Character level in Characters)
            {
                Debug.Log($"Loaded level \"{level.Name}\"");
            }
        }
    }
}
