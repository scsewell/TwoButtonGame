using System;
using System.Collections.Generic;

using UnityEngine;

namespace BoostBlasters.Profiles
{
    /// <summary>
    /// A non-persistent <see cref="IProfile"/> used by local players who do not want to
    /// create a local profile.
    /// </summary>
    public class GuestProfile : IProfile
    {
        private static readonly Color[] DEFAULT_COLORS =
        {
            Color.HSVToRGB(0.00f, 0.75f, 1f),
            Color.HSVToRGB(0.16f, 0.75f, 1f),
            Color.HSVToRGB(0.75f, 0.75f, 1f),
            Color.HSVToRGB(0.35f, 0.75f, 1f),
        };

        private static readonly NameRegistry s_nameRegistery = new NameRegistry();
        private static readonly HashSet<Color> s_colorRegistry = new HashSet<Color>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            s_nameRegistery.Reset();
            s_colorRegistry.Clear();
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
        /// Creates a new <see cref="GuestProfile"/> instance.
        /// </summary>
        public GuestProfile()
        {
            m_guid = Guid.NewGuid();
            m_name = s_nameRegistery.ReserveUniqueName("Guest", true);
            m_color = GetColor();

            s_colorRegistry.Add(m_color);
        }

        /// <summary>
        /// Release this profile.
        /// </summary>
        public void Release()
        {
            if (!m_released)
            {
                s_nameRegistery.ReleaseName(m_name);
                s_colorRegistry.Remove(m_color);
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

        private static Color GetColor()
        {
            foreach (var color in DEFAULT_COLORS)
            {
                if (!s_colorRegistry.Contains(color))
                {
                    return color;
                }
            }
            return ProfileUtils.GetRandomColor();
        }
    }
}
