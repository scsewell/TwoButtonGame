﻿using System;
using System.Linq;

using UnityEngine;

using Framework.IO;

namespace BoostBlasters
{
    /// <summary>
    /// Standardizes aspects of data serialization.
    /// </summary>
    public abstract class SerializableData
    {
        /// <summary>
        /// The header used to identify the type of data represented by the serialized contents.
        /// </summary>
        protected abstract char[] SerializerType { get; }

        /// <summary>
        /// The current version number of the serializer.
        /// </summary>
        protected abstract ushort SerializerVersion { get; }

        /// <summary>
        /// Was this instance successfully deserialized.
        /// </summary>
        public bool IsValid { get; private set; } = true;

        /// <summary>
        /// Serializes this data.
        /// </summary>
        /// <param name="writer">The writer to output to.</param>
        /// <returns>True if the serialization was successful.</returns>
        public bool Serialize(DataWriter writer)
        {
            try
            {
                // write the serializer version
                writer.Write(SerializerType.Select(c => (byte)c).ToArray());
                writer.Write(SerializerVersion);

                // write the contents
                OnSerialize(writer);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to serialize {GetType().Name}! {e.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// Serializes this data.
        /// </summary>
        /// <param name="writer">The writer to output to.</param>
        protected abstract void OnSerialize(DataWriter writer);

        /// <summary>
        /// Deserializes data and applies it to this instance.
        /// </summary>
        /// <param name="reader">The reader to get data from.</param>
        /// <returns>True if the deserialization was successful.</returns>
        protected bool Deserialize(DataReader reader)
        {
            try
            {
                // ensure that upcoming bytes are serialized data of this type
                byte[] type = reader.Read<byte>(SerializerType.Length);

                if (!SerializerType.Select(c => (byte)c).SequenceEqual(type))
                {
                    throw new Exception($"Serialized data is not a {GetType().Name}!");
                }

                // get the serialized format version
                ushort version = reader.Read<ushort>();

                OnDeserialize(reader, version);
                IsValid = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deserialize {GetType().Name}! {e.ToString()}");
                IsValid = false;
            }

            return IsValid;
        }

        /// <summary>
        /// Deserializes data and applies it to this instance.
        /// </summary>
        /// <param name="reader">The reader to get data from.</param>
        /// <param name="version">The serializer version used to write the upcoming data.</param>
        protected abstract void OnDeserialize(DataReader reader, ushort version);
    }
}
