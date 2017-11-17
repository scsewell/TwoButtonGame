using System;
using System.Collections.Generic;
using System.Linq;

public class RaceResult
{
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

    public RaceResult(int rank, bool finished, List<float> lapTimes)
    {
        m_rank = rank;
        m_finished = finished;
        m_lapTimes = lapTimes;
    }

    public void Reset()
    {
        m_rank = 1;
        m_finished = false;
        m_lapTimes.Clear();
    }
}
