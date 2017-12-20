using System;
using System.Collections.Generic;
using System.Linq;
using Framework.IO;

public class RaceResult
{
    private PlayerProfile m_profile;
    public PlayerProfile Profile
    {
        get { return m_profile; }
    }

    private int m_rank;
    public int Rank
    {
        get { return m_rank; }
        set { m_rank = value; }
    }

    private bool m_finished;
    public bool Finished
    {
        get { return m_finished; }
        set { m_finished = value; }
    }

    private List<float> m_lapTimes;
    public List<float> LapTimes { get { return m_lapTimes; } }

    public float FinishTime { get { return LapTimes.Sum(); } }

    public RaceResult()
    {
        m_lapTimes = new List<float>();
        Reset();
    }

    public RaceResult(byte[] bytes, PlayerProfile profile)
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
        writer.WriteValue(m_lapTimes.ToArray());
        return writer.GetBytes();
    }

    public void Reset()
    {
        m_rank = 1;
        m_finished = false;
        m_lapTimes.Clear();
    }
}
