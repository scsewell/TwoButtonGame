using System.IO;

public class ReplayInfo
{
    private FileInfo m_file;
    public FileInfo File { get { return m_file; } }

    private RaceParameters m_raceParams;
    public RaceParameters RaceParams { get { return m_raceParams; } }

    private RaceResult[] m_results;
    public RaceResult[] RaceResults { get { return m_results; } }
    
    public string Name
    {
        get { return m_file.Name.Substring(0, m_file.Name.IndexOf('.')); }
    }

    public ReplayInfo(FileInfo file, RaceParameters raceParams, RaceResult[] results)
    {
        m_file = file;
        m_raceParams = raceParams;
        m_results = results;
    }
}
