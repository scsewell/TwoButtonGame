using System;
using System.Collections.Generic;
using System.Linq;
using Framework.IO;
using BoostBlasters.Players;

namespace BoostBlasters.Races
{
    public class RaceResult
    {
        private Profile m_profile;
        public Profile Profile => m_profile;

        private int m_rank;
        public int Rank
        {
            get => m_rank;
            set => m_rank = value;
        }

        private bool m_finished;
        public bool Finished
        {
            get => m_finished;
            set => m_finished = value;
        }

        private List<float> m_lapTimes;
        public List<float> LapTimes => m_lapTimes;

        public float FinishTime => LapTimes.Sum();

        public RaceResult(Profile profile)
        {
            m_profile = profile;
            m_lapTimes = new List<float>();
            Reset();
        }

        public RaceResult(byte[] bytes, Profile profile)
        {
            BinaryReader reader = new BinaryReader(bytes);
            m_rank = reader.ReadInt();
            m_finished = reader.ReadBool();
            m_lapTimes = reader.ReadArray<float>().ToList();

            m_profile = profile;
        }

        public byte[] GetBytes()
        {
            BinaryWriter writer = new BinaryWriter();
            writer.WriteValue(m_rank);
            writer.WriteValue(m_finished);
            writer.WriteArray(m_lapTimes.ToArray());
            return writer.GetBytes();
        }

        public void Reset()
        {
            m_rank = 1;
            m_finished = false;
            m_lapTimes.Clear();
        }
    }
}
