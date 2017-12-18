using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Framework;
using Framework.IO;

public class ReplayManager : Singleton<ReplayManager>
{
    private static readonly string FOLDER_NAME = "Replays/";
    private static readonly string FILE_EXTENTION = ".rep";

    private static readonly int MAX_REPLAYS = 100;
    
    private object m_replayLock;
    
    private List<ReplayInfo> m_replays;
    public List<ReplayInfo> Replays
    {
        get
        {
            lock (m_replayLock)
            {
                return new List<ReplayInfo>(m_replays);
            }
        }
    }

    private bool m_isRefreshing;
    public bool IsRefreshing
    {
        get
        {
            lock (m_replayLock)
            {
                return m_isRefreshing;
            }
        }
    }

    public ReplayManager()
    {
        m_replays = new List<ReplayInfo>();
        m_isRefreshing = false;
        m_replayLock = new object();
    }

    public async void RefreshReplays()
    {
        lock (m_replayLock)
        {
            m_replays.Clear();
            m_isRefreshing = true;
        }

        await Task.Run(() => GetReplays());

        lock (m_replayLock)
        {
            m_isRefreshing = false;
        }
    }
    
    private void GetReplays()
    {
        List<FileInfo> files = FileIO.GetFiles(GetReplayDir(), FILE_EXTENTION).ToList();
        files.Sort((x, y) => y.CreationTime.CompareTo(x.CreationTime));
        
        foreach (FileInfo file in files)
        {
            if (file.Extension == FILE_EXTENTION)
            {
                if (m_replays.Count < MAX_REPLAYS)
                {
                    byte[] content = FileIO.ReadFileBytes(file.FullName);
                    Framework.IO.BinaryReader reader = new Framework.IO.BinaryReader(content);

                    RaceParameters raceParams = RaceRecording.ParseRaceParams(reader);
                    RaceResult[] raceResults = RaceRecording.ParseRaceResults(reader, raceParams);

                    ReplayInfo info = new ReplayInfo(file, raceParams, raceResults);

                    lock (m_replayLock)
                    {
                        m_replays.Add(info);
                    }
                }
                else
                {
                    File.Delete(file.FullName);
                }
            }
        }
    }

    public RaceRecording LoadReplay(ReplayInfo info)
    {
        return new RaceRecording(FileIO.ReadFileBytes(info.File.FullName));
    }

    public void SaveRecording(RaceRecording recording, List<Player> players)
    {
        string name = "Replay_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + FILE_EXTENTION;

        FileIO.WriteFile(recording.ToBytes(players), GetReplayDir() + name);
    }

    private string GetReplayDir()
    {
        return FileIO.GetInstallDirectory() + FOLDER_NAME;
    }
}
