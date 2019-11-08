using System.Collections.Generic;

using Framework.IO;

namespace BoostBlasters.Races
{
    /// <summary>
    /// Stores the details of how a player placed on a race.
    /// </summary>
    public class RaceResult : SerializableData
    {
        private readonly char[] SERIALIZER_TYPE = new char[] { 'B', 'B', 'R', 'E' };

        protected override char[] SerializerType => SERIALIZER_TYPE;
        protected override ushort SerializerVersion => 1;


        /// <summary>
        /// The placement rank.
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Is this result from a completed race.
        /// </summary>
        public bool Finished { get; set; }

        /// <summary>
        /// The time achieved for each lap in seconds.
        /// </summary>
        public List<float> LapTimes { get; private set; }

        /// <summary>
        /// The total time taken to complete the current laps.
        /// </summary>
        public float FinishTime
        {
            get
            {
                float time = 0f;

                for (int i = 0; i < LapTimes.Count; i++)
                {
                    time += LapTimes[i];
                }

                return time;
            }
        }


        /// <summary>
        /// Creates a new race result.
        /// </summary>
        public RaceResult()
        {
            LapTimes = new List<float>();
            Reset();
        }

        /// <summary>
        /// Deserializes a race result.
        /// </summary>
        /// <param name="reader">A data reader at a serialized race result.</param>
        public RaceResult(DataReader reader)
        {
            Deserialize(reader);
        }

        /// <summary>
        /// Clears the result to the default value.
        /// </summary>
        public void Reset()
        {
            Rank = 1;
            Finished = false;
            LapTimes.Clear();
        }

        protected override void OnSerialize(DataWriter writer)
        {
            writer.Write(Rank);
            writer.Write(Finished);

            writer.Write(LapTimes.Count);
            for (int i = 0; i < LapTimes.Count; i++)
            {
                writer.Write(LapTimes[i]);
            }
        }

        protected override void OnDeserialize(DataReader reader, ushort version)
        {
            Rank = reader.Read<int>();
            Finished = reader.Read<bool>();

            int lapTimesCount = reader.Read<int>();
            LapTimes = new List<float>(lapTimesCount);
            for (int i = 0; i < lapTimesCount; i++)
            {
                LapTimes.Add(reader.Read<float>());
            }
        }
    }
}
