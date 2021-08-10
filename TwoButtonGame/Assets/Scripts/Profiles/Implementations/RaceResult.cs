using System.Collections.Generic;

using Framework;
using Framework.IO;

namespace BoostBlasters.Profiles
{
    /// <summary>
    /// Stores the details of how a racer placed on a race.
    /// </summary>
    public class RaceResult : SerializableData
    {
        protected override FourCC SerializerType { get; } = new FourCC("RARE");
        protected override ushort SerializerVersion => 1;


        private readonly List<float> m_lapTimes = new List<float>();

        /// <summary>
        /// Did the racer complete the race.
        /// </summary>
        public bool Finished { get; private set; }

        /// <summary>
        /// The placement rank (1 = first, etc).
        /// </summary>
        public int Rank { get; private set; }

        /// <summary>
        /// The time in seconds for each lap completed.
        /// </summary>
        public IReadOnlyList<float> LapTimes => m_lapTimes;

        /// <summary>
        /// The total time in seconds for all completed laps.
        /// </summary>
        public float FinishTime
        {
            get
            {
                var time = 0f;
                for (var i = 0; i < LapTimes.Count; i++)
                {
                    time += LapTimes[i];
                }
                return time;
            }
        }

        /// <summary>
        /// Creates a new <see cref="RaceResult"/> instance.
        /// </summary>
        /// <param name="finished">Did the racer complete the race.</param>
        /// <param name="rank">The placement rank (1 = first, etc).</param>
        /// <param name="lapTimes">The time in seconds for each lap completed.</param>
        public RaceResult(bool finished, int rank, List<float> lapTimes)
        {
            Finished = finished;
            Rank = rank;
            m_lapTimes.AddRange(lapTimes);
        }

        /// <summary>
        /// Deserializes a <see cref="RaceResult"/> instance.
        /// </summary>
        /// <param name="reader">A data reader at a serialized race result.</param>
        public RaceResult(DataReader reader)
        {
            Deserialize(reader);
        }

        protected override void OnSerialize(DataWriter writer)
        {
            writer.Write(Finished);
            writer.Write(Rank);

            writer.Write(LapTimes.Count);
            for (var i = 0; i < LapTimes.Count; i++)
            {
                writer.Write(LapTimes[i]);
            }
        }

        protected override void OnDeserialize(DataReader reader, ushort version)
        {
            Finished = reader.Read<bool>();
            Rank = reader.Read<int>();

            var lapTimesCount = reader.Read<int>();
            m_lapTimes.Capacity = lapTimesCount;
            for (var i = 0; i < lapTimesCount; i++)
            {
                m_lapTimes.Add(reader.Read<float>());
            }
        }
    }
}
