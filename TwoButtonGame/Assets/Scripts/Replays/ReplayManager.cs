using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Framework;
using Framework.IO;

public class ReplayManager : Singleton<ReplayManager>
{
    private static readonly string FOLDER_NAME = "Replays/";
    private static readonly string FILE_EXTENTION = ".rep";

    private static readonly int MAX_REPLAYS = 100;

    public void SaveRecording(RaceRecording recording, List<Player> players)
    {
        string name = "Replay_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + FILE_EXTENTION;

        FileIO.WriteFile(recording.ToBytes(players), GetReplayDir() + name);
    }
    
    public List<ReplayInfo> GetReplays()
    {
        List<FileInfo> files = FileIO.GetFiles(GetReplayDir(), FILE_EXTENTION).ToList();
        files.Sort((x, y) => y.CreationTime.CompareTo(x.CreationTime));

        List<ReplayInfo> infos = new List<ReplayInfo>();

        foreach (FileInfo file in files)
        {
            if (file.Extension == FILE_EXTENTION)
            {
                if (infos.Count < MAX_REPLAYS)
                {
                    byte[] content = FileIO.ReadFileBytes(file.FullName);
                    Framework.IO.BinaryReader reader = new Framework.IO.BinaryReader(content);

                    RaceParameters raceParams = RaceRecording.ParseRaceParams(reader);
                    RaceResult[] raceResults = RaceRecording.ParseRaceResults(reader, raceParams.PlayerCount);

                    infos.Add(new ReplayInfo(file, raceParams, raceResults));
                }
                else
                {
                    File.Delete(file.FullName);
                }
            }
        }
        return infos;
    }

    public RaceRecording LoadReplay(ReplayInfo info)
    {
        return new RaceRecording(FileIO.ReadFileBytes(info.File.FullName));
    }

    private string GetReplayDir()
    {
        return FileIO.GetInstallDirectory() + FOLDER_NAME;
    }
}
