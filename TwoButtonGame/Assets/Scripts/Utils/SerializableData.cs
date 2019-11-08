using System;
using System.Linq;

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
        /// Serializes this data.
        /// </summary>
        /// <param name="writer">The writer to output to.</param>
        public void Serialize(DataWriter writer)
        {
            // write the serializer version
            writer.Write(SerializerType);
            writer.Write(SerializerVersion);

            // write the contents
            OnSerialize(writer);
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
        protected void Deserialize(DataReader reader)
        {
            // ensure that upcoming bytes are serialized data of this type
            char[] type = reader.Read<char>(SerializerType.Length);

            if (!SerializerType.SequenceEqual(type))
            {
                throw new Exception($"Serialized data is not a {GetType().Name}!");
            }

            // get the serialized format version
            ushort version = reader.Read<ushort>();

            OnDeserialize(reader, version);
        }

        /// <summary>
        /// Deserializes data and applies it to this instance.
        /// </summary>
        /// <param name="reader">The reader to get data from.</param>
        /// <param name="version">The serializer version used to write the upcoming data.</param>
        protected abstract void OnDeserialize(DataReader reader, ushort version);
    }
}
