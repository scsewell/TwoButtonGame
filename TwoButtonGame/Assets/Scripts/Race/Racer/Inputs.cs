using System;

using UnityEngine;

namespace BoostBlasters.Races.Racers
{
    /// <summary>
    /// The movements a racer can attempt to take.
    /// </summary>
    public struct Inputs
    {
        [Flags]
        private enum Commands : byte
        {
            None    = 0,
            Boost   = 1 << 0,
        }

        private sbyte m_h;
        private sbyte m_v;
        private Commands m_commands;

        /// <summary>
        /// The main stick horizontal input.
        /// </summary>
        public float H
        {
            get => UnpackFloat(m_h);
            set => m_h = PackFloat(value);
        }

        /// <summary>
        /// The main stick vertical input.
        /// </summary>
        public float V
        {
            get => UnpackFloat(m_v);
            set => m_v = PackFloat(value);
        }

        /// <summary>
        /// Is the racer attempting to boost.
        /// </summary>
        public bool Boost
        {
            get => m_commands.HasFlag(Commands.Boost);
            set 
            {
                if (value)
                {
                    m_commands |= Commands.Boost;
                }
                else
                {
                    m_commands &= ~Commands.Boost;
                }
            }
        }

        private static sbyte PackFloat(float value)
        {
            return (sbyte)Mathf.Clamp(Mathf.RoundToInt(value * sbyte.MaxValue), sbyte.MinValue, sbyte.MaxValue);
        }

        private static float UnpackFloat(sbyte value)
        {
            return (float)value / sbyte.MaxValue;
        }
    }
}
