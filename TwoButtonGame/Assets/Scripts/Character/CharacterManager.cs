using System;
using System.Linq;
using System.Threading.Tasks;

using Framework.AssetBundles;

namespace BoostBlasters.Characters
{
    /// <summary>
    /// Manages the playable characters.
    /// </summary>
    public static class CharacterManager
    {
        /// <summary>
        /// The available characters.
        /// </summary>
        public static Character[] Characters { get; private set; } = null;

        /// <summary>
        /// Gets a character by GUID.
        /// </summary>
        /// <param name="guid">The guid of the character.</param>
        /// <returns>The character, or null if not found.</returns>
        public static Character GetByGUID(Guid guid)
        {
            foreach (Character character in Characters)
            {
                if (character.Guid == guid)
                {
                    return character;
                }
            }
            return null;
        }

        /// <summary>
        /// Loads the available characters asynchronously.
        /// </summary>
        public static async Task LoadCharactersAsync()
        {
            Character[] characters = await AssetBundleManager.LoadAssetsAsync<Character>("character", "character");

            // sort and assign the results
            Characters = characters.OrderBy(c => c.SortOrder).ToArray();
        }
    }
}
