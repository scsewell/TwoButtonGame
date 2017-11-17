using System.IO;

public class RecordingInfo
{
    private FileInfo m_file;
    public FileInfo File { get { return m_file; } }

    private RaceParameters m_raceParams;
    public RaceParameters RaceParams { get { return m_raceParams; } }

    private RaceResult[] m_results;
    public RaceResult[] RaceResults { get { return m_results; } }

    public RecordingInfo(FileInfo file, RaceParameters raceParams, RaceResult[] results)
    {
        m_file = file;
        m_raceParams = raceParams;
        m_results = results;
    }
}
