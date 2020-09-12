using System;

using UnityEngine;

namespace BoostBlasters.Profiles
{
    /// <summary>
    /// A <see cref="IProfile"/> used by AI racers.
    /// </summary>
    public class AIProfile : IProfile
    {
        private static readonly NameRegistry s_nameRegistery = new NameRegistry();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            s_nameRegistery.Reset();
        }


        private readonly Guid m_guid;
        private readonly string m_name;
        private readonly Color m_color;
        private bool m_released;

        /// <inheritdoc/>
        public Guid Guid
        {
            get
            {
                EnsureNotReleased();
                return m_guid;
            }
        }

        /// <inheritdoc/>
        public string Name
        {
            get
            {
                EnsureNotReleased();
                return m_name;
            }
        }

        /// <inheritdoc/>
        public Color Color
        {
            get
            {
                EnsureNotReleased();
                return m_color;
            }
        }

        /// <summary>
        /// Creates a new <see cref="AIProfile"/> instance.
        /// </summary>
        /// <param name="name">The desired name.</param>
        public AIProfile(string name)
        {
            m_guid = Guid.NewGuid();
            m_name = s_nameRegistery.ReserveUniqueName(name, true);
            m_color = ProfileUtils.GetRandomColor();
        }

        /// <summary>
        /// Release this profile.
        /// </summary>
        public void Release()
        {
            if (!m_released)
            {
                s_nameRegistery.ReleaseName(m_name);
                m_released = true;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        private void EnsureNotReleased()
        {
            if (m_released)
            {
                throw new ObjectDisposedException($"{GetType().Name}: {m_name}");
            }
        }
    }
}
