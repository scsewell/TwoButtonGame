using System.Collections.Generic;

namespace BoostBlasters.Profiles
{
    /// <summary>
    /// A class used to get unique names.
    /// </summary>
    internal class NameRegistry
    {
        private readonly HashSet<string> m_usedNames = new HashSet<string>();

        /// <summary>
        /// Gets a unique name, appending a number if the requested name is already reserved.
        /// </summary>
        /// <param name="desiredName">The desired name.</param>
        /// <param name="alwaysAddNumber">Append a number to the name even if the desired
        /// name is already unique.</param>
        /// <returns>The unique name.</returns>
        public string ReserveUniqueName(string desiredName, bool alwaysAddNumber)
        {
            var i = 0;
            var name = alwaysAddNumber ? desiredName + (++i) : desiredName;

            while (m_usedNames.Contains(name))
            {
                i++;
                name = desiredName + i;
            }

            m_usedNames.Add(name);
            return name;
        }

        /// <summary>
        /// Make the given name reservable again.
        /// </summary>
        /// <param name="name">The name to unreserve.</param>
        public void ReleaseName(string name)
        {
            m_usedNames.Remove(name);
        }

        /// <summary>
        /// Make all names reservable.
        /// </summary>
        public void Reset()
        {
            m_usedNames.Clear();
        }
    }
}
